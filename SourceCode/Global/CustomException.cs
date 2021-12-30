using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBase.Global
{
    /// <summary>
    /// 自訂錯誤類別物件
    /// </summary>
    public class CustomException : Exception
    {
        Exception _ex { get; set; }
        public override string Message { get
            {
                return _ex.Message;
            } 
        }
        public override string StackTrace
        {
            get
            {
                return _ex.StackTrace;
            }
        }

        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="ex">原始錯誤物件</param>
        public CustomException(Exception ex)
        {
            _ex = ex;
        }
    }
}