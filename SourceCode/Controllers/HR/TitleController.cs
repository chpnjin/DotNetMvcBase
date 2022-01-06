using Newtonsoft.Json.Linq;
using System.Web.Http;
using System.Web.Mvc;
using Utility;
using System.Data;
using System;
using System.IO;
using System.Web;
using System.Threading;
using WebBase.Models;
using Newtonsoft.Json;

namespace WebBase.Controllers
{
    public class TitleController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/HR/Title.cshtml");
        }
    }

    public class ApiTitleController : ApiController
    {
        [System.Web.Http.HttpPost]
        /// <summary>
        /// 查詢使用者
        /// </summary>
        /// <param name="conditions">查詢條件</param>
        /// <returns></returns>
        public JObject Query(JObject obj)
        {
            Title sqlCreator = new Title();
            MySQL dao = new MySQL();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;

            var sqlStr = sqlCreator.Search(obj, false);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }

        /// <summary>
        /// 從GUID抓取使用者
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetOneByGUID(JObject obj)
        {
            Title sqlCreator = new Title();
            MySQL dao = new MySQL();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.GetOneByGUID();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        /// <summary>
        /// 取得查詢條件總比數
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject Count(JObject obj)
        {
            Title sqlCreator = new Title();
            MySQL dao = new MySQL();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.Search(obj, true);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);
            var count = result["rows"][0].Value<string>("Count");
            var returnVal = new JObject();
            returnVal.Add("count", count);

            return returnVal;
        }

        /// <summary>
        /// 將搜尋結果匯出成 Excel 儲存至 server中 /Files/temp/ 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns> 
        [System.Web.Http.HttpPost]
        public JObject Export(JObject obj)
        {
            Title sqlCreator = new Title();
            MySQL dao = new MySQL();
            EXCEL excel = new EXCEL();
            JObject returnMessage = new JObject();
            string controllerName = ControllerContext.RouteData.Values["controller"].ToString().Replace("Api", null);

            var sqlStr = sqlCreator.Search(obj, false);
            var sqlParms = sqlCreator.CreateParameterAry(obj);
            dao.AddExecuteItem(sqlStr, sqlParms);

            var data = dao.Query().Tables[0];

            //檔案建立與命名
            string filename = controllerName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string path = Path.Combine(HttpContext.Current.Server.MapPath("/Files/temp/"), filename + ".xlsx");
            int count = excel.DataTableToExcel(data, controllerName, false, true, path);

            returnMessage.Add("filePath", "/Files/temp/" + filename + ".xlsx");

            return JObject.FromObject(returnMessage);
        }

        /// <summary>
        /// 新增使用者
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject Insert(JObject obj)
        {
            Title sqlCreator = new Title();
            MySQL dao = new MySQL();
            dynamic returnMsg = new JObject();

            var sqlStr = sqlCreator.Insert();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        /// <summary>
        /// 更新使用者
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject Update(JObject obj)
        {
            Title sqlCreator = new Title();
            MySQL dao = new MySQL();
            dynamic returnMsg = new JObject();

            var sqlStr = sqlCreator.Update();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        [System.Web.Http.HttpPost]
        /// <summary>
        /// 刪除使用者
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject Delete(JObject obj)
        {
            Title sqlCreator = new Title();
            MySQL dao = new MySQL();
            dynamic returnMsg = new JObject();

            var sqlStr = sqlCreator.Delete();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        #region 下拉選單

        #endregion
    }
}