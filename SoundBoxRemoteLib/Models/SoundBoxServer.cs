using Acr.DeviceInfo;
using Newtonsoft.Json;
using SoundBoxRemoteLib.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SoundBoxRemoteLib.Models
{
    public class SoundBoxServer
    {
        private const int MIN_SUPPORTED_VERSION = 6;
        private const string URL_SYSTEM_INFO = @"http://{0}:8095/api/v6/system";
        private const string URL_SYSTEM_API = @"http://{0}:8095/api/";
        private const string URL_API_CODE = @"?ApiCode=";

        public string IPAddress { get; private set; }
        public string MachineName { get; set; }
        public string AccountName { get; set; }
        public string SoundBoxVersion { get; set; }
        public APIVersion ApiVersion { get; set; }
        public string CongregationName { get; set; }
        public string SessionId { get; set; }
        public bool TimerApiEnabled { get; set; }
        public bool SongsApiEnabled { get; set; }
        public bool MediaApiEnabled { get; set; }
        public bool ApiCodeRequired { get; set; }
        public string APICode { get; set; }

        #region Global Instantiator

        public static List<SoundBoxServer> FindAllServers()
        {
            var servers = new List<SoundBoxServer>();
            CheckConnection();
            var ip = GetLocalIPAddress();
            string[] classes = ip.Split('.');
            var subnet = classes[0] + "." + classes[1] + "." + classes[2] + ".";

            for (int i = 0; i < 255; i++)
            {
                var checkIp = subnet + i.ToString();
                if (CheckForServer(checkIp))
                    servers.Add(SoundBoxServer.LoadFromIP(checkIp));
            }

            return servers;
        }

        public static SoundBoxServer LoadFromIP(string ipAddress)
        {
            string url = string.Format(URL_SYSTEM_INFO, ipAddress);
            var client = new HttpClient();
            SoundBoxServer server = JsonLoader.LoadFromURL<SoundBoxServer>(url);
            server.IPAddress = ipAddress;
            return server;
        }

        private static void CheckConnection()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                throw new Exception();
        }

        private static string GetLocalIPAddress()
        {

            return DeviceInfo.Connectivity.IpAddress;

            //IPAddress ip;

            //WifiManager wifiManager = (WifiManager)context.GetSystemService(Device.WifiService);
            //int ip = wifiManager.ConnectionInfo.IpAddress;

            //System.Net.Dns
            //var host = Dns.GetHostEntry(Dns.GetHostName());
            //foreach (var ip in host.AddressList)
            //{
            //    if (ip.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        return ip.ToString();
            //    }
            //}
            throw new Exception("Local IP Address Not Found!");
        }

        private static bool CheckForServer(string checkIp)
        {
            string url = string.Format(URL_SYSTEM_API, checkIp);
            var client = new HttpClient();
            client.Timeout = new TimeSpan(0,0,0,0,10);
            try
            {
                var task = Task.Run(async () =>
                {
                    return await client.GetAsync(url);
                });
                var response = task.Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = Task.Run(async () =>
                    {
                        return await response.Content.ReadAsStringAsync();
                    });
                    APIVersion version = JsonConvert.DeserializeObject<APIVersion>(result.Result);
                    return version.HighVersion >= MIN_SUPPORTED_VERSION;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        public SoundBoxServer(string ipAddress)
        {
            this.IPAddress = ipAddress;
        }

        internal string GetAPICodeUrl()
        {
            if (ApiCodeRequired)
                return URL_API_CODE + APICode;
            else
                return "";
        }

    }
}
