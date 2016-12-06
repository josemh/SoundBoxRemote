using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SoundBoxRemoteLib.Models
{
    public class Media : BaseModel
    {
        private const string URL_SUFFIX = "media";
        private const string JSON_LIST_INDEX = "mediaInfo";
        private const string LISTING_CODE_REGEX = @"^[M|W][1-3]-[0-9][0-9][0-9]";
        private const string THUMBNAIL_SMALL = "thumbs64";
        private const string THUMBNAIL_LARGE = "thumbs256";

        public enum MediaTypeEnum
        {
            Video,
            Image,
            Url,
            Pdf,
            Audio,
            Slideshow
        }

        public enum MeetingEnum
        {
            Unknown,
            Weekend,
            Weekday
        }

        public enum MediaActionEnum
        {
            Play,
            Stop,
            Pause,
            Next,
            Prev,
            Blank
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public MediaTypeEnum Type { get; set; }
        public int Duration { get; set; }
        public MeetingEnum Meeting { get; set; }
        public int MeetingSection { get; set; }
        public int Order { get; set; }
        public Image Thumbnail { get; set; }
        

        public static List<Media> GetList(SoundBoxServer server)
        {
            var items = GetListFromServer<Media>(server, URL_SUFFIX, JSON_LIST_INDEX);
            foreach (var item in items)
            {
                if (Regex.IsMatch(item.Title, LISTING_CODE_REGEX))
                {
                    item.Meeting = item.Title.StartsWith("M") ? MeetingEnum.Weekday : MeetingEnum.Weekend;
                    item.MeetingSection = int.Parse(item.Title.Substring(1, 1));
                    item.Order = int.Parse(item.Title.Substring(3, 3));
                    item.Title = item.Title.Substring(7);

                    //item.Thumbnail = server.GetImage(URL_SUFFIX, THUMBNAIL_SMALL, item.Id);
                    switch (item.Type)
                    {
                        case MediaTypeEnum.Video:
                            break;
                        case MediaTypeEnum.Image:
                            break;
                        case MediaTypeEnum.Url:
                            break;
                        case MediaTypeEnum.Pdf:
                            break;
                        case MediaTypeEnum.Audio:
                            break;
                        case MediaTypeEnum.Slideshow:
                            break;
                        default:
                            break;
                    }
                }
            }
            return items;
        }

        #region Media Control

        private JObject GetPostJson(string action)
        {
            var jobj = new JObject();
            jobj.Add("id", Id);
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

        public bool Play()
        {
            return DoAction("play");
        }

        public bool Pause()
        {
            return DoAction("pause");
        }

        public bool Stop()
        {
            return DoAction("stop");
        }

        public bool Next()
        {
            return DoAction("next");
        }

        public bool Previous()
        {
            return DoAction("prev");
        }

        public bool DisplayBlank()
        {
            return DoAction("blank");
        }

        #endregion
    }
}
