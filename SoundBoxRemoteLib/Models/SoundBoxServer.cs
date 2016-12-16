using Acr.DeviceInfo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;
using SoundBoxRemoteLib.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SoundBoxRemoteLib.Models
{
    public class SoundBoxServer
    {

        private static SoundBoxServer activeServer;
        public static SoundBoxServer ActiveServer { get { return activeServer; } }

        private const int MIN_SUPPORTED_VERSION = 7;

        public const string TCP_SERVER_PORT = "8095";
        public const string URL_SYSTEM_API = @"http://{0}:" + TCP_SERVER_PORT + "/api";
        private const string URL_VERSION = @"/v7";
        private const string URL_SYSTEM_INFO = URL_SYSTEM_API + URL_VERSION + @"/system";
        private const string URL_API_CODE = @"?ApiCode=";
        private const string URL_BELL = "bell";
        private const string URL_BACKGROUND_MUSIC = "background-music";

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
                    m_timers = TalkTimer.GetList(this);
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

        private List<Song> m_songs;
        public List<Song> Songs
        {
            get
            {
                if (m_songs == null)
                {
                    m_songs = Song.GetList(this);
                }
                return m_songs;
            }
        }

        private List<Media> m_MediaItems;
        public List<Media> MediaItems
        {
            get
            {
                if (m_MediaItems == null)
                {
                    m_MediaItems = Media.GetList(this);
                }
                return m_MediaItems;
            }
        }

        private MediaStatus _mediaStatus;
        public MediaStatus MediaStatus
        {
            get
            {
                if (_mediaStatus == null)
                    _mediaStatus = new MediaStatus(this);
                return _mediaStatus;
            }
        }

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

        public bool PlayBackgroundMusic()
        {
            var music = LoadObject<BackgroundMusic>(URL_BACKGROUND_MUSIC);
            if (music.Status == Song.SongStatusEnum.Ready)
            {
                var request = new JObject();
                request.Add("action", "play");
                var json = PostUrlWithPayload(URL_BACKGROUND_MUSIC, request.ToString());
                if (json.Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool StopBackgroundMusic()
        {
            var music = LoadObject<BackgroundMusic>(URL_BACKGROUND_MUSIC);
            if (music.Status == Song.SongStatusEnum.Playing)
            {
                var request = new JObject();
                request.Add("action", "stop");
                var json = PostUrlWithPayload(URL_BACKGROUND_MUSIC, request.ToString());
                if (json.Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        #region Global Instantiator

        public static void SetActiveServer(SoundBoxServer activeServer)
        {
            SoundBoxServer.activeServer = activeServer;
        }

        public static List<SoundBoxServer> FindAllServers()
        {
            return new ServerDiscovery(URL_SYSTEM_API, MIN_SUPPORTED_VERSION, GetLocalIPAddress()).FindAll();
        }

        public static List<SoundBoxServer> FindServersUDP()
        {
            return new ServerDiscovery(URL_SYSTEM_API, MIN_SUPPORTED_VERSION, GetLocalIPAddress()).FindUDP();
        }

        public static SoundBoxServer LoadFromIP(string ipAddress)
        {
            string url = string.Format(URL_SYSTEM_INFO, ipAddress);
            SoundBoxServer server = JsonLoader.LoadFromURL<SoundBoxServer>(url);
            server.IPAddress = ipAddress;
            return server;
        }

        internal static string GetLocalIPAddress()
        {
            return DeviceInfo.Connectivity.IpAddress;
        }

        #endregion

        #region Internal Methods

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
        internal string GetUrl(string suffix1, string suffix2, string suffix3)
        {
            var url = string.Format("{0}{1}/{2}/{3}/{4}{5}",
            string.Format(URL_SYSTEM_API, IPAddress),
            URL_VERSION,
            suffix1.TrimEnd('/'),
            suffix2.TrimEnd('/'),
            suffix3.TrimEnd('/'),
            ApiCodeRequired ? string.Format("{0}{1}", URL_API_CODE, APICode) : ""
            );
            return url;
        }

        internal string GetJson(string urlSuffix)
        {
            string json = "";

            var url = GetUrl(urlSuffix);
            using (var client = new HttpClient())
            {
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
        }

        internal T LoadObject<T>(string urlSuffix)
        {
            T obj = default(T);
            var json = GetJson(urlSuffix);
            if (json.Length > 0)
                obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }

        internal T LoadObject<T>(string urlSuffix, string jsonPath)
        {
            T obj = default(T);
            var json = GetJson(urlSuffix);
            if (json.Length > 0)
            {
                var jobj = JObject.Parse(json);
                obj = JsonConvert.DeserializeObject<T>(jobj[jsonPath].ToString());
            }
            return obj;
        }

        internal string PostUrl(string urlSuffix)
        {
            var url = GetUrl(urlSuffix);
            using (var client = new HttpClient())
            {
                var result = client.PostAsync(url, null).Result;
                if (result.IsSuccessStatusCode)
                    return result.Content.ReadAsStringAsync().Result;
             
                return "";
            }
        }

        internal string PostUrl(string urlSuffix, string value)
        {
            //Build the URL using suffix and APICode if needed
            var url = GetUrl(urlSuffix, value);
            using (var client = new HttpClient())
            {
                var result = client.PostAsync(url, null).Result;
                if (result.IsSuccessStatusCode)
                    return result.Content.ReadAsStringAsync().Result;
             
                return "";
            }
        }

        internal string PostUrl(string urlSuffix, string value1, string value2)
        {
            var url = GetUrl(urlSuffix, value1, value2);
            using (var client = new HttpClient())
            {
                var result = client.PostAsync(url, null).Result;
                if (result.IsSuccessStatusCode)
                    return result.Content.ReadAsStringAsync().Result;
             
                return "";
            }
        }

        internal string PostUrlWithPayload(string urlSuffix, string payload)
        {
            var url = GetUrl(urlSuffix);
            using (var client = new HttpClient())
            {
                var result = client.PostAsync(url, new StringContent(payload)).Result;
                if (result.IsSuccessStatusCode)
                    return result.Content.ReadAsStringAsync().Result;
             
                return "";
            }
        }

        internal string PostUrlWithPayload(string urlSuffix, string value, string payload)
        {
            var url = GetUrl(urlSuffix, value);
            using (var client = new HttpClient())
            {
                var result = client.PostAsync(url, new StringContent(payload)).Result;
                if (result.IsSuccessStatusCode)
                    return result.Content.ReadAsStringAsync().Result;

                return "";
            }
        }

        internal string PostUrlWithPayload(string urlSuffix, string value1, string value2, string payload)
        {
            var url = GetUrl(urlSuffix, value1, value2);
            using (var client = new HttpClient())
            {
                var result = client.PostAsync(url, new StringContent(payload)).Result;
                if (result.IsSuccessStatusCode)
                    return result.Content.ReadAsStringAsync().Result;
             
                return "";
            }
        }

        internal Image GetImage(string urlSuffix, string value, string id)
        {
            var url = GetUrl(urlSuffix, value, id);
            var image = new Image();
            image.Source = url;
            return image;
        }

        #endregion

        #region Internal Objects

        private class BackgroundMusic
        {
            public Song.SongStatusEnum Status { get; set; }
            public bool AutoStopEnabled { get; set; }
            public bool AutoStopped { get; set; }
        }

        #endregion

        #region Event Handler

        private const int PORT_LISTEN_DEFAULT = 9550;

        private int _listenPort = PORT_LISTEN_DEFAULT;
        private TcpSocketListener _listener;

        public delegate void SoundBoxEventHandler(object sender, SoundBoxEventArgs e);
        public event SoundBoxEventHandler SoundBoxEvent;

        public bool SubscribeEvents()
        {

            if (StartListener().Result)
            {
                var url = GetUrl(EventInfo.GetSubscribeUrlSuffix());
                var jobj = new JObject();
                jobj.Add("address", GetLocalIPAddress());
                jobj.Add("port", _listenPort);

                using (var client = new HttpClient())
                {
                    var result = client.PostAsync(url, new StringContent(jobj.ToString())).Result;

                    if (result.IsSuccessStatusCode)
                        return true;
                }
            }
            return false;
        }

        public bool UnSubscribeEvents()
        {
            if (_listener != null)
            {
                var url = GetUrl(EventInfo.GetUnSubscribeUrlSuffix());
                var jobj = new JObject();
                jobj.Add("address", GetLocalIPAddress());
                jobj.Add("port", _listenPort);

                using (var client1 = new HttpClient())
                {
                    var result = client1.PostAsync(url, new StringContent(jobj.ToString())).Result;

                    if (!result.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("Client disconnected");
                        _listener.StopListeningAsync();
                        return true;
                    }
                }
            }
            return false;
        }

        private async Task<bool> StartListener()
        {
            try
            {
                //TODO: Handle port already open error
                _listener = new TcpSocketListener();
                _listener.ConnectionReceived += Listener_ConnectionReceived;

                await _listener.StartListeningAsync(_listenPort);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        private async void Listener_ConnectionReceived(object sender, TcpSocketListenerConnectEventArgs e)
        {
            Debug.WriteLine("Client connected");

            var msg = new StringBuilder();
            int bytesRead = -1;
            byte[] buf = new byte[5];

            while (bytesRead != 0)
            {
                bytesRead = await e.SocketClient.ReadStream.ReadAsync(buf, 0, 5);
                msg.Append(Encoding.UTF8.GetString(buf, 0, bytesRead));
            }
            RaiseSoundboxEvent(msg.ToString());
        }

        private void RaiseSoundboxEvent(string json)
        {
            if (SoundBoxEvent != null)
                SoundBoxEvent(this, new SoundBoxEventArgs(json));
        }

        #endregion
    }
}
