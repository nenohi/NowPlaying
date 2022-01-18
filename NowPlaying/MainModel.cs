using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;

namespace NowPlaying
{
    internal class MainModel : BindableBase
    {
        private MainwindowViewModel _mainwindowViewModel;
        private SettingwindowViewModel _settingwindowViewModel;
        private SettingWindow _settingWindow;
        private Spotify _spotify;
        private Misskey misskey = new Misskey();
        private bool _isAlwayTop = false;
        public MainwindowViewModel MainwindowViewModel
        {
            get { return _mainwindowViewModel; }
            set { _mainwindowViewModel = value; }
        }
        public SettingwindowViewModel SettingwindowViewModel
        {
            get { return _settingwindowViewModel; }
            set { _settingwindowViewModel = value; }
        }
        public SettingWindow SettingWindow
        {
            get { return _settingWindow; }
            set { _settingWindow = value; }
        }
        public Spotify Spotify
        {
            get { return _spotify; }
            set { _spotify = value; }
        }

        public bool IsAlwayTop
        {
            get { return _isAlwayTop; }
            set
            {
                _isAlwayTop = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsAlwayTop"));
            }
        }

        public MainModel()
        {
            MainwindowViewModel = new MainwindowViewModel();
            SettingwindowViewModel = new SettingwindowViewModel();
            Spotify = new Spotify();
            MainwindowViewModel.PropertyChanged += MainwindowViewModel_PropertyChanged;
            SettingwindowViewModel.PropertyChanged += MainwindowViewModel_PropertyChanged;
            Spotify.refreshtimer.Elapsed += Refreshtimer_Elapsed;
            ReadSetting();
        }

        private async void Refreshtimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            await RefreshPlayingView();
        }

