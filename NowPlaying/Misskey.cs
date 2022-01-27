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
        public string i { get; set; } = string.Empty;
        public string instanceurl { get; set; } = "https://misskey.io";
        private string appSecret { get; set; } = string.Empty;
        private string token { get; set; } = string.Empty;
        public SpotifyAPI.Web.FullTrack Postedtrack { get; set; } = new SpotifyAPI.Web.FullTrack();

        public async Task PostNote(string Text, string Visibility, SpotifyAPI.Web.CurrentlyPlaying playing)
        {
            if (i == String.Empty) return;
            SpotifyAPI.Web.FullTrack track = (SpotifyAPI.Web.FullTrack)playing.Item;
            if(Postedtrack.Artists != null && track.Artists != null)
            {
                if (track.Artists[0].Name == Postedtrack.Artists[0].Name && track.Name == Postedtrack.Name && Visibility != "specified") return;
            }
            string artists = "";
            foreach (var artist in track.Artists)
            {
                artists += "?[" + artist.Name + "](" + artist.ExternalUrls.FirstOrDefault().Value + ")";
            }
            string txt = Text;
            string Song = "[" + track.Name + "](" + track.ExternalUrls["spotify"] + ")";
            string Album = "?[" + track.Album.Name + "](" + track.Album.ExternalUrls["spotify"] + ")";
            string Playlist = string.Empty;
            if (playing.Context != null && playing.Context.Type == "playlist")
            {
                Playlist = playing.Context.ExternalUrls["spotify"];
            }
            txt = txt.Replace("${Artist}", artists).Replace("${Song}", Song).Replace("${Album}", Album).Replace("${PlaylistURL}", Playlist) + "\n#NowPlaying";
            Dictionary<string, string> postdata = new Dictionary<string, string>();
            postdata.Add("text", txt);
            postdata.Add("visibility", Visibility == "" ? "public" : Visibility);
            postdata.Add("i", i);
            var postjson = JsonConvert.SerializeObject(postdata);
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(postjson, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(instanceurl + "/api/notes/create", content);
            }
            Postedtrack = track;
        }
        public async Task<bool> GetToken(string url)
        {
            var httpsmatch = System.Text.RegularExpressions.Regex.Match(url, "https://");
            string tmpurl=string.Empty;
            if (httpsmatch.Success)
            {
                tmpurl = url;
            }
            else
            {
                tmpurl = "https://" + url;
            }
            if(tmpurl != instanceurl)
            {
                appSecret = string.Empty;
                token = string.Empty;
            }
            if (appSecret == string.Empty && token == string.Empty)
            {

                string appcreateresponse = await CreateApp(url);
                if (appcreateresponse == null) return false;
                AppCreateRes appres;
                AuthSessionGenRes authsegenres;
                appres = JsonConvert.DeserializeObject<AppCreateRes>(appcreateresponse);
                appSecret = appres.secret;
                string authres = await AuthSessionGen(appres.secret);
                authsegenres = JsonConvert.DeserializeObject<AuthSessionGenRes>(authres);
                token = authsegenres.token;
                try
                {
                    System.Diagnostics.Process.Start(authsegenres.url);
                    return true;
                }
                catch
                {
                    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                    {
                        url = authsegenres.url.Replace("&", "^&");
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                        return true;
                    }
                    else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                    {
                        System.Diagnostics.Process.Start("xdg-open", url);
                        return true;

                    }
                    else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                    {
                        System.Diagnostics.Process.Start("open", url);
                        return true;

                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                string authseuserres = await AuthSessionUserkey();
                AuthSessionUserkeyRes authSessionUserkeyRes;
                try
                {
                    authSessionUserkeyRes = JsonConvert.DeserializeObject<AuthSessionUserkeyRes>(authseuserres);
                    i = authSessionUserkeyRes.accessToken;
                }
                catch(Exception ex)
                {
                    NLogService.logger.Error(ex, "Unknown Error By GetToken");
                    return false;
                }
                return true;
            }
        }
        public async Task<string> CreateApp(string url)
        {
            AppCreate postdata = new AppCreate()
            {
                name = "NowPlayingSpotify",
                description = "POSTNote NowPlaying CreatedBy @nenohi@misskey.io",
                permission = new List<string> { "write:notes" }
            };
            var postjson = JsonConvert.SerializeObject(postdata);
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(postjson, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(instanceurl + "/api/app/create", content);
                var res = await response.Content.ReadAsStringAsync();
                return res;
            }
        }
        public async Task<string> AuthSessionGen(string appSecret)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("appSecret", appSecret);
            var postjson = JsonConvert.SerializeObject(data);
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(postjson, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(instanceurl + "/api/auth/session/generate", content);
                var res = await response.Content.ReadAsStringAsync();
                return res;
            }
        }
        public async Task<string> AuthSessionUserkey()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("appSecret", appSecret);
            data.Add("token", token);
            var postjson = JsonConvert.SerializeObject(data);
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(postjson, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(instanceurl + "/api/auth/session/userkey", content);
                var res = await response.Content.ReadAsStringAsync();
                return res;
            }
        }

        public async Task<bool> CheckToken(string itoken)
        {
            using (HttpClient client = new HttpClient())
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("i", itoken);
                var postjson = JsonConvert.SerializeObject(data);
                var content = new StringContent(postjson, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(instanceurl + "/api/i", content);
                if(response.IsSuccessStatusCode)
                {
                    i = itoken;
                    return true;
                }
                return false;
            }
        }

        public class AppCreate
        {
            public string name { get; set; }
            public string description { get; set; }
            public List<string> permission { get; set; }
        }
        public class AppCreateRes
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public List<string> permission { get; set; }
            public string secret { get; set; }
            public string callbackUrl { get; set; }
            public bool isAuthorized { get; set; }
        }
        public class AuthSessionGenRes
        {
            public string token { get; set; }
            public string url { get; set; }
        }
        public class AuthSessionUserkeyRes
        {
            public string accessToken { get; set; }
        }
    }
}
