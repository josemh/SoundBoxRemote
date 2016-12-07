using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoxRemoteLib.Models
{
    public class SoundBoxEventArgs : EventArgs
    {
        public EventInfo EventInfo { get; set; }

        public SoundBoxEventArgs(string json)
        {
            EventInfo = JsonConvert.DeserializeObject<EventInfo>(json);
        }

        public SoundBoxEventArgs(EventInfo.EventTypeEnum eventType, DateTime stamp, string server)
        {
            EventInfo = new EventInfo { EventType = eventType, Stamp = stamp, Server = server };
        }
    }
}
