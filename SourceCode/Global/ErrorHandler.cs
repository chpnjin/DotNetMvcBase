using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace WebBase.Global
{
    /// <summary>
    /// 實作自訂回傳錯誤訊息介面
    /// </summary>
    class ErrorMessage : IHttpActionResult
    {
        /// <summary>
        /// HTTP需求
        /// </summary>
        public HttpRequestMessage Request { get; set; }
        /// <summary>
        /// 要回傳給Client端的訊息
        /// </summary>
        public string ReturnMessage { get; set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError); //統一指定HTTP狀態碼500
            response.Content = new StringContent(ReturnMessage);

            return Task.FromResult(response);
        }
    }

    /// <summary>
    /// 回傳給Client端的錯誤訊息處理
    /// </summary>
    public class GlobalExceptionHandler : ExceptionHandler
    {
        //宣告log4net物件
        ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Handle(ExceptionHandlerContext context)
        {
            //指定回傳給Client端錯誤訊息
            var msg = new ErrorMessage();
            msg.Request = context.Request;
            msg.ReturnMessage = context.Exception.Message + "\n" + context.Exception.StackTrace;

            // 寫入Log File
            // 1.錯誤發生controller API URL
            // 2.錯誤訊息內容
            // 3.需求來源IP位置
            string url = context.Request.RequestUri.AbsolutePath;
            string ip = HttpContext.Current.Request.UserHostAddress;

            ThreadContext.Properties["addr"] = ip;
            logger.Error("API URL:" + url + " / 錯誤內容:" + msg.ReturnMessage);

            context.Result = msg;

        }
    }
}