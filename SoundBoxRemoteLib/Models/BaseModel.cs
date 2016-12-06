using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoxRemoteLib.Models
{
    public class BaseModel
    {

        internal SoundBoxServer _server;

        #region Global Initiator

        public static List<T> GetListFromServer<T>(SoundBoxServer server, string urlSuffix, string jsonIndexer) where T : BaseModel
        {
            List<T> items;

            items = server.LoadObject<List<T>>(urlSuffix, jsonIndexer);
            foreach (var song in items)
            {
                song._server = server;
            }
            return items;
        }

        public static T GetFromServer<T>(SoundBoxServer server, string urlSuffix) where T : BaseModel
        {
            var item = server.LoadObject<T>(urlSuffix);
            item._server = server;
            return item;
        }

        #endregion

    }
}
