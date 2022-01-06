using WebBase.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;

namespace WebBase.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Index.cshtml");
        }

        /// <summary>
        /// 登出動作
        /// </summary>
        /// <param name="LoginGuid">登入紀錄key</param>
        /// <param name="UserGuid">登入帳號key</param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public ActionResult Logout(string LoginGuid = null, string UserGuid = null)
        {
            var dao = new MySQL();
            var sqlCreator = new Login();
            var parmObj = new JObject();
            string clientIP = string.Empty;

            string getLoginInfo = sqlCreator.GetSqlStr("GetOneLoginInfoByGuid");
            string removeLogin = sqlCreator.GetSqlStr("ClearLoginInfo");
            string saveLogout = sqlCreator.GetSqlStr("SaveLogoutInfo");

            if (LoginGuid is null)
            {
                LoginGuid = Session["LoginGuid"].ToString();
            }
            if (UserGuid is null)
            {
                UserGuid = Session["UserGuid"].ToString();
            }

            //1.取得登入時的IP位置
            parmObj.Add("GUID", LoginGuid);
            dao.AddExecuteItem(getLoginInfo, sqlCreator.CreateParameterAry(parmObj));
            clientIP = dao.Query().Tables[0].Rows[0]["CLIENT_IP"].ToString();

            parmObj.RemoveAll();

            //2.刪除登入狀態
            parmObj.Add("LoginGuid", LoginGuid);
            dao.AddExecuteItem(removeLogin, sqlCreator.CreateParameterAry(parmObj));

            parmObj.RemoveAll();

            //3.紀錄登出動作
            parmObj.Add("INSERT_USER", "SYSTEM");
            parmObj.Add("INSERT_TIME", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            parmObj.Add("USER_GUID", UserGuid);
            parmObj.Add("CLIENT_IP", clientIP);
            parmObj.Add("ACTION", "Logout");
            dao.AddExecuteItem(saveLogout, sqlCreator.CreateParameterAry(parmObj));

            dao.Execute();

            if(Session is not null)
            {
                Session.Clear();
            }

            return Json(new { redirectToUrl = Url.Action("Index", "Login") });
        }

        public ActionResult Heartbeat()
        {
            //讀取Session，避免逾時
            string chk = Session["LoginGuid"] as string;
            string loginGuid = Session["UserGuid"] as string;

            if (string.IsNullOrEmpty(chk))
            {
                return Content("Session lost");
            }
            return Content("OK");
        }
    }

    /// <summary>
    /// 依照使用權限取得左側導覽列項目
    /// </summary>
    public class LayoutController : ApiController
    {
        [HttpPost]
        public JArray GetSideBarItems(JObject obj)
        {
            GroupUser groupUser = new GroupUser();
            SYS_FUNCTION function = new SYS_FUNCTION();
            DataTableExtensions extensions = new DataTableExtensions();
            MySQL dao = new MySQL();

            //1.使用者找所屬群組
            var userGuid = groupUser.CreateParameterAry(obj);
            var getGroupsSql = groupUser.GetGroupsByUser();
            dao.AddExecuteItem(getGroupsSql, userGuid);

            var groups = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            //2.所屬群組權限找可用Function
            var allGroupOfAuthority = function.CreateGroupsParameter(groups.GetValue("rows"));
            var getFunctions = function.GetFunctionByUserGroup(allGroupOfAuthority);
            dao.AddExecuteItem(getFunctions, allGroupOfAuthority);

            var returnVal = function.ConvertNavTree(dao.Query().Tables[0]);

            return returnVal;
        }

        /// <summary>
        /// 取得前台按鈕下拉選單項目
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JObject GetFrontOfficeDropDownItems(JObject obj)
        {
            GroupUser groupUser = new GroupUser();
            GroupNavigation groupNavigation = new GroupNavigation();
            DataTableExtensions extensions = new DataTableExtensions();
            MySQL dao = new MySQL();

            //1.使用者找所屬群組
            var userGuid = groupUser.CreateParameterAry(obj);
            var getGroupsSql = groupUser.GetGroupsByUser();
            dao.AddExecuteItem(getGroupsSql, userGuid);

            var groups = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            //2.所屬群組權限找可用下拉選單項目
            var allGroupOfAuthority = groupNavigation.CreateGroupsParameter(groups.GetValue("rows"));
            var getItems = groupNavigation.GetItemsByUserGroup(allGroupOfAuthority);
            dao.AddExecuteItem(getItems, allGroupOfAuthority);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }
    }
}
