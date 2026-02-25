using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace test_audio
{
    internal class Program
    {
        [DllImport("winmm.dll")]
        private static extern MMRESULT waveInOpen(ref IntPtr hWaveIn, int uDeviceID, ref WAVEFORMATEX lpFormat, WaveInProc dwCallback, IntPtr dwInstance, int dwFlags);

        [DllImport("winmm.dll")]
        private static extern MMRESULT waveInPrepareHeader(IntPtr hWaveIn, ref WAVEHDR lpWaveInHdr, int uSize);

        [DllImport("winmm.dll")]
        private static extern MMRESULT waveInAddBuffer(IntPtr hWaveIn, ref WAVEHDR lpWaveInHdr, int uSize);

        [DllImport("winmm.dll")]
        private static extern MMRESULT waveInStart(IntPtr hWaveIn);

        [DllImport("winmm.dll")]
        private static extern MMRESULT waveInStop(IntPtr hWaveIn);

        [DllImport("winmm.dll")]
        private static extern MMRESULT waveInReset(IntPtr hWaveIn);

        [DllImport("winmm.dll")]
        private static extern MMRESULT waveInUnprepareHeader(IntPtr hWaveIn, ref WAVEHDR lpWaveInHdr, int uSize);

        [DllImport("winmm.dll")]
        private static extern MMRESULT waveInClose(IntPtr hWaveIn);

        [DllImport("winmm.dll")]
        private static extern uint waveInGetNumDevs();

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public uint nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEHDR
        {
            public IntPtr lpData;
            public uint dwBufferLength;
            public uint dwBytesRecorded;
            public IntPtr dwUser;
            public uint dwFlags;
            public uint dwLoops;
            public IntPtr lpNext;
            public IntPtr reserved;
        }

        private delegate void WaveInProc(IntPtr hwi, uint uMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);

        private enum MMRESULT: uint
        {
            MMSYSERR_NOERROR = 0, //NO ERROR
        }

        static void RecordMicrophoneAudio()
        {
            IntPtr waveInHandle = IntPtr.Zero;

            // Define the wave format
            WAVEFORMATEX waveFormat = new WAVEFORMATEX
            {
                wFormatTag = 1,           // WAVE_FORMAT_PCM
                nChannels = 1,            // Mono
                nSamplesPerSec = 44100,   // 44.1 kHz
                wBitsPerSample = 16,      // 16-bit
                nBlockAlign = 2,          // Channels * (BitsPerSample / 8)
                nAvgBytesPerSec = 88200,  // SampleRate * BlockAlign
                cbSize = 0                // Extra size (0 for PCM)
            };

            // Prepare a wave header
            WAVEHDR waveHeader = new WAVEHDR
            {
                lpData = Marshal.AllocHGlobal(44100), // Allocate enough memory for the buffer
                dwBufferLength = 44100,              // Match the allocated buffer size
                dwFlags = 0
            };

            // Prepare the header
            waveInPrepareHeader(waveInHandle, ref waveHeader, Marshal.SizeOf(waveHeader));

            // Start recording
            waveInAddBuffer(waveInHandle, ref waveHeader, Marshal.SizeOf(waveHeader));
            waveInStart(waveInHandle);

            Console.WriteLine("Recording... Press Enter to stop.");
            Console.ReadLine();

            // Stop recording
            waveInStop(waveInHandle);
            waveInReset(waveInHandle);
            waveInUnprepareHeader(waveInHandle, ref waveHeader, Marshal.SizeOf(waveHeader));
            waveInClose(waveInHandle);

            Marshal.FreeHGlobal(waveHeader.lpData);
            Console.WriteLine("Recording stopped.");
        }

        static void Main(string[] args)
        {
            Console.WriteLine(waveInGetNumDevs());
            Console.ReadKey();

            Console.WriteLine("Recording microphone audio...");
            RecordMicrophoneAudio();
            Console.ReadKey();
        }
    }
}
