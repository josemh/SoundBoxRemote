using Newtonsoft.Json;
using Sockets.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoxRemoteLib.Models
{
    public class EventInfo
    {
        private const string URL_SUFFIX = "events";
        private const string ACTION_SUBSCRIBE = "subscribe";
        private const string ACTION_UNSUBSCRIBE = "unsubscribe";

        public enum EventTypeEnum
        {
            StartSong,
            StopSong,
            StartVideo,
            StopVideo,
            PauseVideo,
            RestartVideo,
            OpenBrowser,
            CloseBrowser,
            StartAudio,
            StopAudio,
            ShowImage,
            HideImage,
            StartSlideShow,
            StopSlideShow,
            PauseSlideShow,
            RestartSlideShow,
            ShowLyrics,
            HideLyrics,
            StartCountdown,
            StopCountdown,
            StartBackgroundMusic,
            StopBackgroundMusic,
            CloseApp,
            StartTimer,
            StopTimer,
            StartRecording,
            StopRecording,
            StartSubscription,
            DropSubscription
        }

        public EventTypeEnum EventType { get; set; }
        public DateTime Stamp { get; set; }
        public string Server { get; set; }

        public static string GetSubscribeUrlSuffix()
        {
            return string.Format("{0}/{1}", URL_SUFFIX, ACTION_SUBSCRIBE);
        }

        public static string GetUnSubscribeUrlSuffix()
        {
            return string.Format("{0}/{1}", URL_SUFFIX, ACTION_UNSUBSCRIBE);
        }


    }
}
