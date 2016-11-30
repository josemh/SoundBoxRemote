using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoxRemoteLib.Utilities
{
    public class JsonLoader
    {
        public static T LoadFromURL<T>(string url)
        {
            T obj = default(T);
            obj = JsonConvert.DeserializeObject<T>(JsonLoader.GetJsonFromURL(url));
            return obj;
        }

        public static T LoadFromURL<T>(string url, string jsonPath)
        {
            T obj = default(T);
            var json = JsonLoader.GetJsonFromURL(url);
            var jobj = JObject.Parse(json);            
            obj = JsonConvert.DeserializeObject<T>(jobj[jsonPath].ToString());
            return obj;
        }

        internal static string GetJsonFromURL(string url)
        {
            string json = "";
            var client = new HttpClient();
            try
            {
                var task = Task.Run(async () =>
                {
                    return await client.GetAsync(url);
                });
                var response = task.Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = Task.Run(async () =>
                    {
                        return await response.Content.ReadAsStringAsync();
                    });
                    json = result.Result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return json;
        }

        //public static List<T> LoadListFromURL<T>(string url)
        //{
        //    var list = new List<T>();
        //}

    }
}
