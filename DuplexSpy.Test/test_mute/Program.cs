using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using NAudio.Wasapi;

namespace test_mute
{
    internal class Program
    {

        static void Main(string[] args)
        {
            try
            {
                // Create a new instance of MMDeviceEnumerator
                var deviceEnumerator = new MMDeviceEnumerator();

                // Get the default audio endpoint (playback device)
                var defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                // Get the AudioEndpointVolume instance
                var audioEndpointVolume = defaultDevice.AudioEndpointVolume;

                audioEndpointVolume.OnVolumeNotification += (e) =>
                {
                    if (!e.Muted) // If the device is unmuted
                    {
                        Console.WriteLine("Detected unmuted state. Enforcing mute...");
                        audioEndpointVolume.Mute = true; // Enforce mute
                        
                    }
                    audioEndpointVolume.MasterVolumeLevelScalar = 0.0f;
                };

                // Mute the device
                audioEndpointVolume.Mute = true;
                Console.WriteLine("The default audio device has been muted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
    }
}
