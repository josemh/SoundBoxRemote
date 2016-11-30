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
        private const string URL_TIMER_LIST = @"http://{0}:8095/api/v6/timers";

        public int TimerIndex { get; set; }
        public string InternalName { get; set; }
        public string LocalisedTitle { get; set; }
        public string LocalisedTabName { get; set; }
        public string InternalTabName { get; set; }
        public int Status { get; set; }
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
            string url = string.Format(URL_TIMER_LIST, server.IPAddress);
            if (server.ApiCodeRequired)
                url += server.GetAPICodeUrl();
            List<TalkTimer> timers;
            timers = JsonLoader.LoadFromURL<List<TalkTimer>>(url, "timerInfo");
            return timers;
        }

        #endregion
    }
}
