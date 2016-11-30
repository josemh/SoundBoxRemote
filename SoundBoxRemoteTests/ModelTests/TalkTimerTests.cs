using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundBoxRemoteLib.Models;
using System;
using System.Collections.Generic;

namespace SoundBoxRemoteTests.ModelTests
{
    [TestClass]
    public class TalkTimerTests
    {
        [TestMethod]
        public void TimerLoadAllTest()
        {
            List<SoundBoxServer> servers = SoundBoxServer.FindAllServers();
            var timers = new List<TalkTimer>();

            if (servers.Count > 0)
            {
                servers[0].APICode = "12345";
                timers = TalkTimer.GetFromServer(servers[0]);
            }

            Assert.AreNotEqual(timers.Count, 0);
        }
    }
}
