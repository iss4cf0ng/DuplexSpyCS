using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace winClient48Small
{
    public class Server
    {
        public Socket socket;
        public static int MAX_BUFFER_LENGTH = 65536;
        public byte[] abBuffer = new byte[MAX_BUFFER_LENGTH];
        public string server_ip;
        public int server_port;

        public (string szXmlPublicKey, string szXmlPrivateKey) tpRsaKeyPair;
        public (byte[] abKey, byte[] abIV) tpAesKey;

        //Constructor
        public Server(Socket socket)
        {
            this.socket = socket;
        }

        public void encSend(int nCmd, int nParam, string szMsg)
        {
            string szCipher = Crypto.AESEncrypt(szMsg, tpAesKey.abKey, tpAesKey.abIV);
            Send(nCmd, nParam, szCipher);
        }

        public void Send(int nCmd, int nParam, byte[] abMsg)
        {

        }
        public void Send(int nCmd, int nParam, string szMsg)
        {

        }

        public void SendCommand(string szPayload)
        {

        }

        public void SendCommand(string[] aszPayload)
        {

        }
    }
}
