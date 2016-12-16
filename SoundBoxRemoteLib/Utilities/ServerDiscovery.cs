using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;
using SoundBoxRemoteLib.Models;

namespace SoundBoxRemoteLib.Utilities
{
    class ServerDiscovery
    {
        private readonly string m_apiUrl;
        private readonly int m_minSupportedVersion;
        private readonly string m_localIpAddress;


        public ServerDiscovery(string apiUrl, int minSupportedVersion, string localIpAddress)
        {
            m_apiUrl = apiUrl;
            m_minSupportedVersion = minSupportedVersion;
            m_localIpAddress = localIpAddress;
        }

        public List<SoundBoxServer> FindUDP()
        {
            var servers = new List<SoundBoxServer>();
            string[] classes = m_localIpAddress.Split('.');
            var broadcast = classes[0] + "." + classes[1] + "." + classes[2] + ".255";

            var ips = BroadcastUDP(broadcast).Result;
            foreach (var ip in ips)
            {
                servers.Add(new Models.SoundBoxServer(ip));
            }

            return servers;
        }

        public List<SoundBoxServer> FindAll()
        {
            const int START_TIMEOUT_MS = 10;
            const int MAX_TIMEOUT_MS = 250;
            const int DELTA_MS = 50;

            int timeoutMillisecs = START_TIMEOUT_MS;

            List<SoundBoxServer> servers = FindAllServersInternal(timeoutMillisecs);

            while (servers.Count == 0 && (timeoutMillisecs += DELTA_MS) < MAX_TIMEOUT_MS)
            {
                servers = FindAllServersInternal(timeoutMillisecs);
            }

            return servers;
        }


        #region Internal Methods

        private static void CheckConnection()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                throw new Exception();
        }

        private List<SoundBoxServer> FindAllServersInternal(int timeoutMillisecs)
        {
            var servers = new List<SoundBoxServer>();
            object locker = new object();

            CheckConnection();
            string[] classes = m_localIpAddress.Split('.');
            var subnet = classes[0] + "." + classes[1] + "." + classes[2] + ".";

            Parallel.For((long)0, 254, (i, loopState) =>
            {
                try
                {
                    var checkIp = subnet + i;
                    Debug.WriteLine("Checking server {0} using {1}ms timeout", checkIp, timeoutMillisecs);
                    var result = CheckForServer(checkIp, timeoutMillisecs);

                    if (result.TimedOut)
                    {
                        loopState.Stop();
                        return;
                    }

                    if (result.Found)
                    {
                        SoundBoxServer server = SoundBoxServer.LoadFromIP(checkIp);
                        if (server != null)
                        {
                            lock (locker)
                            {
                                servers.Add(server);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ; // ignore
                }
            });

            return servers;
        }

        private ServerCheckResult CheckForServer(string checkIp, int timoutMillisecs)
        {
            ServerCheckResult result = new ServerCheckResult();

            string url = string.Format(m_apiUrl, checkIp);
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMilliseconds(timoutMillisecs);
                try
                {
                    var response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        APIVersion version = JsonConvert.DeserializeObject<APIVersion>(response.Content.ReadAsStringAsync().Result);
                        return new ServerCheckResult { Found = version.HighVersion >= m_minSupportedVersion };
                    }

                    return new ServerCheckResult();
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine("Timeout");
                    return new ServerCheckResult { TimedOut = true };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error thrown: " + ex.Message);
                    return new ServerCheckResult();
                }
            }
        }

        private static async Task<List<string>> BroadcastUDP(string ipAddress)
        {
            const int UDP_TIMEOUT_MS = 4000;

            using (var client = new UdpSocketClient())
            {
                var msg = Encoding.UTF8.GetBytes("SoundBox");
                var replies = new List<string>();
                var port = int.Parse(SoundBoxServer.TCP_SERVER_PORT);

                AutoResetEvent repliedEvent = new AutoResetEvent(false);

                client.MessageReceived += new EventHandler<UdpSocketMessageReceivedEventArgs>(
                    delegate(object sender, UdpSocketMessageReceivedEventArgs args)
                    {
                        var reply = Encoding.UTF8.GetString(args.ByteData, 0, args.ByteData.Length);
                        var tab = Convert.ToChar(9);
                        if (reply.Contains("SoundBox" + tab))
                        {
                            var parts = reply.Split(tab);
                            if (!replies.Contains(parts[1]))
                                replies.Add(parts[1]);
                        }

                        repliedEvent.Set();
                    });

                // unusual! overcomes problem in limitation?
                await client.SendToAsync(msg, ipAddress, port);
                await client.ConnectAsync(ipAddress, port);
                await client.SendAsync(msg);
                
                repliedEvent.WaitOne(TimeSpan.FromMilliseconds(UDP_TIMEOUT_MS));

                return replies;
            }
        }

        #endregion

        #region Internal Objects

        private class ServerCheckResult
        {
            public bool Found { get; set; }
            public bool TimedOut { get; set; }
        }

        #endregion
    }
}
