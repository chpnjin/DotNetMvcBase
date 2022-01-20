using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebBase.Models
{
    /// <summary>
    /// 介面:各功能頁面存取資料建立參數列表與生成SQL String通用
    /// </summary>
    public interface ISqlCreator
    {
        /// <summary>
        /// 將來自前端的JSON格式Key/Value參數轉為MySqlParameter陣列
        /// </summary>
        /// <param name="input">JSON格式Key/Value參數</param>
        /// <returns>MySqlParameter陣列</returns>
        public IDataParameter[] CreateParameterAry(JObject input);
        /// <summary>
        /// 取得SQL字串
        /// </summary>
        /// <param name="actionName">動作名稱</param>
        /// <param name="parm">前端傳入參數</param>
        /// <returns>可包含具名參數的SQL字串</returns>
        public string GetSqlStr(string actionName, [Optional] JObject parm);

        /// <summary>
        /// 傳入帶有具名參數的SQL字串與參數設定陣列,回傳DB真正執行的SQL
        /// </summary>
        /// <param name="withParmSqlStr">帶有具名參數的SQL string</param>
        /// <param name="parameters">參數對應表</param>
        /// <returns></returns>
        public string CreateSqlStr(string withParmSqlStr, IDataParameter[] parameters);
    }
}
