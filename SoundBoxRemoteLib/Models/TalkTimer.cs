using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoundBoxRemoteLib.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoxRemoteLib.Models
{
    public class TalkTimer
    {
        private const string URL_TIMER_SUFFIX = "timers";
        private const string URL_TIMER_LIST = SoundBoxServer.URL_SYSTEM_API + @"/" + URL_TIMER_SUFFIX;
        
        public enum TimerStatusEnum
        {
            Ready,
            Running,
            Stopped
        }

        private SoundBoxServer _server;

        public int TimerIndex { get; set; }
        public string InternalName { get; set; }
        public string LocalisedTitle { get; set; }
        public string LocalisedTabName { get; set; }
        public string InternalTabName { get; set; }
        public TimerStatusEnum Status { get; set; }
        public int Index { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsApplicable { get; set; }
        public bool IsStudentTalk { get; set; }
        public int PlannedAllocationSecs { get; set; }
        public int ActualAllocationSecs { get; set; }
        public int ElapsedMillisecs { get; set; }
        public int DisplayedMins { get; set; }
        public int DisplayedSecs { get; set; }
        public int RunningIndex { get; set; }

        #region Global Initiator

        public static List<TalkTimer> GetFromServer(SoundBoxServer server)
        {
            List<TalkTimer> timers;

            timers = server.LoadObject<List<TalkTimer>>(URL_TIMER_SUFFIX, "timerInfo");
            foreach (var timer in timers)
            {
                timer._server = server;
            }
            return timers;
        }

        #endregion

        public void PushStatus()
        {
            if (_server != null)
            {
                //var server = SoundBoxServer.ActiveServer;
                var json = _server.PostUrl(URL_TIMER_SUFFIX, Index.ToString());                
                if (json.Length > 0)
                {
                    var jobj = JObject.Parse(json);
                    var timer = JsonConvert.DeserializeObject<TalkTimer>(json);
                    this.Status = timer.Status;

                    foreach (var item in _server.Timers)
                    {
                        item.RunningIndex = timer.RunningIndex;
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("Server not set on timer");
            }
        }
    }
}
