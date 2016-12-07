using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundBoxRemoteLib.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoundBoxRemoteTests.ModelTests
{
    [TestClass]
    public class MediaTests : BaseModelTest
    {
         
        [TestMethod]
        public void TestMediaLoadAll()
        {
            Assert.AreNotEqual(SoundBoxServer.ActiveServer.MediaItems.Count, 0, "No media was loaded");
        }

        [TestMethod]
        public void TestPlayVideo()
        {
            var server = SoundBoxServer.ActiveServer;
            var video = server.MediaItems.First(m => m.Type == Media.MediaTypeEnum.Video);
            Assert.IsNotNull(video, "No video media found");

            Assert.IsTrue(video.Play(), "Video did not play");
            Thread.Sleep(1500);

            server.MediaStatus.Update();
            Assert.AreEqual(server.MediaStatus.Status, MediaStatus.MediaStatusEnum.Active, "Video media status did not update");
            Assert.AreEqual(video.Id, server.MediaStatus.Id, "Wrong video media Id being reported");

            Assert.IsTrue(video.Pause(), "Video did not pause");
            Thread.Sleep(1000);

            server.MediaStatus.Update();
            Assert.AreEqual(server.MediaStatus.Status, MediaStatus.MediaStatusEnum.Paused, "Video media status did not update");

            Assert.IsTrue(video.Stop(), "Video did not stop");
            Thread.Sleep(1000);

            server.MediaStatus.Update();
            Assert.AreEqual(server.MediaStatus.Status, MediaStatus.MediaStatusEnum.Inactive, "Video media status did not update");
        }

        [TestMethod]
        public void TestShowImage()
        {
            var server = SoundBoxServer.ActiveServer;
            var image = server.MediaItems.Find(m => m.Type == Media.MediaTypeEnum.Image);

            Assert.IsNotNull(image, "No image loaded");

            Assert.IsTrue(image.Play(), "Image did not display");
            Thread.Sleep(1500);

            server.MediaStatus.Update();
            Assert.AreEqual(server.MediaStatus.Status, MediaStatus.MediaStatusEnum.Active, "Image media status not updating");
            
            Assert.IsTrue(image.Stop(), "Image did not stop");
            Thread.Sleep(1000);

            server.MediaStatus.Update();
            Assert.AreEqual(server.MediaStatus.Status, MediaStatus.MediaStatusEnum.Inactive, "Image media status not updating");
        }

    }
}
