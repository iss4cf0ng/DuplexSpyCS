using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    internal class FormMonitor
    {
        public event EventHandler<FormCountChangedEventArgs> FormCountChanged;

        public FormMonitor()
        {
            Application.OpenForms.Cast<Form>().ToList().ForEach(RegisterFormEvents);
        }

        private void RegisterFormEvents(Form form)
        {
            form.Load += (s, e) => OnFormCountChanged();
            form.FormClosed += (s, e) => OnFormCountChanged();
        }

        private void OnFormCountChanged()
        {
            FormCountChanged?.Invoke(this, new FormCountChangedEventArgs(Application.OpenForms.Count));
        }
    }

    public class FormCountChangedEventArgs : EventArgs
    {
        public int CurrentFormCount { get; }
        public FormCountChangedEventArgs(int form_cnt)
        {
            CurrentFormCount = form_cnt;
        }
    }
}
