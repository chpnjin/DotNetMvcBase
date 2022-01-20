using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace WebBase.Models
{
    /// <summary>
    /// 針對Login的頁面對DB做操作
    /// </summary>
    class Login : ISqlCreator
    {
        string login { get { return "LOG_LOGIN"; } }
        string loginHist { get { return "LOG_LOGIN_HIST"; } }

        public IDataParameter[] CreateParameterAry(JObject input)
        {
            if (input is null)
            {
                return new MySqlParameter[] { };
            }

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
                switch (name)
                {
                    default:
                        parm.DbType = System.Data.DbType.String;
                        break;
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
                case "LoginAuthenticator":
                    sqlStr = LoginAuthenticator();
                    break;
                case "SaveLogin":
                    sqlStr = SaveLoginInfo();
                    break;
                case "SaveLoginHist":
                    sqlStr = SaveLoginHistory();
                    break;
                case "ClearLoginInfo":
                    sqlStr = ClearLoginInfo();
                    break;
                case "SaveLogoutInfo":
                    sqlStr = SaveLoginHistory();
                    break;
                case "GetCurrentSingInIpCount":
                    sqlStr = GetCurrentSingInIpCount();
                    break;
                case "GetOneLoginInfoByGuid":
                    sqlStr = GetOneLoginInfoByGuid();
                    break;
                case "GetLoginInfoByIP":
                    sqlStr = GetLoginInfoByIP();
                    break;
                default:
                    break;
            }

            return sqlStr;
        }

        /// <summary>
        /// 使用者驗證
        /// </summary>
        /// <param name="id">使用者帳號</param>
        /// <param name="password">密碼</param>
        /// <returns>使用者名稱</returns>
        string LoginAuthenticator()
        {
            string sqlStr = "SELECT U.GUID ,U.ID,E.NAME ,D.NAME AS DEPART ,T.NAME AS TITLE ";
            sqlStr += "FROM SYS_USER U ";
            sqlStr += "LEFT JOIN HR_EMPLOYEE E ON E.GUID = U.EMPLOYEE_GUID ";
            sqlStr += "LEFT JOIN HR_EMPLOYEE_DEPARTMENT ED ON ED.EMPLOYEE_GUID = E.GUID ";
            sqlStr += "LEFT JOIN HR_EMPLOYEE_TITLE ET ON ET.EMPLOYEE_GUID = E.GUID ";
            sqlStr += "LEFT JOIN HR_DEPARTMENT D ON D.GUID = ED.DEPARTMENT_GUID ";
            sqlStr += "LEFT JOIN HR_TITLE T ON T.GUID = ET.TITLE_GUID ";
            sqlStr += "WHERE U.ENABLE = 1 AND U.ID = @Account AND U.PASSWORD = @Password ";
            sqlStr += "ORDER BY D.ID,T.ID LIMIT 1;";

            return sqlStr;
        }

        /// <summary>
        /// 儲存登入資訊
        /// </summary>
        /// <param name="ToHistoryTable">是否存至歷史紀錄表</param>
        /// <returns></returns>
        string SaveLoginInfo()
        {
            string sqlStr = $"INSERT INTO { login }";
            sqlStr += "(GUID,INSERT_USER,INSERT_TIME,USER_GUID,CLIENT_IP,`ACTION`) VALUES ";
            sqlStr += "(@GUID,@INSERT_USER,@INSERT_TIME,@USER_GUID,@CLIENT_IP,@ACTION);";

            return sqlStr;
        }
        /// <summary>
        /// 儲存登入歷史紀錄
        /// </summary>
        /// <returns></returns>
        string SaveLoginHistory()
        {
            string sqlStr = $"INSERT INTO { loginHist } ";
            sqlStr += "(GUID,INSERT_USER,INSERT_TIME,USER_GUID,CLIENT_IP,`ACTION`) VALUES ";
            sqlStr += "(uuid(),@INSERT_USER,@INSERT_TIME,@USER_GUID,@CLIENT_IP,@ACTION);";

            return sqlStr;
        }

        /// <summary>
        /// 清除登入資訊
        /// </summary>
        /// <returns></returns>
        string ClearLoginInfo()
        {
            string sqlStr = $"DELETE FROM { login } WHERE GUID = @LoginGuid;";

            return sqlStr;
        }

        /// <summary>
        /// 取得目前系統登入人數
        /// </summary>
        /// <returns></returns>
        string GetCurrentSingInIpCount()
        {
            string sqlStr = string.Empty;

            sqlStr += $"SELECT COUNT(DISTINCT CLIENT_IP) AS IP_COUNT FROM {login};";

            return sqlStr;
        }

        /// <summary>
        /// 取得單筆登入資訊
        /// </summary>
        /// <returns></returns>
        string GetOneLoginInfoByGuid()
        {
            string sqlStr = string.Empty;
            sqlStr += $"SELECT * FROM {login} WHERE GUID = @GUID";

            return sqlStr;
        }

        /// <summary>
        /// 取得IP登入紀錄
        /// </summary>
        /// <returns></returns>
        private string GetLoginInfoByIP()
        {
            string sqlStr = string.Empty;
            sqlStr += $"SELECT COUNT(*) FROM {login} WHERE CLIENT_IP = @IP;";

            return sqlStr;
        }

        public string CreateSqlStr(string withParmSqlStr, IDataParameter[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}