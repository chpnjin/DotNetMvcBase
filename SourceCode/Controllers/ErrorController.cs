using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;

namespace WebBase.Controllers
{

    public class ErrorController : Controller
    {
        /// <summary>
        /// 若有登入紀錄直接引導至首頁,否則登入頁
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View("~/Views/ErrorPage.cshtml");
        }
    }
        public class ApiErrorController : ApiController
    {
        /// <summary>
        /// 回傳錯誤頁面
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGetAttribute]
        public HttpResponseMessage Status500()
        {
            string errorPageFilePath = string.Empty;

            //依照web.config的<compilation debug=""> 設定值決定顯示頁面
            if (HttpContext.Current.IsDebuggingEnabled)
            {
                errorPageFilePath = HostingEnvironment.MapPath("/Views/Error_debug.cshtml");
            }
            else
            {
                errorPageFilePath = HostingEnvironment.MapPath("/Views/Error.cshtml");
            }

            var response = new HttpResponseMessage();
            response.Content = new StringContent(File.ReadAllText(errorPageFilePath));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}