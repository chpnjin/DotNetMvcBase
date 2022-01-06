using Newtonsoft.Json.Linq;
using WebBase.Models;
using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Utility;
using MySql.Data.MySqlClient;
using WebBase.Global;

namespace WebBase.Controllers
{
    public class LoginController : Controller
    {
        /// <summary>
        /// 若有登入紀錄直接引導至首頁,否則登入頁
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View("~/Views/Login.cshtml");
        }

        /// <summary>
        /// 儲存登入狀態並導引至首頁
        /// </summary>
        /// <param name="UserGuid"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public ActionResult ToHome(String UserGuid)
        {
            Session["LoginGuid"] = Guid.NewGuid().ToString();
            Session["UserGuid"] = UserGuid;

            //儲存登入紀錄(及時,歷史)
            string clientIP = System.Web.HttpContext.Current.Request.UserHostAddress;
            if (!string.IsNullOrEmpty(clientIP))
            {
                var dao = new MySQL();
                var sqlCreator = new Login();
                var parmObj = new JObject();

                parmObj.Add("GUID", Session["LoginGuid"].ToString());
                parmObj.Add("INSERT_USER", "SYSTEM");
                parmObj.Add("INSERT_TIME", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                parmObj.Add("USER_GUID", UserGuid);
                parmObj.Add("CLIENT_IP", clientIP);
                parmObj.Add("ACTION", "Login");

                var currentLog = sqlCreator.GetSqlStr("SaveLogin");
                var histLog = sqlCreator.GetSqlStr("SaveLoginHist");
                var parmAry = sqlCreator.CreateParameterAry(parmObj);

                dao.AddExecuteItem(currentLog, parmAry);
                dao.AddExecuteItem(histLog, parmAry);

                dao.Execute();
            }

            return Json(new { redirectToUrl = Url.Action("Index", "Home") });
        }
    }

    public class ApiLoginController : ApiController
    {
        DataTableExtensions dt_ext = new DataTableExtensions();
        MySQL dao = new MySQL();

        /// <summary>
        /// 登入驗證
        /// </summary>
        /// <param name="UserInfo">帳號,密碼,語系</param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject LoginAuthenticator(JObject UserInfo)
        {
            Login login = new Login();
            string dbpassword;
            JObject result = new JObject();

            //1.授權驗證
            if (DateTime.Now < License.StartDay)
            {
                result["loginFail"] = "LicenseNotYet";
                return result;
            }
            if (DateTime.Now > License.EndDay)
            {
                result["loginFail"] = "LicenseExpire";
                return result;
            }

            string sqlStr = login.GetSqlStr("GetLoginInfoByIP");
            var sqlParms = login.CreateParameterAry(
                new JObject() {
                    new JProperty("IP", HttpContext.Current.Request.UserHostAddress)
                });

            dao.AddExecuteItem(sqlStr, sqlParms);
            var ipCount = dao.Query().Tables[0].Rows[0][0].ToString();

            var ipExist = int.Parse(ipCount) > 0 ? true : false ;
            
            //IP無登入紀錄時檢查登入者是否超過人數
            if (!ipExist)
            {
                int SingInCount = (int)GetCurrentSingInCount(new JObject()).GetValue("SingInCount");

                if(SingInCount + 1 > License.AllowQuantity)
                {
                    result["loginFail"] = "overLimit";
                    return result;
                }
            }

            //2.帳號密碼驗證
            InputDataProcessor processor = new InputDataProcessor();
            dbpassword = processor.TextEncryption(UserInfo.GetValue("Password").ToString());

            UserInfo["Password"] = dbpassword;

            sqlStr = login.GetSqlStr("LoginAuthenticator");
            sqlParms = login.CreateParameterAry(UserInfo);

            dao.AddExecuteItem(sqlStr, sqlParms);

            result = dt_ext.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return result;
        }

        /// <summary>
        /// 取得目前登入者人數
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JObject GetCurrentSingInCount(JObject obj)
        {
            Login sqlCreator = new Login();
            JObject result = new JObject();

            var sqlStr = sqlCreator.GetSqlStr("GetCurrentSingInIpCount");

            dao.AddExecuteItem(sqlStr, null);

            string SingInCount = dao.Query().Tables[0].Rows[0].ItemArray[0].ToString();

            result.Add("SingInCount", SingInCount);

            return result;
        }
    }
}