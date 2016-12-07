using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundBoxRemoteLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoxRemoteTests.ModelTests
{
    public class BaseModelTest
    {
        [TestInitialize]
        public void TestInit()
        {
            if (SoundBoxServer.ActiveServer == null)
            {
                //Use UDP method first (faster)
                List<SoundBoxServer> servers = SoundBoxServer.FindServersUDP();
                if (servers.Count > 0)
                {
                    servers[0].APICode = "12345";
                    servers[0].ApiCodeRequired = true;
                    SoundBoxServer.SetActiveServer(servers[0]);
                }
                else
                {
                    //Fall back on network scan if UDP failed
                    servers = SoundBoxServer.FindAllServers();
                    if (servers.Count > 0)
                    {
                        servers[0].APICode = "12345";
                        SoundBoxServer.SetActiveServer(servers[0]);
                    }
                    else
                        Assert.Fail("No servers found");
                }
            }
        }
    }
}
