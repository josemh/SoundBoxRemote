using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundBoxRemoteLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoundBoxRemoteTests.ModelTests
{
    [TestClass]
    public class MediaTests
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
        public void TestMediaLoadAll()
        {
            Assert.AreNotEqual(SoundBoxServer.ActiveServer.MediaItems.Count, 0);
        }

        [TestMethod]
        public void TestPlayVideo()
        {
            var server = SoundBoxServer.ActiveServer;
            var video = server.MediaItems.First(m => m.Type == Media.MediaTypeEnum.Video);
            Assert.IsNotNull(video);

            Assert.IsTrue(video.Play());
            server.MediaStatus.Update();
            Assert.AreEqual(server.MediaStatus.Status, MediaStatus.MediaStatusEnum.Active);
            Assert.AreEqual(video.Id, server.MediaStatus.Id);

            Thread.Sleep(500);

            Assert.IsTrue(video.Pause());
            server.MediaStatus.Update();
            Assert.AreEqual(server.MediaStatus.Status, MediaStatus.MediaStatusEnum.Paused);

            Assert.IsTrue(video.Stop());
            server.MediaStatus.Update();
            Assert.AreEqual(server.MediaStatus.Status, MediaStatus.MediaStatusEnum.Inactive);
        }

        [TestMethod]
        public void TestShowImage()
        {
            var server = SoundBoxServer.ActiveServer;
            var image = server.MediaItems.Find(m => m.Type == Media.MediaTypeEnum.Image);

            Assert.IsTrue(image.Play());
            server.MediaStatus.Update();
            Assert.AreEqual(server.MediaStatus.Status, MediaStatus.MediaStatusEnum.Active);

            Assert.IsTrue(image.Stop());
            server.MediaStatus.Update();
            Assert.AreEqual(server.MediaStatus.Status, MediaStatus.MediaStatusEnum.Inactive);
        }

    }
}
