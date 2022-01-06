using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;

namespace WebBase.Models
{
    /// <summary>
    /// 針對Table:SYS_PARAM的資料操作
    /// </summary>
    public class SYS_PARAM : ISqlCreator
    {
        string TableName
        {
            get
            {
                return "SYS_PARAM";
            }
        }

        public IDataParameter[] CreateParameterAry(JObject input)
        {
            List<MySqlParameter> parmList = new List<MySqlParameter>();

            //JSON項目逐一加入參數表中
            foreach (var x in input)
            {
                MySqlParameter parm = new MySqlParameter();
                string name = x.Key;
                JToken value = x.Value;

                parm.ParameterName = "@" + name;
                parm.Value = value;
                //指定參數資料型態
                if (name == "DATA_STATUS" || name == "AUTHORITY_STATUS")
                {
                    parm.DbType = System.Data.DbType.Int32;
                }
                else if (name == "NOT_AUTHORITY")
                {
                    parm.DbType = System.Data.DbType.Boolean;
                }
                else if (name == "GUID")
                {
                    parm.DbType = System.Data.DbType.Guid;
                }
                else
                {
                    parm.DbType = System.Data.DbType.String;
                }

                parmList.Add(parm);
            }

            return parmList.ToArray();
        }

        public string GetSqlStr(string actionName,[Optional] JObject parm)
        {
            string sqlStr = string.Empty;

            switch (actionName)
            {
                case "GetDropDownListItems":
                    sqlStr = GetDropDownListItems();
                    break;
                case "GetSysParamList":
                    sqlStr = GetSysParamList();
                    break;
                default:
                    break;
            }

            return sqlStr;
        }

        /// <summary>
        /// 取得下拉選單項目
        /// </summary>
        /// <returns></returns>
        string GetDropDownListItems()
        {
            string sqlStr = $"SELECT `VALUE`,TEXT FROM {TableName} ";
            sqlStr += "WHERE `FUNCTION` = @FUNCTION AND FILTER_KEY = @FILTER_KEY ORDER BY TEXT;";

            return sqlStr;
        }

        /// <summary>
        /// 取得參數列表
        /// </summary>
        /// <returns></returns>
        public string GetSysParamList()
        {
            string sqlStr = $"SELECT FILTER_KEY,`TEXT`,`VALUE`,`TYPE` FROM {TableName} ";
            sqlStr += "WHERE (`FUNCTION` = @FUNCTION) AND (FILTER_KEY COLLATE utf8mb4_general_ci LIKE CONCAT(@FILTER_KEY,'%')) AND `ENABLE` = 1";

            return sqlStr;
        }
    }
}