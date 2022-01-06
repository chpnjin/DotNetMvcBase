using WebBase.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.SessionState;

namespace WebBase.Controllers
{
    /// <summary>
    /// 全網站通用API
    /// </summary>
    public class GlobalController : ApiController
    {
        [HttpGet]
        public JObject Test()
        {
            JObject obj = new JObject();
            obj.Add("TEST", "Test return value.");

            return obj;
        }

        /// <summary>
        /// 取得下拉選單項目
        /// </summary>
        /// <param name="obj">目標條件</param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetDropDownListItems(JObject obj)
        {
            MySQL dao = new MySQL();
            SYS_PARAM sqlCreator = new SYS_PARAM();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.GetSqlStr("GetDropDownListItems"); //取得SQL執行字串
            var parms = sqlCreator.CreateParameterAry(obj); //建立對應具名參數列表
            dao.AddExecuteItem(sqlStr, parms);

            var result = extensions.ConvertDataTableToKeyValue(dao.Query().Tables[0]);

            return result;
        }

        /// <summary>
        /// 取得全域通用參數
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JObject GetGlobalParam()
        {
            MySQL dao = new MySQL();
            SYS_PARAM sqlCreator = new SYS_PARAM();
            DataTableExtensions extensions = new DataTableExtensions();
            JObject obj = new JObject();
            JObject returnVal = new JObject();

            var sessionSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");

            obj.Add("FUNCTION", "GLOBAL");
            obj.Add("FILTER_KEY", "%");

            var sqlStr = sqlCreator.GetSqlStr("GetSysParamList"); //取得SQL執行字串
            var parms = sqlCreator.CreateParameterAry(obj); //建立對應具名參數列表
            dao.AddExecuteItem(sqlStr, parms);

            
            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            foreach (var item in result.Value<JArray>("rows"))
            {
                returnVal.Add(item.Value<string>("FILTER_KEY"), item.Value<string>("VALUE"));
            }

            //抓Session有效期限設定(單位:秒)
            returnVal.Add("SessionTimeout", sessionSection.Timeout.TotalSeconds.ToString());

            return returnVal;
        }

        /// <summary>
        /// 取得導覽麵包屑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JObject GetBreadcrumb(JObject obj)
        {
            MySQL dao = new MySQL();
            SYS_FUNCTION sqlCreator = new SYS_FUNCTION();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.GetGetBreadcrumb(); //取得SQL執行字串
            var parms = sqlCreator.CreateParameterAry(obj); //建立對應具名參數列表
            dao.AddExecuteItem(sqlStr, parms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        [HttpPost]
        public JObject GetHelpFile(JObject obj)
        {
            MySQL dao = new MySQL();
            SYS_FUNCTION sqlCreator = new SYS_FUNCTION();
            JObject filePath = new JObject();

            var sqlStr = sqlCreator.GetHelpFileNameByFunction();
            var parms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, parms);

            DataSet queryResult = dao.Query();

            if (queryResult.Tables.Count > 0)
            {
                string fileName = queryResult.Tables[0].Rows[0].ItemArray[0].ToString();
                string fileLocation = WebConfigurationManager.AppSettings["HelpFilesLocation"];

                if (fileName == "")
                {
                    filePath.Add("filePath", null);
                }
                else
                {
                    filePath.Add("filePath", fileLocation + fileName);
                }
            }

            return filePath;
        }

        /// <summary>
        /// 取得特定Function的所有設定參數
        /// </summary>
        /// <param name="obj">JSON格式:{FUNCTION : sys_param的FUNCTION_ID欄位值}</param>
        /// <returns></returns>
        [HttpPost]
        public JArray GetParamsByFunction(JObject obj)
        {
            MySQL dao = new MySQL();
            SYS_PARAM sqlCreator = new SYS_PARAM();
            DataTableExtensions extensions = new DataTableExtensions();

            obj.Add("FILTER_KEY", "%");

            var sqlStr = sqlCreator.GetSqlStr("GetSysParamList"); //取得SQL執行字串
            var parms = sqlCreator.CreateParameterAry(obj); //建立對應具名參數列表
            dao.AddExecuteItem(sqlStr, parms);

            var str = JsonConvert.SerializeObject(dao.Query().Tables[0]);
            var result = JArray.Parse(str);

            return result;
        }

        /// <summary>
        /// 取得特定key的config appSettings value()
        /// </summary>
        /// <param name="obj">JSON格式:鍵值為keys的Json Array</param>
        /// <param> example: "keys":["Ford", "BMW", "Fiat"]</param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetAppSettingsValueByKey(JObject obj)
        {
            JArray keyList = (JArray)obj.GetValue("keys");
            JObject result = new JObject();

            //逐一取得key值的Value
            foreach (var key in keyList)
            {
                string settingkey = key.Value<string>();
                try
                {
                    string value = WebConfigurationManager.AppSettings[settingkey];
                    result.Add(settingkey, value);
                }
                catch (Exception ex)
                {
                    result.Add(settingkey, "undefined");
                }
            }

            return result;
        }

        /// <summary>
        /// 取得系統版本號
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JObject GetSystemVersionNum()
        {
            JObject result = new JObject();

            //1.建立dll檔路徑
            string FilePath = HttpRuntime.AppDomainAppPath;
            FilePath += "bin";

            //2.挑選要顯示版本的dll檔案
            string[] FileList = Directory.GetFiles(FilePath, "*.dll");

            var showVerList = FileList.Where(
                x =>
                x.Contains("WebBase.dll") ||
                x.Contains("WebBase_Devel.dll")
                ).ToList();

            //3.將版本資訊存入Json
            foreach (var filePath in showVerList)
            {
                Assembly assembly = Assembly.LoadFrom(filePath);
                result.Add(assembly.ManifestModule.Name, assembly.GetName().Version.ToString());
            }

            return result;
        }
    }
}