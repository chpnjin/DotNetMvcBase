using Newtonsoft.Json.Linq;
using System;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Web;
using System.Web.Configuration;

namespace WebBase.Global
{
    /// <summary>
    /// 授權狀態定義
    /// </summary>
    public enum LiceneseStatus
    {
        /// <summary>
        /// 未取得
        /// </summary>
        None = 0,
        /// <summary>
        /// 有效
        /// </summary>
        Effective = 1,
        /// <summary>
        /// 無效
        /// </summary>
        Invalid = 2,
    }

    /// <summary>
    /// 授權內容
    /// </summary>
    public static class License
    {
        /// <summary>
        /// 起始時間
        /// </summary>
        public static DateTime StartDay { get; set; }
        /// <summary>
        /// 到期時間
        /// </summary>
        public static DateTime EndDay { get; set; }
        /// <summary>
        /// 可同時連線IP數量
        /// </summary>
        public static int AllowQuantity { get; set; }
        /// <summary>
        /// 授權狀態
        /// </summary>
        public static LiceneseStatus Status { get; set; }
    }

    /// <summary>
    /// 授權管理工具
    /// </summary>
    public class LicenseManager
    {
        int port = 51500; //TCP通用埠

        /// <summary>
        /// 授權來源主機
        /// </summary>
        string LicenseSourceHost { get; set; }

        /// <summary>
        /// 建構式
        /// </summary>
        public LicenseManager()
        {
            License.StartDay = DateTime.MinValue;
            License.EndDay = DateTime.MinValue;
            License.AllowQuantity = 0;
            License.Status = LiceneseStatus.None;
            LicenseSourceHost = WebConfigurationManager.AppSettings["LicenseSourceHostIP"];
        }

        /// <summary>
        /// 啟動授權取得
        /// </summary>
        public void Startup()
        {
            Timer liceneseChecker = new Timer();
            liceneseChecker.Elapsed += new ElapsedEventHandler(GetLicense);
            liceneseChecker.Interval = 1000 * 86400; //每24小時檢查一次
            liceneseChecker.Enabled = true;

            GetLicense(new object(), null); //初始化時先抓一次
        }

        /// <summary>
        /// 連線授權管理服務取得授權
        /// </summary>
        /// <returns></returns>
        async void GetLicense(object sender, ElapsedEventArgs e)
        {
            //測試連線3次
            for (int i = 0; i < 3; i++)
            {
                var client = new Socket(SocketType.Stream, ProtocolType.Tcp);
                client.ReceiveTimeout = 10000;
                IAsyncResult tryConnectResult = client.BeginConnect(LicenseSourceHost, port, null, null);
                tryConnectResult.AsyncWaitHandle.WaitOne(3000, true); //等3秒

                if (!tryConnectResult.IsCompleted || client.Connected == false)
                {
                    //作如果沒連上線的事
                }
                else if (client.Connected == true)
                {
                    //作連上線的事
                    var stream = new NetworkStream(client);
                    byte[] msg = Encoding.UTF8.GetBytes("GetLicense");
                    byte[] receive = new byte[256];

                    //傳送授權請求並等待接收
                    stream.Write(msg, 0, msg.Length);
                    var receiveLength = await stream.ReadAsync(receive, 0, receive.Length);

                    string receiveMsg = Encoding.UTF8.GetString(receive, 0, receiveLength);
                    var licenese = receiveMsg.Split('|');

                    //驗證接收資訊是否正確,若錯誤將再次請求
                    if (licenese.Length != 4)
                    {
                        client.Close();
                        continue;
                    }

                    bool IsCorrectHardware = bool.Parse(licenese[0].ToString());
                    License.StartDay = DateTime.Parse(licenese[1]);
                    License.EndDay = DateTime.Parse(licenese[2]);
                    License.AllowQuantity = int.Parse(licenese[3]);

                    //授權安裝硬體資訊不匹配
                    if (!IsCorrectHardware)
                    {
                        License.Status = LiceneseStatus.Invalid;
                        break;
                    }

                    //不在授權有效期限
                    if (DateTime.Today >= License.StartDay && DateTime.Today <= License.EndDay)
                    {
                        License.Status = LiceneseStatus.Effective;
                    }
                    else
                    {
                        License.Status = LiceneseStatus.Invalid;
                    }

                    break;
                }
                //連線失敗時
                License.StartDay = DateTime.MinValue;
                License.EndDay = DateTime.MinValue;
                License.AllowQuantity = 0;

                client.Close();
            }
        }

        /// <summary>
        /// 釋放啟用主機名額
        /// </summary>
        public void Release()
        {
            var client = new Socket(SocketType.Stream, ProtocolType.Tcp);

            IAsyncResult MyResult = client.BeginConnect(LicenseSourceHost, port, null, null);
            MyResult.AsyncWaitHandle.WaitOne(3000, true); //等3秒

            if (!MyResult.IsCompleted || client.Connected == false)
            {
                //作如果沒連上線的事
            }
            else if (client.Connected == true)
            {
                //作連上線的事
                var stream = new NetworkStream(client);
                byte[] msg = Encoding.UTF8.GetBytes("Release");
                byte[] receive = new byte[256];

                stream.Write(msg, 0, msg.Length);

                System.Threading.Thread.Sleep(3000);

                int receiveLength = stream.Read(receive, 0, receive.Length);
                string responseData = Encoding.UTF8.GetString(receive, 0, receiveLength);
            }

            client.Close();
        }
    }
}