using WebBase.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using Utility;

namespace WebBase.Controllers
{
    public class LogController : ApiController
    {
        /// <summary>
        /// 儲存事件
        /// </summary>
        /// <param name="obj"></param>
        [HttpPost]
        public JObject SaveEventLog(JObject obj)
        {
            LOG sqlCreator = new LOG();
            DAO dao = new DAO();
            JObject returnVal = new JObject();

            var getGroupIdStr = sqlCreator.GetSqlStr("GetTopOneUserGroup");
            var sqlStr = sqlCreator.GetSqlStr("InsertToActionLog"); //產SQL String
            var parm = sqlCreator.CreateParameterAry(obj); //產對應具名參數表

            //1.取得對應UserGroup後,加至Insert指令具名參數
            dao.AddExecuteItem(getGroupIdStr, parm); //加入執行排程清單
            var group = dao.Query().Tables[0];
            string groupId = group.Rows.Count > 0 ? group.Rows[0].ItemArray[0].ToString() : "";

            obj.Add("GROUP_ID", groupId);
            parm = sqlCreator.CreateParameterAry(obj);

            //2.存至ACTION LOG表
            dao.AddExecuteItem(sqlStr, parm); //加入執行排程清單
            var result = dao.Execute(); //開始執行
            returnVal.Add("result", result);

            return returnVal;
        }

        /// <summary>
        /// 根據呼叫的Url取得對應執行的SQL字串(所用參數--function:功能,action:動作)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JObject GetSqlStringByAction(JObject obj)
        {
            ISqlCreator sqlStrCreater; //建立SQL字串介面
            DAO dao = new DAO();　//DB存取介面
            InputDataProcessor processor = new InputDataProcessor();
            JObject conditions = (JObject)obj.GetValue("params");
            JObject returnVal = new JObject();
            string function = obj.GetValue("function").ToString();
            string action = obj.GetValue("action").ToString();
            string sqlStr = string.Empty;

            //功能頁面
            switch (function)
            {
                case "User":
                    sqlStrCreater = new User();
                    break;
                default:
                    sqlStrCreater = new User();
                    break;
            }

            sqlStr = sqlStrCreater.GetSqlStr(action, conditions);

            sqlStr = dao.CreateSqlStr(sqlStr, sqlStrCreater.CreateParameterAry(conditions));
            //SQL加密
            sqlStr = processor.Base64Encrypt(sqlStr, Encoding.UTF8);

            returnVal.Add("sqlStr", sqlStr);

            return returnVal;
        }

        /// <summary>
        /// 儲存SQL執行效能
        /// </summary>
        /// <param name="obj"></param>
        [HttpPost]
        public void SaveSqlPerformanceLog(JObject obj)
        {
            LOG sqlCreator = new LOG();
            DAO dao = new DAO();
            InputDataProcessor processor = new InputDataProcessor();

            //SQL解密
            string requestSql = processor.Base64Decrypt(obj.GetValue("SQL").ToString(), Encoding.UTF8);
            obj["SQL"] = requestSql;

            var getGroupIdStr = sqlCreator.GetSqlStr("GetTopOneUserGroup");
            var sqlStr = sqlCreator.GetSqlStr("InsertToPerformanceLog");
            var parm = sqlCreator.CreateParameterAry(obj); //產對應具名參數表

            //1.取得對應UserGroup後,加至Insert指令具名參數
            dao.AddExecuteItem(getGroupIdStr, parm); //加入執行排程清單
            var group = dao.Query().Tables[0];
            string groupId = group.Rows.Count > 0 ? group.Rows[0].ItemArray[0].ToString() : "";

            obj.Add("GROUP_ID", groupId);
            parm = sqlCreator.CreateParameterAry(obj);

            //2.存至LOG表
            dao.AddExecuteItem(sqlStr, parm); //加入執行排程清單
            dao.Execute(); //開始執行
        }

        /// <summary>
        /// 取得今日特定User操作
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetTodayActionLogByUser(JObject obj)
        {
            LOG sqlCreator = new LOG();
            DataTableExtensions extensions = new DataTableExtensions();
            DAO dao = new DAO();

            var sqlStr = sqlCreator.GetSqlStr("GetTodayActionLogByUser");
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);

            var returnVal = extensions.ConvertDataTableToJObject(dao.Query().Tables[0]);

            return returnVal;
        }

        /// <summary>
        /// 新增至通知清單
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject InsertNotice(JObject obj)
        {
            LOG sqlCreator = new LOG();
            DAO dao = new DAO();
            JObject returnVal = new JObject();

            var noticeType = obj.GetValue("type").ToString();
            var title = obj.GetValue("title").ToString();
            var msg = obj.GetValue("msg");

            var sqlStr = sqlCreator.GetSqlStr("AddNotice");
            JObject one = new JObject();

            one.Add("TYPE", noticeType.ToUpper());
            one.Add("TITLE", title.ToLower());
            one.Add("CONTENT", msg.ToString());

            var sqlParms = sqlCreator.CreateParameterAry(one);
            dao.AddExecuteItem(sqlStr, sqlParms);

            var result = dao.Execute(); //開始執行
            returnVal.Add("result", result);

            return returnVal;
        }

        /// <summary>
        /// 讀取登入者未讀取與已讀取通知
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public JArray GetNoticeByUser(JObject obj)
        {
            LOG sqlCreator = new LOG();
            DAO dao = new DAO();

            var sqlStr = sqlCreator.GetSqlStr("QueryNoticeByUser");
            var sqlParms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, sqlParms);
            string tmp = JsonConvert.SerializeObject(dao.Query().Tables[0]);
            JArray ary = JArray.Parse(tmp);

            return ary;
        }

        /// <summary>
        /// 新增特定使用者已讀取通知紀錄
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JObject UpdateReadStatus(JObject obj)
        {
            LOG sqlCreator = new LOG();
            DAO dao = new DAO();
            JObject returnVal = new JObject();

            var sqlStr = sqlCreator.GetSqlStr("AddReadNoticeLog");
            var parms = sqlCreator.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, parms);

            var result = dao.Execute(); //開始執行
            returnVal.Add("result", result);

            return returnVal;
        }

        /// <summary>
        /// 新增API LOG
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject AddApiLog(JObject obj)
        {
            LOG log = new LOG();
            DAO dao = new DAO();
            JObject returnVal = new JObject();

            var sqlStr = log.GetSqlStr("InsertToApiLog");
            var parms = log.CreateParameterAry(obj);

            dao.AddExecuteItem(sqlStr, parms);

            var result = dao.Execute(); //開始執行
            returnVal.Add("result", result);

            return returnVal;
        }
    }
}