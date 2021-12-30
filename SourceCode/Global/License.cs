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

        public LicenseManager()
        {
            License.AllowQuantity = -1;
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

            //初始化
            License.StartDay = DateTime.MinValue;
            License.EndDay = DateTime.MinValue;
            License.AllowQuantity = 0;

            GetLicense(new object(), null); //初始化時先抓一次
        }

        /// <summary>
        /// 連線授權管理服務取得授權
        /// </summary>
        /// <returns></returns>
        private void GetLicense(object sender, ElapsedEventArgs e)
        {
            TcpClient client = new TcpClient();

            client.ReceiveTimeout = 5000;

            try
            {
                IAsyncResult MyResult = client.BeginConnect(LicenseSourceHost, port, null, null);
                MyResult.AsyncWaitHandle.WaitOne(3000, true); //等3秒

                if (!MyResult.IsCompleted || client.Connected == false)
                {
                    //連線取得授權失敗(暫時關閉)
                    License.StartDay = DateTime.MinValue;
                    License.EndDay = DateTime.MaxValue;
                    License.AllowQuantity = 65535;
                }
                else if (client.Connected == true)
                {
                    //成功抓取授權資訊
                    var stream = client.GetStream();
                    byte[] msg = Encoding.UTF8.GetBytes("GetLicenese");
                    byte[] receive = new byte[256];

                    stream.Write(msg, 0, msg.Length);

                    System.Threading.Thread.Sleep(3000);

                    int receiveLength = stream.Read(receive, 0, receive.Length);
                    string receiveMsg = Encoding.UTF8.GetString(receive, 0, receiveLength);
                    var licenese = receiveMsg.Split('|');

                    License.StartDay = DateTime.Parse(licenese[0]);
                    License.EndDay = DateTime.Parse(licenese[1]);
                    License.AllowQuantity = int.Parse(licenese[2]);
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                client.Close();
            }
        }

        /// <summary>
        /// 釋放啟用主機名額
        /// </summary>
        public void ReleaseLicense()
        {
            TcpClient client = new TcpClient();

            IAsyncResult MyResult = client.BeginConnect(LicenseSourceHost, port, null, null);
            MyResult.AsyncWaitHandle.WaitOne(3000, true); //等3秒

            if (!MyResult.IsCompleted || client.Connected == false)
            {
                //作如果沒連上線的事
            }
            else if (client.Connected == true)
            {
                //作連上線的事
                var stream = client.GetStream();
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