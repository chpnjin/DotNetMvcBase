using WebBase.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace WebBase.Controllers.SYS
{
    public class GroupFunctionController : Controller
    {
        // GET: GroupFunction
        public ActionResult Index()
        {
            return View("~/Views/SYS/GroupFunction.cshtml");
        }
    }

    public class ApiGroupFunctionController : ApiController
    {
        [System.Web.Http.HttpPost]
        public JObject QueryA(JObject obj)
        {
            GroupFunction sqlCreator = new GroupFunction();
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

        /// <summary>
        /// 取得查詢條件總比數
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject CountA(JObject obj)
        {
            GroupFunction sqlCreator = new GroupFunction();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.QueryA(obj, true);
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        /// <summary>
        /// 查詢功能表
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public JObject QueryB(JObject obj)
        {
            SYS_FUNCTION sqlCreator = new SYS_FUNCTION();
            DAO dao = new DAO();
            DataTableExtensions extensions = new DataTableExtensions();

            var sqlStr = sqlCreator.GetFunctionList(obj.GetValue("sort").ToString());
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        /// <summary>
        /// 查詢關聯表資料
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject QueryBind(JObject obj)
        {
            GroupFunction sqlCreator = new GroupFunction();
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
            GroupFunction sqlCreator = new GroupFunction();
            DAO dao = new DAO();
            dynamic returnMsg = new JObject();
            JArray ary = JArray.FromObject(obj["BindGuids"]);

            var sqlStr = sqlCreator.InsertBind();

            foreach (var GuidB in ary)
            {
                JObject oneData = new JObject();
                oneData.Add("GROUP_GUID", obj["GuidA"]);
                oneData.Add("FUNCTION_GUID", GuidB);
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
            GroupFunction sqlCreator = new GroupFunction();
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