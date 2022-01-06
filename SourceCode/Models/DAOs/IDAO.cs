using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBase.Models
{
    /// <summary>
    /// 多資料庫通用資料操作介面
    /// </summary>
    public interface IDAO
    {
        /// <summary>
        /// 資料庫連線字串
        /// </summary>
        public string connectString { get; set; }

        /// <summary>
        /// 新增待執行SQL到執行佇列
        /// </summary>
        /// <param name="sqlStr">SQL字串</param>
        /// <param name="parameters">具名參數陣列</param>
        public void AddExecuteItem(string sqlStr, IDataParameter[] parameters);
        /// <summary>
        /// 以交易方式依序執行SQL執行佇列中的項目,發生錯誤時會RollBack至執行第一個項目前的狀態,無論有無成功均會清除SQL執行列表中的所有項目
        /// </summary>
        /// <returns>是否完全執行成功</returns>
        public bool Execute();
        /// <summary>
        /// 依照SQL執行列表項目依序執行Query,並將結果存至DataSet
        /// </summary>
        /// <returns>System.Data.DataSet</returns>
        public DataSet Query();
        /// <summary>
        /// 傳入帶有@開頭參數的SQL與對應參數列表,回傳DB真正執行的SQL
        /// </summary>
        /// <param name="withParmSqlStr">帶有具名參數的SQL string</param>
        /// <param name="parameters">參數對應表</param>
        /// <returns></returns>
        public string CreateSqlStr(string withParmSqlStr, IDataParameter[] parameters);
    }

    /// <summary>
    /// SQL執行內容項目設定
    /// </summary>
    public class ExecuteItem
    {
        /// <summary>
        /// SQL指令
        /// </summary>
        public string sqlStr { get; set; }
        /// <summary>
        /// 該SQL指令對應參數表
        /// </summary>
        public IDataParameter[] parameterList { get; set; }

    }
}
