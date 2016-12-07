using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundBoxRemoteLib.Models;
using System.Threading.Tasks;

namespace SoundBoxRemoteTests.ModelTests
{
    [TestClass]
    public class TalkTimerTests : BaseModelTest
    {

        [TestMethod]
        public void TestTimerLoadAll()
        {
            Assert.AreNotEqual(SoundBoxServer.ActiveServer.Timers.Count, 0, "No timers loaded");
        }

        [TestMethod]
        public void TestRunTimer()
        {
            var server = SoundBoxServer.ActiveServer;
            server.Timers[0].PushStatus();

            // SoundBox delays timer start by 1500 ms to allow display to sync up.  
            // Must include longer delay here to allow timer to actually start
            Task.Delay(2000);

            Assert.AreEqual(server.Timers[0].Status, TalkTimer.TimerStatusEnum.Running, "Timer did not run");
            Assert.IsNotNull(server.ActiveTimer, "No active timer found");
            server.Timers[0].PushStatus();

            Task.Delay(1000);

            Assert.AreEqual(server.Timers[0].Status, TalkTimer.TimerStatusEnum.Stopped, "Timer did not stop");

            //Reset timer
            server.Timers[0].PushStatus();
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
