using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundBoxRemoteLib.Models;
using System;
using System.Collections.Generic;

namespace SoundBoxRemoteTests.ModelTests
{
    [TestClass]
    public class TalkTimerTests
    {

        [TestInitialize]
        public void TestInit()
        {
            if (SoundBoxServer.ActiveServer == null)
            {
                List<SoundBoxServer> servers = SoundBoxServer.FindAllServers();
                if (servers.Count > 0)
                {
                    servers[0].APICode = "12345";
                    SoundBoxServer.SetActiveServer(servers[0]);
                }
                else
                    Assert.Fail();
            }
        }

        [TestMethod]
        public void TimerLoadAllTest()
        {
            Assert.AreNotEqual(SoundBoxServer.ActiveServer.Timers.Count, 0);
        }

        [TestMethod]
        public void TestRunTimer()
        {
            var server = SoundBoxServer.ActiveServer;
            Assert.AreEqual(server.Timers[0].Status, TalkTimer.TimerStatusEnum.Ready);
            server.Timers[0].PushStatus();
            Assert.AreEqual(server.Timers[0].Status, TalkTimer.TimerStatusEnum.Running);
            Assert.IsNotNull(server.ActiveTimer);
            server.Timers[0].PushStatus();
            Assert.AreEqual(server.Timers[0].Status, TalkTimer.TimerStatusEnum.Stopped);
        }

        [TestMethod]
        public void TestSoundBell()
        {
            var server = SoundBoxServer.ActiveServer;
            var result = server.SoundBell();
            Assert.IsTrue(result);
        }
    }
}
