using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoxRemoteLib.Models
{
    public class MediaStatus : BaseModel
    {
        private const string URL_SUFFIX = "media-status";

        public enum MediaStatusEnum
        {
            Inactive,
            Active,
            Paused
        }

        public MediaStatusEnum Status { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public Media.MediaTypeEnum Type { get; set; }
        public string SlideId { get; set; }
        public int SlideIndex { get; set; }
        public int Duration { get; set; }
        public int Position { get; set; }

        private bool _updating = false;

        public MediaStatus(SoundBoxServer server)
        {
            _server = server;
        }

        public void Update()
        {
            if (!_updating)
            {
                try
                {
                    _updating = true;

                    var status = BaseModel.GetFromServer<MediaStatus>(_server, URL_SUFFIX);

                    this.Status = status.Status;
                    this.Id = status.Id;
                    this.Title = status.Title;
                    this.Type = status.Type;
                    this.SlideId = status.SlideId;
                    this.SlideIndex = status.SlideIndex;
                    this.Duration = status.Duration;
                    this.Position = status.Position;
                }
                finally
                {
                    _updating = false;
                }
            }
        }

        public bool Play()
        {
            return DoAction("play");
        }

        public bool Stop()
        {
            return DoAction("stop");
        }

        private JObject GetPostJson(string action)
        {
            var jobj = new JObject();
            jobj.Add("action", action);
            return jobj;
        }

        private bool DoAction(string action)
        { 
            var jobj = GetPostJson(action);
            var json = _server.PostUrlWithPayload(URL_SUFFIX, jobj.ToString());
            if (json.Length > 0)
            {
                return true;
            }
            return false;
        }        
    }
}
