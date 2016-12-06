using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoxRemoteLib.Models
{
    public class Song
    {

        private const string URL_SONG_SUFFIX = "songs";

        public enum SongStatusEnum
        {
            Ready,
            Playing,
            Empty
        }

        private SoundBoxServer _server; 

        public int Index { get; set; }
        public string InternalName { get; set; }
        public string LocalisedName { get; set; }
        public string Title { get; set; }
        public SongStatusEnum Status { get; set; }
        public bool IsEnabled { get; set; }
        public int SongNumber { get; set; }
        public int LengthMillisecs { get; set; }

        #region Global Initiator

        public static List<Song> GetFromServer(SoundBoxServer server)
        {
            List<Song> songs;

            songs = server.LoadObject<List<Song>>(URL_SONG_SUFFIX, "songInfo");
            foreach (var song in songs)
            {
                song._server = server;
            }
            return songs;
        }

        #endregion

        public bool SetNewSong(int newSong)
        {
            var json = _server.PostUrl(URL_SONG_SUFFIX, Index.ToString(), newSong.ToString());
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
            var songs = Song.GetFromServer(_server);

            if (songs.Any(s => s.Status != SongStatusEnum.Ready))
                // A song is already playing
                return false;
            else if (songs[Index].Status == SongStatusEnum.Ready)
            {
                var json = _server.PostUrl(URL_SONG_SUFFIX, Index.ToString());
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
