using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using WebBase.Global;
using System.Configuration;
using System.Runtime.InteropServices;

namespace WebBase.Models
{
    /// <summary>
    /// 資料存取底層
    /// </summary>
    public class MySQL : IDAO
    {
        /// <summary>
        /// 連線字串
        /// </summary>
        string IDAO.connectString { get; set; }
        string connectString { get; set; }
        protected MySqlConnection conn;
        protected MySqlCommand cmd;

        /// <summary>
        /// 排定SQL執行列表項目
        /// </summary>
        protected List<ExecuteItem> ExecuteList { get; set; }

        /// <summary>
        /// 建構式:連線字串預設抓Web.config的connString值
        /// </summary>
        /// <param name="connStr">[可選]連線字串,未設定時抓web.config的connString</param>
        public MySQL([Optional] string connStr)
        {
            if(connStr == null)
            {
                connectString = ConfigurationManager.ConnectionStrings["connStr_MySQL"].ConnectionString;
            }
            else
            {
                connectString = connStr;
            }

            ExecuteList = new List<ExecuteItem>();
        }

        /// <summary>
        /// 新增SQL執行項目至列表
        /// </summary>
        /// <param name="sqlStr">要執行的SQL指令(包含@開頭的具名參數)</param>
        /// <param name="parameters">參數設定清單</param>
        public void AddExecuteItem(string sqlStr, IDataParameter[] parameters)
        {
            ExecuteItem newItem = new ExecuteItem();
            newItem.sqlStr = sqlStr;
            newItem.parameterList = (MySqlParameter[])parameters;
            ExecuteList.Add(newItem);
        }

        /// <summary>
        /// 以交易方式依序執行SQL執行列表項目,發生錯誤時會RollBack至執行第一個項目前的狀態,無論有無成功均會清除SQL執行列表中的所有項目
        /// </summary>
        /// <returns></returns>
        public bool Execute()
        {
            bool result = false;

            try
            {
                using (conn = new MySqlConnection(connectString))
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
            }
            catch (Exception ex)
            {
                result = false;
                //將Exception傳至上層由ExceptionHandler統一處理
                if (ex.InnerException != null)
                {
                    throw new CustomException(ex.InnerException);
                }
                else
                {
                    throw new CustomException(ex);
                }
            }
            finally
            {
                cmd.Dispose();
                ExecuteList.Clear();
                if (conn.State != ConnectionState.Closed) conn.Close();
            }

            return result;
        }

        /// <summary>
        /// 依照SQL執行列表項目依序執行Query,並將結果存至DataSet
        /// </summary>
        /// <returns></returns>
        public DataSet Query()
        {
            DataSet ds = new DataSet();

            try
            {
                using (conn = new MySqlConnection(connectString))
                {
                    conn.Open();

                    foreach (ExecuteItem item in ExecuteList)
                    {
                        using (cmd = new MySqlCommand())
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
            }
            catch (Exception ex)
            {
                //將Exception傳至上層由ExceptionHandler統一處理
                if (ex.InnerException != null)
                {
                    throw new CustomException(ex.InnerException);
                }
                else
                {
                    throw new CustomException(ex);
                }
            }
            finally
            {
                ExecuteList.Clear();
                if (conn.State != ConnectionState.Closed) conn.Close();
            }

            return ds;
        }
    }
}