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
    public class SYS_FUNCTION : ISqlCreator
    {
        string TableName
        {
            get
            {
                return "SYS_FUNCTION";
            }
        }

        public IDataParameter[] CreateParameterAry(JObject input)
        {if (input is null)
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
                    case "GUID":
                        parm.DbType = DbType.Guid;
                        break;
                    case "GROUP_ID":
                        parm.DbType = DbType.String;
                        break;
                    case "LIST_ITEM_ID":
                        parm.DbType = DbType.String;
                        break;
                    case "FUNCTION_ID":
                        parm.DbType = DbType.String;
                        break;
                    default:
                        parm.Value = value + "%";
                        parm.DbType = DbType.String;
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
                parm.DbType = DbType.String;

                parmList.Add(parm);
            }

            return parmList.ToArray();
        }

        public string GetSqlStr(string actionName, [Optional] JObject parm)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 取得所有功能清單
        /// </summary>
        /// <returns></returns>
        public string GetFunctionList(string order)
        {
            string sqlStr = string.Format("SELECT DISTINCT GUID, FUNCTION_ID,DOC_KEY FROM {0} WHERE ENABLE = 1 ", TableName);
            sqlStr += "AND FUNCTION_ID != '#' AND FUNCTION_ID LIKE CONCAT(@FUNCTION_ID,'%') ";
            sqlStr += string.Format("ORDER BY {0} ", order);

            return sqlStr;
        }

        /// <summary>
        /// 依權限取得對應可用的功能項目
        /// </summary>
        /// <returns></returns>
        public string GetFunctionByUserGroup(MySqlParameter[] groups)
        {
            string sqlStr = $"SELECT F.GUID,F.FUNCTION_ID,F.LEVEL,F.`INDEX`,F.PARENT_GUID,F.DOC_KEY,F.ICON_KEY FROM {TableName} F ";
            sqlStr += "WHERE ENABLE = 1 AND FUNCTION_ID = '#' ";
            sqlStr += "UNION ALL ";
            sqlStr += $"SELECT F.GUID,F.FUNCTION_ID,F.LEVEL,F.`INDEX`,F.PARENT_GUID,F.DOC_KEY,F.ICON_KEY FROM {TableName} F ";
            sqlStr += "INNER JOIN SYS_GROUP_FUNCTION GF ON GF.FUNCTION_GUID = F.GUID ";
            sqlStr += "INNER JOIN SYS_GROUP G ON GF.GROUP_GUID = G.GUID ";
            sqlStr += "WHERE GF.ENABLE = 1 AND G.ID IN (";

            foreach (var item in groups)
            {
                sqlStr += item.ParameterName;
                
                if(item != groups.Last())
                {
                    sqlStr += ",";
                }
            }

            sqlStr += ");";

            return sqlStr;
        }

        /// <summary>
        /// 取得麵包屑項目
        /// </summary>
        /// <returns></returns>
        public string GetGetBreadcrumb()
        {
            string sqlStr = "SELECT L1.DOC_KEY AS LEVEL1,L2.DOC_KEY AS LEVEL2,L3.DOC_KEY AS LEVEL3 ";
            sqlStr += $"FROM {TableName} L3 ";
            sqlStr += $"LEFT JOIN {TableName} L2 ON L2.GUID = L3.PARENT_GUID ";
            sqlStr += $"LEFT JOIN {TableName} L1 ON L1.GUID = L2.PARENT_GUID ";
            sqlStr += "WHERE L3.ENABLE = 1 AND L3.FUNCTION_ID = @FUNCTION_ID;";

            return sqlStr;
        }

        /// <summary>
        /// 將撈出來的導覽列設定組成用於生成導覽列的前端JSON
        /// </summary>
        /// <param name="table">原始資料DataTable</param>
        /// <returns></returns>
        public JArray ConvertNavTree(DataTable origin)
        {
            List<NavItem> rootList = new List<NavItem>();
            List<NavItem> subList = new List<NavItem>();

            //功能
            var funcItems = (from x in origin.AsEnumerable()
                             where x.Field<string>("FUNCTION_ID") != "#"
                             orderby x.Field<float>("INDEX")
                             select x);

            //導覽項目
            var navItems = from x in origin.AsEnumerable()
                           where x.Field<string>("FUNCTION_ID") == "#"
                           orderby x.Field<float>("INDEX")
                           select x;

            //功能頁面分組
            foreach (var func in funcItems)
            {
                //1.找出屬於該功能的上層項目
                var upper = (from x in navItems
                             where x.Field<string>("GUID") == func["PARENT_GUID"].ToString()
                             select x).First();

                if ((float)upper["LEVEL"] == 1)
                {
                    //找根目錄
                    var targetItem = from x in rootList
                                     where x.Id == upper["GUID"].ToString()
                                     select x;

                    if (targetItem.Count() > 0) //直接新增
                    {
                        targetItem.First().Sub.Add(new NavItem
                        {
                            Id = func["GUID"].ToString(),
                            Function = func["FUNCTION_ID"].ToString(),
                            DocKey = func["DOC_KEY"].ToString(),
                            Parent = func["PARENT_GUID"].ToString(),
                            Index = (float)func["INDEX"],
                        });
                    }
                    else //將父階層在根目錄新增後,將此功能頁面加至此項
                    {
                        var rootItem = new NavItem
                        {
                            Id = upper["GUID"].ToString(),
                            Parent = string.Empty,
                            Function = upper["FUNCTION_ID"].ToString(),
                            DocKey = upper["DOC_KEY"].ToString(),
                            IconKey = upper["ICON_KEY"].ToString(), //父階層包含圖示
                            Sub = new List<NavItem>(),
                            Index = (float)upper["INDEX"],
                        };

                        rootItem.Sub.Add(new NavItem
                        {
                            Id = func["GUID"].ToString(),
                            Parent = func["PARENT_GUID"].ToString(),
                            Function = func["FUNCTION_ID"].ToString(),
                            DocKey = func["DOC_KEY"].ToString(),
                            Index = (float)func["INDEX"],
                        });

                        rootList.Add(rootItem);
                    }

                }

                if ((float)upper["LEVEL"] == 2)
                {
                    //找子目錄清單
                    var targetItem = from x in subList
                                     where x.Id == upper["GUID"].ToString()
                                     select x;

                    if (targetItem.Count() > 0) //直接新增
                    {
                        targetItem.First().Sub.Add(new NavItem
                        {
                            Id = func["GUID"].ToString(),
                            Function = func["FUNCTION_ID"].ToString(),
                            DocKey = func["DOC_KEY"].ToString(),
                            Parent = func["PARENT_GUID"].ToString(),
                            Index = (float)func["INDEX"]
                        });
                    }
                    else //子項目清單新增一項後,把功能加入子項目
                    {
                        var subItem = new NavItem
                        {
                            Id = upper["GUID"].ToString(),
                            Parent = upper["PARENT_GUID"].ToString(),
                            Function = upper["FUNCTION_ID"].ToString(),
                            DocKey = upper["DOC_KEY"].ToString(),
                            Index = (float)upper["INDEX"],
                            Sub = new List<NavItem>()
                        };

                        subItem.Sub.Add(new NavItem
                        {
                            Id = func["GUID"].ToString(),
                            Parent = func["PARENT_GUID"].ToString(),
                            Function = func["FUNCTION_ID"].ToString(),
                            Index = (float)func["INDEX"],
                            DocKey = func["DOC_KEY"].ToString()
                        });

                        subList.Add(subItem);
                    }
                }
            }

            //子導覽項目新增至根目錄
            var orderedSub = subList.OrderBy(x => x.Index);
            foreach (var sub in orderedSub)
            {
                //1.找出子導覽項所屬的根導覽項目
                var root = (from x in navItems
                            where x.Field<string>("GUID") == sub.Parent
                            select x).First();

                //2.檢查此根項目是否已新增
                var rootItem = from x in rootList
                               where x.Id == root["GUID"].ToString()
                               select x;

                if (rootItem.Count() > 0)
                {
                    //追加
                    rootItem.First().Sub.Add(sub);
                }
                else
                {
                    //新增
                    var newRootItem = new NavItem
                    {
                        Id = root["GUID"].ToString(),
                        Index = (float)root["INDEX"],
                        Parent = string.Empty,
                        Function = root["FUNCTION_ID"].ToString(),
                        DocKey = root["DOC_KEY"].ToString(),
                        IconKey = root["ICON_KEY"].ToString(), //父階層包含圖示
                        Sub = new List<NavItem>()
                    };

                    newRootItem.Sub.Add(sub);
                    rootList.Add(newRootItem);
                }
            }

            var returnVal = rootList.OrderBy(x => x.Index);

            return JArray.FromObject(returnVal);
        }

        /// <summary>
        /// 取得特定功能的使用說明檔檔名
        /// </summary>
        /// <returns></returns>
        public string GetHelpFileNameByFunction()
        {
            string sqlStr = $"SELECT HELP_FILE_NAME FROM {TableName} ";
            sqlStr += "WHERE ENABLE = 1 AND FUNCTION_ID = @FUNCTION_ID;";

            return sqlStr;
        }
    }

    class NavItem
    {
        public string Id { get; set; }
        public float Index { get; set; }
        public string Parent { get; set; }
        public string DocKey { get; set; }
        public string IconKey { get; set; }
        public string Function { get; set; }
        public List<NavItem> Sub { get; set; }
    }
}