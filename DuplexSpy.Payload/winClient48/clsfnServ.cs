using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace winClient48
{
    internal class clsfnServ
    {
        public clsfnServ()
        {

        }

        public string ServiceControl(string[] names, string str_status, bool resume = false)
        {
            int code = 0;
            string msg = "?";

            ServiceControllerStatus status = (ServiceControllerStatus)Enum.Parse(typeof(ServiceControllerStatus), str_status);
            try
            {
                foreach (string name in names)
                {
                    using (ServiceController service = new ServiceController(name))
                    {
                        switch (status)
                        {
                            case ServiceControllerStatus.Running:
                                if (resume)
                                {
                                    service.Continue();
                                }
                                else
                                {
                                    service.Start();
                                }
                                break;
                            case ServiceControllerStatus.Stopped:
                                service.Stop();
                                break;
                            case ServiceControllerStatus.Paused:
                                service.Pause();
                                break;
                        }

                        msg = service.Status.ToString();
                    }
                }

                code = 1;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return $"{code}|{clsCrypto.b64E2Str(string.Join(",", names.Select(x => clsCrypto.b64E2Str(x))))}|{clsCrypto.b64E2Str(msg)}";
        }
    }
}
