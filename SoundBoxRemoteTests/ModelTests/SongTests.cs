using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundBoxRemoteLib.Models;
using System.Threading.Tasks;

namespace SoundBoxRemoteTests.ModelTests
{
    [TestClass]
    public class SongTests : BaseModelTest
    {
        const int SLEEP_MS = 6000;

        [TestMethod]
        public void TestLoadSongs()
        {
            Assert.AreNotEqual(SoundBoxServer.ActiveServer.Songs.Count, 0);
        }

        [TestMethod]
        public void TestSetNewSong()
        {
            Assert.IsTrue(SoundBoxServer.ActiveServer.Songs[0].SetNewSong(100));
            Assert.IsTrue(SoundBoxServer.ActiveServer.Songs[1].SetNewSong(101));
            Assert.IsTrue(SoundBoxServer.ActiveServer.Songs[2].SetNewSong(102));
        }

        [TestMethod]
        public void TestPlaySong()
        {
            var server = SoundBoxServer.ActiveServer;
            Assert.IsTrue(server.Songs[0].PlaySong());
            Assert.IsFalse(server.Songs[0].PlaySong(), "Same song already playing, should not have played again");
            Assert.IsFalse(server.Songs[1].PlaySong(), "Another song is playing, should not have been allowed");

            Thread.Sleep(SLEEP_MS);
            
            Assert.IsTrue(server.Songs[0].StopSong(), "Could not stop song");
        }

        [TestMethod]
        public void TestBackgroundMusic()
        {
            var server = SoundBoxServer.ActiveServer;
            Assert.IsTrue(server.PlayBackgroundMusic());
            Assert.IsFalse(server.PlayBackgroundMusic(), "Music is already playing. This should have failed");

            Thread.Sleep(SLEEP_MS);

            Assert.IsTrue(server.StopBackgroundMusic());
            Assert.IsFalse(server.StopBackgroundMusic(), "Music is already stopped. This should have failed");
        }

    }
}
