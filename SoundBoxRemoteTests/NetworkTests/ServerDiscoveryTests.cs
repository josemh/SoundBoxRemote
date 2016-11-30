using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundBoxRemoteLib.Models;
using System.Collections.Generic;

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
    }
}
