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
    /// 使用者設定功能頁面相關資料操作:繼承資料存取通用介面
    /// </summary>
    public class User : ISqlCreator
    {
        public string MasterTable { get { return "SYS_USER U"; } }

        /// <summary>
        /// 用JObject生成對應的SQL參數陣列
        /// </summary>
        /// <param name="input">前端輸入值</param>
        /// <returns></returns>
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
                if (name == "DATA_STATUS" || name == "AUTHORITY_STATUS")
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
                case "Count":
                    sqlStr = Search(parm, true);
                    break;
                case "Query":
                    sqlStr = Search(parm, false);
                    break;
                case "Add":
                    sqlStr = Insert();
                    break;
                case "Edit":
                    sqlStr = Update();
                    break;
                case "Delete":
                    sqlStr = Delete();
                    break;
                default:
                    break;
            }

            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--查詢
        /// </summary>
        /// <param name="parm">查詢條件參數</param>
        /// <param name="getCount">是否為查詢數量</param>
        /// <returns></returns>
        public string Search(JObject parm, bool getCount)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr;

            //是否為查詢資料筆數
            if (getCount)
            {
                sqlStr = string.Format(@"SELECT COUNT(*) 'Count' FROM {0} ", MasterTable);
            }
            else
            {
                sqlStr = string.Format(@"SELECT U.GUID,U.ID,E.ID AS EMPLOYEE_ID,U.INSERT_TIME,U.REMARK FROM {0} ", MasterTable);
            }

            //關聯-權限群組
            if (!string.IsNullOrEmpty((string)conditions.GROUP_ID))
            {
                sqlStr += "INNER JOIN SYS_USER_GROUP UG ON UG.USER_GUID = U.GUID ";
                sqlStr += "INNER JOIN SYS_GROUP G ON G.GUID = UG.GROUP_GUID ";
            }

            sqlStr += "LEFT JOIN HR_EMPLOYEE E ON E.GUID = U.EMPLOYEE_GUID ";
            sqlStr += "WHERE U.ENABLE = 1 ";

            //帳號
            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND U.ID LIKE CONCAT(@ID, '%') ";
            }
            //工號
            if (!string.IsNullOrEmpty((string)conditions.EMPLOYEE_ID))
            {
                sqlStr += "AND E.ID LIKE CONCAT(@EMPLOYEE_ID,'%') ";
            }
            //權限群組
            if (!string.IsNullOrEmpty((string)conditions.GROUP_ID))
            {
                sqlStr += "AND G.ID = @GROUP_ID ";
            }
            //建立時間-起
            if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_START))
            {
                sqlStr += "AND U.INSERT_TIME >= @INSERT_TIME_START ";
            }
            //建立時間-終
            if (!string.IsNullOrEmpty((string)conditions.INSERT_TIME_END))
            {
                sqlStr += "AND U.INSERT_TIME <= @INSERT_TIME_END ";
            }

            //查詢數量不需換頁
            if (getCount)
            {
                return sqlStr;
            }

            if (parm.TryGetValue("sort", out _))
            {
                sqlStr += string.Format("ORDER BY {0} {1} ", (string)conditions.sort, (string)conditions.order);
            }

            //含排序 or 換頁
            if (parm.TryGetValue("page", out _))
            {
                int offset = (int)conditions.rows * ((int)conditions.page - 1);
                sqlStr += string.Format("LIMIT {0} ", conditions.rows);
                sqlStr += string.Format("OFFSET {0}", offset);
            }
            sqlStr += ";";

            return sqlStr;
        }

        /// <summary>
        /// 取得唯一值
        /// </summary>
        /// <returns></returns>
        public string GetOneByGUID()
        {
            string sqlStr = string.Format(@"SELECT U.GUID,U.ID,U.PASSWORD,U.REMARK FROM {0} ", MasterTable);
            sqlStr += "WHERE ENABLE = 1 AND GUID = @GUID";

            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--新增
        /// </summary>
        /// <returns></returns>
        public string Insert()
        {
            string sqlStr = "INSERT INTO SYS_USER (GUID,ID,PASSWORD,REMARK,INSERT_USER) ";
            sqlStr += @"VALUES (UUID(),@ID,@PASSWORD,@REMARK,@INSERT_USER);";

            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--更新
        /// </summary>
        /// <returns></returns>
        public string Update()
        {
            string sqlStr = @"UPDATE SYS_USER SET 
                ID = @ID,
                PASSWORD = @PASSWORD,
                REMARK = @REMARK,
                UPDATE_USER = @UPDATE_USER,
                UPDATE_TIME = now()
                WHERE GUID = @GUID;";

            return sqlStr;
        }

        /// <summary>
        /// 回傳SQL指令--刪除
        /// </summary>
        /// <returns></returns>
        public string Delete()
        {
            string sqlStr = "UPDATE SYS_USER SET ENABLE = 0,UPDATE_USER = @UPDATE_USER,UPDATE_TIME = now() WHERE GUID = @GUID;";
            return sqlStr;
        }
    }
}