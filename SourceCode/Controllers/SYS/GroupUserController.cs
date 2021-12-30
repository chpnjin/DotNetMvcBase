using WebBase.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace WebBase.Controllers.SYS
{
    public class GroupUserController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/SYS/GroupUser.cshtml");
        }
    }

    public class ApiGroupUserController : ApiController
    {
        /// <summary>
        /// 取得查詢條件總比數
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject CountA(JObject obj)
        {
            GroupUser sqlCreator = new GroupUser();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.QueryA(obj, true);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        [System.Web.Http.HttpPost]
        public JObject QueryA(JObject obj)
        {
            GroupUser sqlCreator = new GroupUser();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();
            dynamic parm = obj as dynamic;

            var sqlStr = sqlCreator.QueryA(obj, false);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            returnVal["total"] = parm.total;
            returnVal.Add("order", parm.order);
            returnVal.Add("page", parm.page);
            returnVal.Add("sort", parm.sort);
            return returnVal;
        }

        [System.Web.Http.HttpPost]
        public JObject GetOneByGUID(JObject obj)
        {
            GroupUser sqlCreator = new GroupUser();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.GetOne();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        //[System.Web.Http.HttpPost]
        //public JObject Export(JObject obj)
        //{
        //    GroupUser sqlCreator = new GroupUser();
        //    DAO dao = new DAO();
        //    EXCEL excel = new EXCEL();
        //    JObject returnMessage = new JObject();
        //    string controllerName = ControllerContext.RouteData.Values["controller"].ToString().Replace("Api", null);

        //    var sqlStr = sqlCreator.QueryA(obj, false);
        //    var sqlParms = sqlCreator.CreateParameterAry(obj);
        //    dao.AddExecuteItem(sqlStr, sqlParms);

        //    var data = dao.Query().Tables[0];

        //    //檔案建立與命名
        //    string filename = controllerName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        //    string path = Path.Combine(HttpContext.Current.Server.MapPath("/Files/temp/"), filename + ".xlsx");
        //    int count = excel.DataTableToExcel(data, controllerName, false, true, path);

        //    returnMessage.Add("filePath", "/Files/temp/" + filename + ".xlsx");

        //    return JObject.FromObject(returnMessage);
        //}

        public JObject InsertA(JObject obj)
        {
            GroupUser sqlCreator = new GroupUser();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();

            var sqlStr = sqlCreator.InsertA();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        public JObject UpdateA(JObject obj)
        {
            GroupUser sqlCreator = new GroupUser();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();

            var sqlStr = sqlCreator.UpdateA();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        [System.Web.Http.HttpPost]
        public JObject DeleteA(JObject obj)
        {
            GroupUser sqlCreator = new GroupUser();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();

            var sqlStr = sqlCreator.DeleteA();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        /// <summary>
        /// 查詢關聯表資料
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject QueryBind(JObject obj)
        {
            GroupUser sqlCreator = new GroupUser();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.QueryBind();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        /// <summary>
        /// 新增關聯
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject InsertBind(JObject obj)
        {
            GroupUser sqlCreator = new GroupUser();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            JArray ary = JArray.FromObject(obj["BindGuids"]);

            var sqlStr = sqlCreator.InsertBind();

            foreach (var GuidB in ary)
            {
                JObject oneData = new JObject();
                oneData.Add("GROUP_GUID", obj["GuidA"]);
                oneData.Add("USER_GUID", GuidB);
                oneData.Add("INSERT_USER", obj["UPDATE_USER"].ToString());
                var sqlParms = sqlCreator.CreateParameterAry(oneData);
                dao.AddExecuteItem(sqlStr, sqlParms);
            }

            returnMsg.result = dao.Execute();

            return returnMsg;
        }

        [System.Web.Http.HttpPost]
        /// <summary>
        /// 刪除關聯
        /// </summary>
        /// <returns></returns>
        public JObject DeleteBind(JObject obj)
        {
            GroupUser sqlCreator = new GroupUser();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();

            var sqlStr = sqlCreator.DeleteBind();
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            returnMsg.result = dao.Execute();

            return returnMsg;
        }
    }
}