using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace WebBase.Models
{
    /// <summary>
    /// 儲存紀錄專用
    /// </summary>
    public class LOG : ISqlCreator
    {
        string actionTable { get { return "log_action"; } }
        string performanceTable { get { return "log_performance"; } }

        string noticeTable { get { return "log_notice"; } }

        string readedNoticeTable { get { return "log_notice_have_read"; } }

        string userTable { get { return "sys_user"; } }

        string apiLogTable { get { return "log_api"; } }

        public MySqlParameter[] CreateParameterAry(JObject input)
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
                if (name == "DATA_STATUS" || name == "AUTHORITY_STATUS" || name == "OFFSET")
                {
                    parm.DbType = System.Data.DbType.Int32;
                }
                else
                {
                    parm.DbType = System.Data.DbType.String;
                }

                parmList.Add(parm);
            }

            return parmList.ToArray();
        }

        public string GetSqlStr(string actionName, [Optional] JObject parm)
        {
            string sqlStr = string.Empty;

            switch (actionName)
            {
                case "InsertToActionLog":
                    sqlStr = InsertToActionLog();
                    break;
                case "InsertToPerformanceLog":
                    sqlStr = InsertToPerformanceLog();
                    break;
                case "GetTopOneUserGroup":
                    sqlStr = GetTopOneUserGroup();
                    break;
                case "GetTodayActionLogByUser":
                    sqlStr = GetTodayActionLogByUser();
                    break;
                case "AddNotice":
                    sqlStr = AddNotice();
                    break;
                case "AddReadNoticeLog":
                    sqlStr = AddReadNoticeLog();
                    break;
                case "QueryNoticeByUser":
                    sqlStr = QueryNoticeByUser();
                    break;
                case "InsertToApiLog":
                    sqlStr = InsertToApiLog();
                    break;
                default:
                    break;
            }

            return sqlStr;
        }

        /// <summary>
        /// 取得SQL:取得要存入權限群組欄位的群組ID
        /// </summary>
        /// <returns></returns>
        string GetTopOneUserGroup()
        {
            string sqlStr = "SELECT G.ID FROM sys_group G ";
            sqlStr += "INNER JOIN sys_user_group UG ON UG.GROUP_GUID = G.GUID ";
            sqlStr += "INNER JOIN sys_user U ON UG.USER_GUID = U.GUID ";
            sqlStr += "INNER JOIN sys_group_function GF ON GF.GROUP_GUID = G.GUID ";
            sqlStr += "INNER JOIN sys_function F ON GF.FUNCTION_GUID = F.GUID ";
            sqlStr += "WHERE U.ID = @USER_ID AND F.FUNCTION_ID = @FUNCTION_ID ";
            sqlStr += "ORDER BY G.ID LIMIT 1;";

            return sqlStr;
        }

        /// <summary>
        /// 取得SQL:存入使用者動作紀錄
        /// </summary>
        /// <returns></returns>
        string InsertToActionLog()
        {
            string sqlStr = $"INSERT INTO {actionTable} (GUID,INSERT_USER,USER_ID,USER_NAME,DEPARTMENT_ID,GROUP_ID,FUNCTION_ID,TARGET_ELEMENT,`ACTION`) ";
            sqlStr += "VALUES (UUID(),'SYSTEM',@USER_ID,@USER_NAME,@DEPARTMENT_ID,@GROUP_ID,@FUNCTION_ID,@TARGET_ELEMENT,@ACTION)";

            return sqlStr;
        }

        /// <summary>
        /// 取得SQL:存入SQL執行效能紀錄
        /// </summary>
        /// <returns></returns>
        string InsertToPerformanceLog()
        {
            string sqlStr = $"INSERT INTO {performanceTable} (GUID,INSERT_USER,USER_ID,GROUP_ID,FUNCTION_ID,`ACTION`,`SQL`,ELAPSED) ";
            sqlStr += "VALUES (UUID(),'SYSTEM',@USER_ID,@GROUP_ID,@FUNCTION_ID,@ACTION,@SQL,@ELAPSED)";

            return sqlStr;
        }

        /// <summary>
        /// 取得定使用者當日的動作紀錄
        /// </summary>
        /// <returns></returns>
        string GetTodayActionLogByUser()
        {
            string sqlStr = $"SELECT F.DOC_KEY,`ACTION`,TARGET_ELEMENT,A.INSERT_TIME FROM {actionTable} A ";
            sqlStr += "INNER JOIN sys_function F ON F.FUNCTION_ID = A.FUNCTION_ID ";
            sqlStr += "WHERE USER_ID = @USER_ID AND A.INSERT_TIME BETWEEN curdate() AND CONCAT(curdate(),' 23:59:59') ";
            sqlStr += "ORDER BY A.INSERT_TIME DESC;";

            return sqlStr;
        }

        /// <summary>
        /// 新增通知
        /// </summary>
        /// <returns></returns>
        string AddNotice()
        {
            string sqlStr = $"INSERT INTO {noticeTable} (GUID,INSERT_TIME,TYPE,TITLE,CONTENT) VALUES (UUID(),NOW(),@TYPE,@TITLE,@CONTENT);";
            return sqlStr;
        }

        /// <summary>
        /// 新增已讀通知紀錄
        /// </summary>
        /// <returns></returns>
        string AddReadNoticeLog()
        {
            string sqlStr = $"INSERT INTO {readedNoticeTable} (GUID,INSERT_TIME,NOTICE_GUID,USER_GUID) VALUES (UUID(),NOW(),@NOTICE_GUID,@USER_GUID);";
            return sqlStr;
        }

        /// <summary>
        /// 依使用者查詢通知
        /// </summary>
        /// <returns></returns>
        string QueryNoticeByUser()
        {
            string sqlStr = @$"SELECT * FROM(
    -- 未讀取
    SELECT N.GUID,N.INSERT_TIME,N.`TYPE`,N.TITLE,N.CONTENT,'N' AS 'READED'
    FROM {noticeTable} N
    WHERE N.GUID NOT IN(
        SELECT NR.NOTICE_GUID FROM {readedNoticeTable}
        INNER JOIN {readedNoticeTable} NR ON NR.NOTICE_GUID = N.GUID
        INNER JOIN {userTable} U ON U.GUID = NR.USER_GUID
        WHERE NR.USER_GUID = @USER_GUID
    )
    UNION
    -- 今日已讀取
    SELECT N.GUID,N.INSERT_TIME,N.TYPE,N.TITLE,N.CONTENT,'Y' AS 'READED'
    FROM {noticeTable} N
    INNER JOIN {readedNoticeTable} NR ON NR.NOTICE_GUID = N.GUID
    INNER JOIN {userTable} U ON U.GUID = NR.USER_GUID
    WHERE NR.USER_GUID = @USER_GUID
    AND N.INSERT_TIME BETWEEN TIMESTAMP(CURDATE()) AND date_add(CURDATE(), interval 24 * 60 * 60 - 1 second)
    ) a ORDER BY INSERT_TIME DESC; ";
            return sqlStr;
        }

        /// <summary>
        /// 新增API LOG
        /// </summary>
        /// <returns></returns>
        string InsertToApiLog()
        {
            string sqlStr = $"INSERT INTO {apiLogTable} (GUID,INSERT_TIME,API_HOST_URL,API_ACTION_NAME,SEND_DATA,RESPONSE_DATA) VALUES (UUID(),NOW(),@API_HOST_URL,@API_ACTION_NAME,@SEND_DATA,@RESPONSE_DATA);";
            return sqlStr;
        }
    }
}