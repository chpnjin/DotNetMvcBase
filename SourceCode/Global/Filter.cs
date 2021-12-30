using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebBase.Global
{
    /// <summary>
    /// WebAPI請求過濾
    /// </summary>
    /// <param name="actionContext"></param>
    public class AuthorizationFilters : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {

        }
    }

    public class ReturnFilter : System.Web.Http.Filters.ActionFilterAttribute {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            string action = actionContext.Request.RequestUri.LocalPath;

            //排除驗證清單
            string[] nonVerify =
            {
                "/api/ApiLogin/LoginAuthenticator", //登入驗證
                "/api/ApiLogin/GetCurrentSingInCount", //取得目前登入人數
                "/api/Error/Status500", // Server 端 Error
                "/api/Error/GetErrorMsg" // 取得錯誤訊息
            };

            if (!nonVerify.Contains(action))
            {
                var licenseStatus = HttpContext.Current.Application.Get("LicenseStatus");

                if (licenseStatus == null)
                {
                    LicenseManager license = new LicenseManager();
                    license.Startup();
                }

                //等待抓到網站授權狀態再往下執行
                while (licenseStatus == null)
                {
                    licenseStatus = HttpContext.Current.Application.Get("LicenseStatus");
                }

                if (licenseStatus.ToString() != "true")
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                else
                {

                }
            }
        }


    }

    /// <summary>
    /// Action請求過濾
    /// </summary>
    public class WebActionFilters : System.Web.Mvc.ActionFilterAttribute
    {
        //1
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string action = filterContext.HttpContext.Request.Path;

            //排除驗證:登入與登出動作,錯誤頁面
            string[] nonVerify =
            {
                "/Login",
                "/Login/ToHome",
                "/Home/Logout",
                "/Error"
            };

            if (!nonVerify.Contains(action))
            {
                var licenseStatus = HttpContext.Current.Application.Get("LicenseStatus");

                if (licenseStatus == null)
                {
                    LicenseManager license = new LicenseManager();
                    license.Startup();
                }

                //無登入紀錄直接重新導向登入頁
                if (filterContext.HttpContext.Session["LoginGuid"] == null)
                {
                    filterContext.Result = new RedirectResult("/Login");
                    return;
                }
            }
        }

        //2
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }

        //3
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {

        }

        //4
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {

        }
    }
}