using Acr.DeviceInfo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        private static SoundBoxServer activeServer;
        public static SoundBoxServer ActiveServer { get { return activeServer; } }

        private const int MIN_SUPPORTED_VERSION = 6;

        public const string URL_SYSTEM_API = @"http://{0}:8095/api";
        private const string URL_VERSION = @"/v6";
        private const string URL_SYSTEM_INFO = URL_SYSTEM_API + URL_VERSION + @"/system";
        private const string URL_API_CODE = @"?ApiCode=";
        private const string URL_BELL = "bell";

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

        private List<TalkTimer> m_timers;
        public List<TalkTimer> Timers
        {
            get
            {
                if (m_timers == null)
                {
                    m_timers = TalkTimer.GetFromServer(this);
                }
                return m_timers;
            }
        }

        public TalkTimer ActiveTimer
        {
            get
            {
                var index = Timers[0].RunningIndex;
                if (index != -1)
                    return Timers[index];
                else
                    return null;
            }
        }

        #region Global Instantiator

        public static void SetActiveServer(SoundBoxServer activeServer)
        {
            SoundBoxServer.activeServer = activeServer;
        }

        public static List<SoundBoxServer> FindAllServers()
        {
            var servers = new List<SoundBoxServer>();
            CheckConnection();
            var ip = GetLocalIPAddress();
            string[] classes = ip.Split('.');
            var subnet = classes[0] + "." + classes[1] + "." + classes[2] + ".";

            Task<SoundBoxServer>[] task = new Task<SoundBoxServer>[255];
            for (int i = 0; i < 255; i++)
            {
                task[i] = Task<SoundBoxServer>.Factory.StartNew(() =>
                {
                    var checkIp = subnet + i.ToString();
                    SoundBoxServer server = null;
                    Debug.WriteLine("Checking server {0}", checkIp);
                    if (CheckForServer(checkIp))
                        server = SoundBoxServer.LoadFromIP(checkIp);

                    return server;
                });
                if (task[i].Result != null)
                    servers.Add(task[i].Result);
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
        }

        private static bool CheckForServer(string checkIp)
        {
            string url = string.Format(URL_SYSTEM_API, checkIp);
            var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 0, 0, 10);
            try
            {
                var response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    APIVersion version = JsonConvert.DeserializeObject<APIVersion>(result);
                    return version.HighVersion >= MIN_SUPPORTED_VERSION;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error thrown: " + ex.ToString());
                return false;
            }
        }

        #endregion

        public SoundBoxServer(string ipAddress)
        {
            this.IPAddress = ipAddress;
        }

        public bool SoundBell()
        {
            var json = PostUrl(URL_BELL);
            if (json.Length > 0)
            {
                var jobj = JObject.Parse(json);
                return bool.Parse(jobj["success"].ToString());
            }
            return false;
        }

        internal string GetAPICodeUrl()
        {
            if (ApiCodeRequired)
                return URL_API_CODE + APICode;
            else
                return "";
        }

        internal string GetUrl(string suffix)
        {
            var url = string.Format("{0}{1}/{2}{3}",
                string.Format(URL_SYSTEM_API, IPAddress),
                URL_VERSION,
                suffix.TrimEnd('/'),
                ApiCodeRequired ? string.Format("{0}{1}", URL_API_CODE, APICode) : ""
                );
            return url;
        }
        internal string GetUrl(string suffix1, string suffix2)
        {
            var url = string.Format("{0}{1}/{2}/{3}{4}",
            string.Format(URL_SYSTEM_API, IPAddress),
            URL_VERSION,
            suffix1.TrimEnd('/'),
            suffix2.TrimEnd('/'),
            ApiCodeRequired ? string.Format("{0}{1}", URL_API_CODE, APICode) : ""
            );
            return url;
        }

        internal string GetJson(string urlSuffix)
        {
            string json = "";

            var url = GetUrl(urlSuffix);            
            var client = new HttpClient();

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
                    json = result.Result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return json;
        }

        internal T LoadObject<T>(string urlSuffix)
        {
            T obj = default(T);
            obj = JsonConvert.DeserializeObject<T>(GetJson(urlSuffix));
            return obj;
        }

        internal T LoadObject<T>(string urlSuffix, string jsonPath)
        {
            T obj = default(T);
            var json = GetJson(urlSuffix);
            var jobj = JObject.Parse(json);
            obj = JsonConvert.DeserializeObject<T>(jobj[jsonPath].ToString());
            return obj;
        }

        internal string PostUrl(string urlSuffix)
        {
            var url = GetUrl(urlSuffix);
            var client = new HttpClient();
            var result = client.PostAsync(url, null).Result;
            return result.Content.ReadAsStringAsync().Result;
        }
        internal string PostUrl(string urlSuffix, string value)
        {
            //Build the URL using suffix and APICode if needed
            var url = GetUrl(urlSuffix, value);
            var client = new HttpClient();
            var result = client.PostAsync(url, null).Result;
            return result.Content.ReadAsStringAsync().Result;
        }

    }
}
