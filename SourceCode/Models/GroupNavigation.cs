using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace WebBase.Models
{
    public class GroupNavigation : ISqlCreator
    {
        public string TableA { get { return "SYS_GROUP"; } }
        public string TableB { get { return "SYS_NAVIGATION"; } }
        public string BindTable { get { return "SYS_GROUP_NAVIGATION"; } }

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

                switch (name)
                {
                    case "ID":
                    case "NAME":
                        parm.Value = value;
                        parm.DbType = System.Data.DbType.String;
                        break;
                    default:
                        parm.Value = value;
                        parm.DbType = System.Data.DbType.String;
                        break;
                }

                parmList.Add(parm);
            }

            return parmList.ToArray();
        }

        /// <summary>
        /// 建立權限群組具名參數表
        /// </summary>
        /// <returns></returns>
        public MySqlParameter[] CreateGroupsParameter(JToken input)
        {
            List<MySqlParameter> parmList = new List<MySqlParameter>();

            foreach (var item in input)
            {
                MySqlParameter parm = new MySqlParameter();
                parm.ParameterName = "@" + item.Value<string>("ID");
                parm.Value = item.Value<string>("ID");
                parm.MySqlDbType = MySqlDbType.VarChar;
                parm.DbType = DbType.String;

                parmList.Add(parm);
            }

            return parmList.ToArray();
        }

        public string GetSqlStr(string actionName, [Optional] JObject parm)
        {
            string sqlStr = string.Empty;

            switch (actionName)
            {
                case "CountA":
                    sqlStr = QueryA(parm, true);
                    break;
                case "SearchA":
                    sqlStr = QueryA(parm, false);
                    break;
                case "QueryBind":
                    sqlStr = QueryBind();
                    break;
                case "InsertBind":
                    sqlStr = InsertBind();
                    break;
                case "DeleteBind":
                    sqlStr = DeleteBind();
                    break;
            }

            return sqlStr;
        }

        public string QueryA(JObject parm, bool getCount)
        {
            dynamic conditions = parm as dynamic;
            string sqlStr;

            //是否為查詢資料筆數
            if (getCount)
            {
                sqlStr = string.Format(@"SELECT COUNT(*) `Count` FROM {0} ", TableA);
            }
            else
            {
                sqlStr = string.Format(@"SELECT GUID,ID,NAME,REMARK FROM {0} ", TableA);
            }

            sqlStr += "WHERE ENABLE = 1 ";

            if (!string.IsNullOrEmpty((string)conditions.ID))
            {
                sqlStr += "AND ID LIKE CONCAT(@ID , '%') ";
            }

            if (!string.IsNullOrEmpty((string)conditions.NAME))
            {
                sqlStr += "AND NAME LIKE CONCAT(@NAME , '%') ";
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

        public string QueryBind()
        {
            string sqlStr = string.Format("SELECT BIND.GUID,B.FUNCTION_ID,B.DOC_KEY FROM {0} BIND ", BindTable);
            sqlStr += string.Format("INNER JOIN {0} A ON A.GUID = BIND.GROUP_GUID ", TableA);
            sqlStr += string.Format("INNER JOIN {0} B ON B.GUID = BIND.NAVIGATION_GUID ", TableB);
            sqlStr += "WHERE BIND.ENABLE = 1 AND BIND.GROUP_GUID = @GROUP_GUID ";

            return sqlStr;
        }

        public string InsertBind()
        {
            string sqlStr = string.Format("INSERT INTO {0} (GUID,GROUP_GUID,NAVIGATION_GUID,INSERT_USER) ", BindTable);
            sqlStr += @"VALUES (UUID(),@GROUP_GUID,@NAVIGATION_GUID,@INSERT_USER);";

            return sqlStr;
        }

        public string DeleteBind()
        {
            string sqlStr = string.Format("UPDATE {0} SET ENABLE = 0 WHERE GUID = @GUID ", BindTable);

            return sqlStr;
        }

        /// <summary>
        /// 依權限取得對應可用的功能項目
        /// </summary>
        /// <returns></returns>
        public string GetItemsByUserGroup(MySqlParameter[] groups)
        {
            string sqlStr = $"SELECT N.URL,N.`INDEX`,N.DOC_KEY FROM {TableB} N ";
            sqlStr += $"INNER JOIN {BindTable} GN ON GN.NAVIGATION_GUID = N.GUID ";
            sqlStr += $"INNER JOIN {TableA} G ON GN.GROUP_GUID = G.GUID ";
            sqlStr += "WHERE N.`ENABLE` = true AND G.ID IN (";

            foreach (var item in groups)
            {
                sqlStr += item.ParameterName;

                if (item != groups.Last())
                {
                    sqlStr += ",";
                }
            }

            sqlStr += ");";

            return sqlStr;
        }

        public string CreateSqlStr(string withParmSqlStr, IDataParameter[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}