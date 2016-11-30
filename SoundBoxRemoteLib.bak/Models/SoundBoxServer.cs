using System;
using System.Collections.Generic;
using System.Net;
using Xamarin.Forms;

namespace SoundBoxRemoteLib.Models
{
    public class SoundBoxServer
    {
        public static List<SoundBoxServer> FindAllServers()
        {
            try
            {
                CheckConnection();
                var ip = GetLocalIPAddress();
                //var ipRange
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        private static void CheckConnection()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                throw new Exception();
        }

        public static string GetLocalIPAddress()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                var reachability = new System.Confi NetworkReachability("0.0.0.0");
            }

            System.Net.Dns.GetHostName();


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
    }
}
