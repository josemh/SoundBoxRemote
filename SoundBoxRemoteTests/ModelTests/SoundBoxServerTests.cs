using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundBoxRemoteLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoundBoxRemoteTests.ModelTests
{
    [TestClass]
    public class SoundBoxServerTests : BaseModelTest
    {
        private int _eventCounter;

        [TestMethod]
        public void TestEvents()
        {
            Debug.WriteLine("TestEvents: Current Thread: [" + Thread.CurrentThread.Name + "]");
            var server = SoundBoxServer.ActiveServer;
            Assert.IsNotNull(server, "Server not found");

            server.SoundBoxEvent += Server_SoundBoxEvent;

            Assert.IsTrue(server.SubscribeEvents(), "Event subscription failed");
            Task.Delay(500);

            var song = server.Songs[0];
            song.PlaySong();
            Task.Delay(1000);

            song.StopSong();
            Task.Delay(1000);

            Assert.AreNotEqual(_eventCounter, 0, "No events were detected");

            server.UnSubscribeEvents();
        }

        private void Server_SoundBoxEvent(object sender, SoundBoxEventArgs e)
        {
            Debug.WriteLine("Server_SoundBoxEvent: Current Thread: [" + Thread.CurrentThread.Name + "]");
            Debug.WriteLine(e.EventInfo.EventType.ToString());
            _eventCounter++;
        }
    }
}
