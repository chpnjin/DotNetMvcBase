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
        public ActionResult ToHome(string UserGuid, string SelectedLang)
        {
            Session["LoginGuid"] = Guid.NewGuid().ToString();
            Session["UserGuid"] = UserGuid;
            Session["SelectedLang"] = SelectedLang; //儲存登入時選擇顯示的語系

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
                parmObj.Add("USER_GUID", Session["UserGuid"].ToString());
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
            if (License.Status != LiceneseStatus.Effective)
            {
                switch (License.Status)
                {
                    case LiceneseStatus.None:
                        result["loginFail"] = "None";
                        break;
                    case LiceneseStatus.Invalid:
                        result["loginFail"] = "Invalid";
                        break;
                }

                return result;
            }

            //2.IP無登入紀錄時檢查登入者是否超過人數
            string sqlStr = login.GetSqlStr("GetLoginInfoByIP");
            MySqlParameter[] sqlParms = (MySqlParameter[])login.CreateParameterAry(
                new JObject() {
                    new JProperty("IP", HttpContext.Current.Request.UserHostAddress)
                });

            dao.AddExecuteItem(sqlStr, sqlParms);
            var ipCount = dao.Query().Tables[0].Rows[0][0].ToString();

            var ipExist = int.Parse(ipCount) > 0 ? true : false;

            if (!ipExist)
            {
                int SingInCount = (int)GetCurrentSingInCount(new JObject()).GetValue("SingInCount");

                if (SingInCount + 1 > License.AllowQuantity)
                {
                    result["loginFail"] = "OverLimit";
                    return result;
                }
            }

            //3.帳號密碼驗證
            InputDataProcessor processor = new InputDataProcessor();
            dbpassword = processor.TextEncryption(UserInfo.GetValue("Password").ToString());

            UserInfo["Password"] = dbpassword;

            sqlStr = login.GetSqlStr("LoginAuthenticator");
            sqlParms = (MySqlParameter[])login.CreateParameterAry(UserInfo);

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

        /// <summary>
        /// 重讀已登入使用者資訊
        /// </summary>
        /// <returns></returns>
        public JObject ReloadLoginUserInfo()
        {
            var session = HttpContext.Current.Session;
            JObject result = new JObject();
            Login sqlCreator = new Login();

            //1.抓Session的登入資訊
            string LoginUserGUID = session["UserGuid"].ToString();

            if (!string.IsNullOrEmpty(LoginUserGUID))
            {
                result.Add("userGuid", LoginUserGUID);
                result.Add("userLng", session["SelectedLang"].ToString());

                //2.取得登入User的資訊
                var sqlStr = sqlCreator.GetSqlStr("GetUserInfoByGuid");
                dao.AddExecuteItem(sqlStr, sqlCreator.CreateParameterAry(new JObject {
                    { "GUID", LoginUserGUID }
                }));

                var userInfo = dao.Query().Tables[0];

                if (userInfo.Rows.Count > 0)
                {
                    //3.轉Json後回傳
                    result.Add("userId", userInfo.Rows[0][0].ToString());
                    result.Add("userName", userInfo.Rows[0][1].ToString());
                    result.Add("depart", userInfo.Rows[0][2].ToString());
                    result.Add("title", userInfo.Rows[0][3].ToString());
                }
            }
            else
            {

            }

            return result;
        }
    }
}