using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NowPlaying
{
    internal class Misskey
    {
        public string i { get; set; } = "";
        public async Task PostNote(string Text, string Visibility)
        {
            Dictionary<string, string> postdata = new Dictionary<string, string>();
            postdata.Add("text", Text);
            postdata.Add("visibility", Visibility==""?"public":Visibility);
            postdata.Add("i", i);
            var postjson = JsonConvert.SerializeObject(postdata);
            using(HttpClient client = new HttpClient())
            {
                var content = new StringContent(postjson, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://misskey.io/api/notes/create", content);
            }

        }
    }
}
