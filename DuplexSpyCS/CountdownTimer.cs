using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DuplexSpyCS
{
    internal class CountdownTimer
    {
        private int m_nSecond;
        private System.Timers.Timer m_timer;

        public delegate void CountdownCompletedEventHandler();
        public event CountdownCompletedEventHandler CountdownCompleted;

        public CountdownTimer(int nSecond)
        {
            m_nSecond = nSecond;
            m_timer = new System.Timers.Timer(1000);
            m_timer.Elapsed += OnTimedEvent;
        }

        public void Start()
        {
            m_timer.Start();
        }
        public void Stop()
        {
            m_timer.Stop();
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (m_nSecond > 0)
            {
                m_nSecond--;
            }
            else
            {
                m_timer.Stop();
                CountdownCompleted?.Invoke();
            }
        }
    }
}
