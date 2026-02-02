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

        public bool m_bIslistening = false;

        public string m_szName { set; get; }
        public int m_nPort { get; set; }
        public string m_szDescription { get; set; }

        public enListenerProtocol m_protocol { get; set; }

        #endregion
        #region Event

        /// <summary>
        /// Received bytes event handler.
        /// </summary>
        /// <param name="l">Listener object.</param>
        /// <param name="v">Victim object.</param>
        /// <param name="buffer">Received bytes buffer.</param>
        /// <param name="rec">Bytes size.</param>
        public delegate void ReceivedEventHandler(clsListener l, clsVictim v, (int Command, int Param, int DataLength, byte[] MessageData) buffer, int rec);
        public event ReceivedEventHandler Received; //Received bytes event.

        /// <summary>
        /// Decoded bytes event handler.
        /// </summary>
        /// <param name="l">Listener object.</param>
        /// <param name="v">Victim object.</param>
        /// <param name="aMsg">Decoded bytes data.</param>
        public delegate void ReceivedDecodedEventHandler(clsListener l, clsVictim v, List<string> Msg);
        public event ReceivedDecodedEventHandler ReceivedDecoded; //Decoded bytes event.

        /// <summary>
        /// Victim disconnect event handler.
        /// </summary>
        /// <param name="v">clsVictim object.</param>
        public delegate void DisconenctedEventHandler(clsVictim v);
        public event DisconenctedEventHandler Disconencted; //Disconnected event.

        /// <summary>
        /// Implant disconencted event handler.
        /// </summary>
        /// <param name="l">Listener object.</param>
        /// <param name="v">clsVictim object.</param>
        /// <param name="lsMsg">Message List.</param>
        public delegate void ImplantConnectedHandler(clsListener l, clsVictim v, List<string> lsMsg);
        public event ImplantConnectedHandler ImplantConnected;

        #endregion

        public clsListener()
        {

        }

        ~clsListener()
        {

        }

        public virtual void fnStart()
        {

        }

        public virtual void fnStop()
        {

        }

        public void fnOnListenerStarted(clsListener listener)
        {
            clsStore.sql_conn.WriteSystemLogs($"Listener is started(Name={listener.m_szName}, Port={listener.m_nPort}, Protocol={Enum.GetName(listener.m_protocol)})");
        }

        public void fnOnListenerStopped(clsListener listener)
        {
            clsStore.sql_conn.WriteSystemLogs($"Listener is stopped(Name={listener.m_szName})");
        }

        public void fnReceived(clsListener listener, clsVictim victim, (int nCommand, int nParam, int nDataLength, byte[] abMsg) buffer, int nRecv)
        {
            Received?.Invoke(listener, victim, buffer, nRecv);
        }

        public void fnReceivedDecoded(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            ReceivedDecoded?.Invoke(listener, victim, lsMsg);
        }

        public void fnDisconnected(clsVictim victim)
        {
            Disconencted?.Invoke(victim);
        }

        public void fnImplantConnected(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            ImplantConnected?.Invoke(listener, victim, lsMsg);
        }
    }
}