        public async Task ReadSetting()
        {
            File.Decrypt("APISetting.json");
            using (System.IO.StreamReader r = new System.IO.StreamReader("APISetting.json"))
            {
                string json = r.ReadToEnd();
                Item items = JsonConvert.DeserializeObject<Item>(json);
                if (items != null)
                {
                    Spotify.ClientID = items.ClientID;
                    if (items.SpotifyRefToken != string.Empty)
                    {
                        if(await Spotify.SetToken(items.SpotifyRefToken))
                        {
                            SettingwindowViewModel.Spotifybuttondisable = false;
                            SettingwindowViewModel.SpotifyConnectButton = "Connected";
                        }
                    }
                    if(items.MisskeyToken != string.Empty && items.MisskeyInstanceURL != string.Empty)
                    {
                        misskey.instanceurl = items.MisskeyInstanceURL;
                        SettingwindowViewModel.InputMisskeyInstanceURL=items.MisskeyInstanceURL;
                        misskey.i = items.MisskeyToken;
                        SettingwindowViewModel.MisskeyButtonDisable = false;
                        SettingwindowViewModel.MisskeyConnectButton = "Connected";
                    }
                    SettingwindowViewModel.IsAlwayTop = items.alwaytop;
                    SettingwindowViewModel.MisskeyVisibility = items.MisskeyVisibility;
                }
            }
        }
        public class Item
        {
            public string ClientID;
            public string MisskeyToken;
            public string MisskeyInstanceURL;
            public string SpotifyRefToken;
            public bool alwaytop;
            public string MisskeyVisibility;
        }
        private async void MainwindowViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null) return;
            string[] propertys = e.PropertyName.Split('-');
            if (propertys[0] == "OpenSettingWindow")
            {
                if (SettingWindow == null || !SettingWindow.Activate())
                {
                    SettingWindow = new SettingWindow
                    {
                        DataContext = SettingwindowViewModel
                    };
                    SettingWindow.Show();
                }
                else
                {
                    _settingWindow.WindowState = System.Windows.WindowState.Normal;
                }
            }
            else if (propertys[0] == "SpotifyAuth")
            {
                if (!SettingwindowViewModel.Spotifybuttondisable) return;
                SettingwindowViewModel.Spotifybuttondisable = false;
                await Spotify.GetToken2();
            }
            else if (propertys[0] == "PlayingSend")
            {
                if (!Spotify.IsGetToken) return;
                SpotifyAPI.Web.CurrentlyPlaying playing;
                try
                {
                    playing = await Spotify.GetCurrentlyPlaying();
                }
                catch
                {
                    await Spotify.RefreshTokenFunc();
                    playing = await Spotify.GetCurrentlyPlaying();
                }
                if (misskey.i == "") return;
                if (playing == null || playing.Item == null) return;
                SpotifyAPI.Web.FullTrack track = (SpotifyAPI.Web.FullTrack)playing.Item;
                string artists = "";
                foreach (var artist in track.Artists)
                {
                    artists += "?[" + artist.Name + "](" + artist.ExternalUrls.FirstOrDefault().Value + ")";
                }
                string txt = "Song:[" + track.Name + "](" + track.ExternalUrls["spotify"] + ")\n" +
                    "Artist:" + artists + "\n" +
                    "Album:?[" + track.Album.Name + "](" + track.Album.ExternalUrls["spotify"] + ")\n" +
                    "#NowPlaying";
                await misskey.PostNote(txt, SettingwindowViewModel.MisskeyVisibility);
            }
            else if (propertys[0] == "MisskeyAuth")
            {
                if (SettingwindowViewModel.InputMisskeyInstanceURL == string.Empty) return;
                bool res = await misskey.GetToken(SettingwindowViewModel.InputMisskeyInstanceURL);
                if(misskey.i != string.Empty)
                {
                    SettingwindowViewModel.MisskeyConnectButton = "Connected";
                }
            }
            else if (propertys[0] == "RefreshPlayingView")
            {
                await RefreshPlayingView();
            }
            else if (propertys[0] == "SettingIsAlwayTop")
            {
                IsAlwayTop = SettingwindowViewModel.IsAlwayTop;
            }
            else if (propertys[0] == "PlayerControlCommand")
            {
                switch (propertys[1])
                {
                    case "Previous":
                        await Spotify.PreviousSongs();
                        break;
                    case "PlayResume":
                        await Spotify.PlayResume();
                        break;
                    case "Next":
                        await Spotify.NextSongs();
                        break;
                    default:
                        break;
                }
            }
        }
        public async Task RefreshPlayingView()
        {
            if (!Spotify.IsGetToken) return;

            SpotifyAPI.Web.CurrentlyPlaying playing = await Spotify.GetCurrentlyPlaying();
            if (playing != null && playing.Item != null)
            {
                if (playing.Item.Type == SpotifyAPI.Web.ItemType.Track)
                {
                    SpotifyAPI.Web.FullTrack track = (SpotifyAPI.Web.FullTrack)playing.Item;
                    string artists = "";
                    foreach (var item in track.Artists)
                    {
                        artists += item.Name + "・";
                    }
                    artists = Regex.Replace(artists, "・$", string.Empty);
                    if (MainwindowViewModel.ViewSong != track.Name)
                    {
                        MainwindowViewModel.ViewSong = track.Name;
                        MainwindowViewModel.ViewAlbum = track.Album.Name;
                        MainwindowViewModel.ViewArtist = artists;
                        await MainwindowViewModel.ViewImageURL(track.Album.Images[0].Url);
                    }

                }
            }
        }

        public DelegateCommand WindowClosing
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    using (System.IO.StreamWriter w = new System.IO.StreamWriter("APISetting.json"))
                    {
                        Item items = new Item()
                        {
                            ClientID = Spotify.ClientID ?? string.Empty,
                            MisskeyToken = misskey.i ?? string.Empty,
                            MisskeyInstanceURL = misskey.instanceurl ?? string.Empty,
                            SpotifyRefToken = Spotify.RefreshToken ?? string.Empty,
                            alwaytop = SettingwindowViewModel.IsAlwayTop,
                            MisskeyVisibility = SettingwindowViewModel.MisskeyVisibility
                        };

                        var data = JsonConvert.SerializeObject(items);
                        w.WriteLine(data);
                        try
                        {
                            File.Encrypt("APISetting.json");
                        }
                        catch
                        {

                        }
                    }
                    Spotify.Dispose();
                });
            }
        }

    }
}
