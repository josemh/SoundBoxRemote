using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundBoxRemoteLib.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoundBoxRemoteTests.NetworkTests
{
    [TestClass]
    public class ServerDiscoveryTests
    {        
        [TestMethod]
        public void TestFindAllServers()
        {
            List<SoundBoxServer> servers;
            servers = SoundBoxServer.FindAllServers();
            Assert.AreEqual(servers.Count, 1);
        }

        [TestMethod]
        public void TestFindAllServersUDP()
        {
            List<SoundBoxServer> servers;
            servers = SoundBoxServer.FindServersUDP();
            Assert.AreNotEqual(servers.Count, 0);
        }
    }
}
