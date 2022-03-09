using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using WebBase.Controllers;
using WebBase.Global;
using Newtonsoft.Json.Linq;
using System.Web.SessionState;

namespace WebBase
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            GlobalFilters.Filters.Add(new WebActionFilters());

            //引用log4net的config
            string log4netPath = Server.MapPath("~/log4net.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(log4netPath));

            LicenseManager manager = new LicenseManager();
            manager.Startup();
        }

        /// <summary>
        /// 啟用 Web API 可讀取Session
        /// </summary>
        protected void Application_PostAuthorizeRequest()
        {
            if (HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/api"))
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }

        /// <summary>
        /// Session到期時自動記錄為登出
        /// </summary>
        protected void Session_End()
        {
            var loginGuid = Session["LoginGuid"];
            var userGuid = Session["UserGuid"];

            if (loginGuid != null)
            {
                HomeController controller = new HomeController();
                controller.Logout(loginGuid.ToString(), userGuid.ToString());
            }
        }

        /// <summary>
        /// 應用程式關閉時執行的程式碼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_End(object sender, EventArgs e)
        {
            LicenseManager manager = new LicenseManager();
            manager.Release();
        }
    }
}
