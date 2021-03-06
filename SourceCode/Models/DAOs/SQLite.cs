using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using WebBase.Global;

namespace WebBase.Models
{
    /// <summary>
    /// SQLite DB存取工具
    /// </summary>
    public class SQLite : IDAO
    {
        string _connectString;
        /// <summary>
        /// 連線DB字串
        /// </summary>
        public string connectString
        {
            get
            {
                return _connectString;
            }
            set
            {
                _connectString = value;
            }
        }
        protected SqliteConnection conn;
        protected SqliteCommand cmd;

        /// <summary>
        /// 排定SQL執行列表項目
        /// </summary>
        protected List<ExecuteItem> ExecuteList { get; set; }

        /// <summary>
        /// 建構式:連線字串預設抓Web.config的connString值
        /// </summary>
        /// <param name="connStr">[可選]連線字串,未設定時抓web.config的connString</param>
        public SQLite([Optional] string connStr)
        {
            if (connStr == null)
            {
                connectString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            }
            else
            {
                connectString = connStr;
            }

            ExecuteList = new List<ExecuteItem>();
        }

        public void AddExecuteItem(string sqlStr, IDataParameter[] parameters)
        {
            ExecuteItem newItem = new ExecuteItem();
            newItem.sqlStr = sqlStr;
            newItem.parameterList = (SqliteParameter[])parameters;
            ExecuteList.Add(newItem);
        }

        public string CreateSqlStr(string withParmSqlStr, IDataParameter[] parameters)
        {
            string sqlStr = withParmSqlStr;

            foreach (var item in parameters)
            {
                //參數資料型態不為數字時轉換值須加引號
                if (item.DbType != DbType.Int32)
                {
                    item.Value = "'" + item.Value + "'";
                }

                sqlStr = sqlStr.Replace(item.ParameterName, item.Value.ToString());
            }

            return sqlStr;
        }

        public bool Execute()
        {
            bool result = false;

            using (conn = new SqliteConnection(connectString))
            {
                conn.Open();
                cmd = conn.CreateCommand();
                cmd.Connection = conn;
                cmd.Transaction = conn.BeginTransaction();

                try
                {
                    //逐筆執行佇列中內容
                    foreach (ExecuteItem item in ExecuteList)
                    {
                        cmd.CommandText = item.sqlStr;
                        if (item.parameterList is not null)
                        {
                            cmd.Parameters.AddRange(item.parameterList.ToArray());
                        }

                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    cmd.Transaction.Commit();
                }
                catch (Exception ex)
                {
                    cmd.Transaction.Rollback();
                    throw ex;
                }
            }
            result = true;

            ExecuteList.Clear();

            return result;
        }

        public DataSet Query()
        {
            DataSet ds = new DataSet();

            using (conn = new SqliteConnection(connectString))
            {
                conn.Open();

                foreach (ExecuteItem item in ExecuteList)
                {
                    using (cmd = new SqliteCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = item.sqlStr;
                        cmd.Parameters.Clear();

                        if (item.parameterList is not null)
                        {
                            cmd.Parameters.AddRange(item.parameterList);
                        }

                        using (IDataReader dr = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                        {
                            DataTable dt = new DataTable();

                            dt.Load(dr);
                            ds.Tables.Add(dt);
                        }
                    }
                }
            }

            ExecuteList.Clear();

            return ds;
        }
    }
}