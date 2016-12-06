using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundBoxRemoteLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoxRemoteTests.ModelTests
{
    [TestClass]
    public class SongTests
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
        public void TestLoadSongs()
        {
            Assert.AreNotEqual(SoundBoxServer.ActiveServer.Songs.Count, 0);
        }

        [TestMethod]
        public void TestSetNewSong()
        {
            Assert.IsTrue(SoundBoxServer.ActiveServer.Songs[0].SetNewSong(100));
        }

        [TestMethod]
        public void TestPlaySong()
        {
            var server = SoundBoxServer.ActiveServer;
            Assert.IsTrue(server.Songs[0].PlaySong());
            Assert.IsFalse(server.Songs[0].PlaySong(), "Same song already playing, should not have played again");
            Assert.IsFalse(server.Songs[1].PlaySong(), "Another song is playing, should not have been allowed");
        }

        [TestMethod]
        public void TestBackgroundMusic()
        {
            var server = SoundBoxServer.ActiveServer;
            Assert.IsTrue(server.PlayBackgroundMusic());
            Assert.IsFalse(server.PlayBackgroundMusic(), "Music is already playing. This should have failed");

            Assert.IsTrue(server.StopBackgroundMusic());
            Assert.IsFalse(server.StopBackgroundMusic(), "Music is already stopped. This should have failed");
        }

    }
}
