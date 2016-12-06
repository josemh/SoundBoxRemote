using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoxRemoteLib.Models
{
    public class Song : BaseModel
    {

        private const string URL_SUFFIX = "songs";
        private const string JSON_LIST_INDEX = "songInfo";

        public enum SongStatusEnum
        {
            Ready,
            Playing,
            Empty
        }

        public int Index { get; set; }
        public string InternalName { get; set; }
        public string LocalisedName { get; set; }
        public string Title { get; set; }
        public SongStatusEnum Status { get; set; }
        public bool IsEnabled { get; set; }
        public int SongNumber { get; set; }
        public int LengthMillisecs { get; set; }

        #region Global Initiator

        public static List<Song> GetList(SoundBoxServer server)
        {
            return GetListFromServer<Song>(server, URL_SUFFIX, JSON_LIST_INDEX);
        }

        #endregion

        public bool SetNewSong(int newSong)
        {
            var json = _server.PostUrl(URL_SUFFIX, Index.ToString(), newSong.ToString());
            if (json.Length > 0)
            {
                var song = JsonConvert.DeserializeObject<Song>(json);
                if (song.SongNumber == newSong)
                    return true;
            }
            return false;
        }

        public bool PlaySong()
        {
            var songs = Song.GetList(_server);

            if (songs.Any(s => s.Status != SongStatusEnum.Ready))
                // A song is already playing
                return false;
            else if (songs[Index].Status == SongStatusEnum.Ready)
            {
                var json = _server.PostUrl(URL_SUFFIX, Index.ToString());
                if (json.Length > 0)
                {
                    var song = JsonConvert.DeserializeObject<Song>(json);
                    Status = song.Status;
                    return true;
                }
            }
            //else
            //    throw new InvalidOperationException("Song is already playing");

            return false;
        }

    }
}
