using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;


namespace NowPlaying
{
    internal class MainModel : BindableBase
    {
        private MainwindowViewModel _mainwindowViewModel = new MainwindowViewModel();
        private SettingwindowViewModel _settingwindowViewModel = new SettingwindowViewModel();
        private SettingWindow _settingWindow = new SettingWindow();
        private Spotify _spotify = new Spotify();
        private Misskey misskey = new Misskey();
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


        public MainModel()
        {
            MainwindowViewModel = new MainwindowViewModel();
            SettingwindowViewModel = new SettingwindowViewModel();
            Spotify = new Spotify();
            ReadSetting();
            MainwindowViewModel.PropertyChanged += MainwindowViewModel_PropertyChanged;
            SettingwindowViewModel.PropertyChanged += MainwindowViewModel_PropertyChanged;
            MainwindowViewModel.PropertyChanged += MainwindowViewModel_PropertyChanged;
            SettingwindowViewModel.PropertyChanged += MainwindowViewModel_PropertyChanged;
        }
        public void ReadSetting()
        {
            using System.IO.StreamReader r = new System.IO.StreamReader("Setting.json");
            string json = r.ReadToEnd();
            Item? items = JsonConvert.DeserializeObject<Item>(json);
            Spotify.ClientID = items?.ClientID;
        }
        public class Item
        {
            public string ClientID;
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
            else if (propertys[0] == "SpotifyConnect")
            {
                SettingwindowViewModel.Spotifybuttondisable = false;
                await Spotify.GetToken2();
            }
            else if (propertys[0] == "SpotifyConnectDone")
            {
                SettingwindowViewModel.Spotifybuttondisable = true;
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
                    await Spotify.RefreshToken();
                    playing = await Spotify.GetCurrentlyPlaying();
                }
                if (playing != null)
                {
                    if(playing.Item.Type == SpotifyAPI.Web.ItemType.Track)
                    {
                        SpotifyAPI.Web.FullTrack track = (SpotifyAPI.Web.FullTrack)playing.Item;
                        if (track.Name == MainwindowViewModel.ViewSong) return;
                        MainwindowViewModel.ViewSong = track.Name;
                        MainwindowViewModel.ViewAlbum = track.Album.Name;
                        MainwindowViewModel.ViewArtist = track.Artists[0].Name;
                        MainwindowViewModel.ViewImageURL(track.Album.Images[0].Url);
                        string txt = "Song:[" + track.Name + "](" + track.ExternalUrls["spotify"] + ")\n" +
                            "Artist:"+ track.Artists[0].Name + "\n"+
                            "Album:"+ track.Album.Name + "\n"+
                            "#NowPlaying";
                        if(misskey.i != "")
                        {
                            misskey.PostNote(txt, SettingwindowViewModel.MisskeyVisibility);
                        }
                    }
                }
            }
            else if (propertys[0] == "InputMisskeyI")
            {
                misskey.i = SettingwindowViewModel.InputMisskeyI;
            }
            else if (propertys[0] == "RefreshPlayingView")
            {
                if (!Spotify.IsGetToken) return;

                SpotifyAPI.Web.CurrentlyPlaying playing = await Spotify.GetCurrentlyPlaying();
                if (playing != null)
                {
                    if (playing.Item.Type == SpotifyAPI.Web.ItemType.Track)
                    {
                        SpotifyAPI.Web.FullTrack track = (SpotifyAPI.Web.FullTrack)playing.Item;
                        MainwindowViewModel.ViewSong = track.Name;
                        MainwindowViewModel.ViewAlbum = track.Album.Name;
                        MainwindowViewModel.ViewArtist = track.Artists[0].Name;
                        MainwindowViewModel.ViewImageURL(track.Album.Images[0].Url);

                    }
                }
            }
        }
        public async void RefreshPlayingView()
        {
            if (!Spotify.IsGetToken) return;

            SpotifyAPI.Web.CurrentlyPlaying playing = await Spotify.GetCurrentlyPlaying();
            if (playing != null)
            {
                if (playing.Item.Type == SpotifyAPI.Web.ItemType.Track)
                {
                    SpotifyAPI.Web.FullTrack track = (SpotifyAPI.Web.FullTrack)playing.Item;
                    MainwindowViewModel.ViewSong = track.Name;
                    MainwindowViewModel.ViewAlbum = track.Album.Name;
                    MainwindowViewModel.ViewArtist = track.Artists[0].Name;
                    MainwindowViewModel.ViewImageURL(track.Album.Images[0].Url);

                }
            }
        }
    }
}
