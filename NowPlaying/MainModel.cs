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
using NLog;
using System.Windows.Media;

namespace NowPlaying
{
    internal class MainModel : BindableBase
    {
        private MainwindowViewModel _mainwindowViewModel;
        private SettingwindowViewModel _settingwindowViewModel;
        private SettingWindow _settingWindow;
        private Spotify _spotify;
        private Misskey misskey = new();
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
            if (!Spotify.IsGetToken)
            {
                SettingwindowViewModel.Spotifybuttondisable = true;
            }
            else
            {
                SettingwindowViewModel.Spotifybuttondisable = false;
            }
            try
            {
                await RefreshPlayingView();
            }
            catch (System.Net.Http.HttpRequestException err)
            {
                NLogService.logger.Error(err);
            }
        }

        public async Task ReadSetting()
        {
            NLogService.PrintInfoLog("Loading SettingFile");
            if (!File.Exists("APISetting.json"))
            {
                File.Copy("DefaultAPISetting.json", "APISetting.json");
            }
            else
            {
                File.Decrypt("APISetting.json");
            }
            using (StreamReader r = new StreamReader("APISetting.json"))
            {
                string json = r.ReadToEnd();
                Item? items = JsonConvert.DeserializeObject<Item>(json);
                if (items != null)
                {
                    Spotify.ClientID = items.ClientID;
                    if (items.SpotifyRefToken != string.Empty)
                    {
                        if (await Spotify.SetToken(items.SpotifyRefToken))
                        {
                            SettingwindowViewModel.Spotifybuttondisable = false;
                            SettingwindowViewModel.SpotifyConnectButton = "Connected";
                            NLogService.PrintInfoLog("Spotify Token OK");
                        }
                    }
                    if (items.MisskeyToken != string.Empty && items.MisskeyInstanceURL != string.Empty)
                    {
                        misskey.instanceurl = items.MisskeyInstanceURL;
                        SettingwindowViewModel.InputMisskeyInstanceURL = items.MisskeyInstanceURL;
                        if (await misskey.CheckToken(items.MisskeyToken))
                        {
                            SettingwindowViewModel.MisskeyButtonDisable = false;
                            SettingwindowViewModel.MisskeyConnectButton = "Connected";
                            NLogService.PrintInfoLog("MisskeyInstance Connected");
                        }
                        else
                        {
                            SettingwindowViewModel.MisskeyButtonDisable = true;
                            SettingwindowViewModel.MisskeyConnectButton = "Connect";
                        }
                    }
                    SettingwindowViewModel.IsAlwayTop = items.alwaytop;
                    SettingwindowViewModel.MisskeyVisibility = items.MisskeyVisibility;
                    SettingwindowViewModel.SettingBackgroundColorText = items.BackgroundColorText;
                    SettingwindowViewModel.SettingForegroundColorText = items.ForegroundColorText;
                    SettingwindowViewModel.IsAutoChangeColor = items.AutoChangeColor;
                    if(items.SettingPostDataText == string.Empty)
                    {
                        SettingwindowViewModel.SettingPostDataText = "Song:${Song}\nArtist:${Artist}\nAlbum:${Album}\n";
                    }
                    else
                    {
                        SettingwindowViewModel.SettingPostDataText = items.SettingPostDataText;
                    }
                }
            }
            File.Encrypt("APISetting.json");
            NLogService.PrintInfoLog("Loading SettingFile Done");
        }
        public class Item
        {
            public string ClientID = "";
            public string MisskeyToken = "";
            public string MisskeyInstanceURL = "";
            public string SpotifyRefToken = "";
            public bool alwaytop = false;
            public string MisskeyVisibility = "Public";
            public string BackgroundColorText = "White";
            public string ForegroundColorText = "Black";
            public bool AutoChangeColor = false;
            public string SettingPostDataText = "";
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
                if (misskey.i == "") return;

                SpotifyAPI.Web.CurrentlyPlaying playing;
                try
                {
                    playing = await Spotify.GetCurrentlyPlaying();
                }
                catch (Exception ex)
                {
                    NLogService.PrintInfoLog("SpotifyTokenRefresh");
                    await Spotify.RefreshTokenFunc();
                    playing = await Spotify.GetCurrentlyPlaying();
                }
                if (playing == null || playing.Item == null) return;

                await misskey.PostNote(SettingwindowViewModel.SettingPostDataText, SettingwindowViewModel.MisskeyVisibility, playing);
            }
            else if (propertys[0] == "MisskeyAuth")
            {
                if (SettingwindowViewModel.InputMisskeyInstanceURL == string.Empty) return;
                bool res = await misskey.GetToken(SettingwindowViewModel.InputMisskeyInstanceURL);
                if (misskey.i != string.Empty)
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
            else if (propertys[0] == "SettingBackgroundColor")
            {
                MainwindowViewModel.MainBackgroundColor = SettingwindowViewModel.SettingBackgroundColor;
            }
            else if (propertys[0] == "SettingForegroundColor")
            {
                MainwindowViewModel.MainForegroundColor = SettingwindowViewModel.SettingForegroundColor;
            }
            else if (propertys[0] == "ImageAverageBackgroundColor")
            {
                if (SettingwindowViewModel.IsAutoChangeColor)
                {
                    SettingwindowViewModel.AutoChangeBackgroundColor = MainwindowViewModel.ImageAverageBackgroundColor;
                }
            }
            else if (propertys[0] == "ImageAverageForegroundColor")
            {
                if (SettingwindowViewModel.IsAutoChangeColor)
                {
                    SettingwindowViewModel.AutoChangeForegroundColor = MainwindowViewModel.ImageAverageForegroundColor;

                }
            }
            else if (propertys[0] == "IsAutoChangeColor")
            {
                MainwindowViewModel.AutoChangeColor = SettingwindowViewModel.IsAutoChangeColor;
                SettingwindowViewModel.AutoChangeForegroundColor = MainwindowViewModel.ImageAverageForegroundColor;
                SettingwindowViewModel.AutoChangeBackgroundColor = MainwindowViewModel.ImageAverageBackgroundColor;

            }
            else if(propertys[0] == "SettingCheckPostButton")
            {
                if (!Spotify.IsGetToken) return;
                if (misskey.i == "") return;

                SpotifyAPI.Web.CurrentlyPlaying playing;
                try
                {
                    playing = await Spotify.GetCurrentlyPlaying();
                }
                catch (Exception ex)
                {
                    NLogService.PrintInfoLog("SpotifyTokenRefresh");
                    await Spotify.RefreshTokenFunc();
                    playing = await Spotify.GetCurrentlyPlaying();
                }
                if (playing == null || playing.Item == null) return;
                await misskey.PostNote(SettingwindowViewModel.SettingPostDataText, "specified", playing);
            }
            else if(propertys[0] == "RepeatCommand")
            {
                if(!Spotify.IsGetToken) return;
                await Spotify.SetRepeat();
                MainwindowViewModel.Shuffle_Status = Spotify.ShuffleStatus;
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
                    File.Decrypt("APISetting.json");
                    NLogService.PrintInfoLog("Saving SettingFile");
                    using (StreamWriter w = new StreamWriter("APISetting.json"))
                    {
                        Item items = new Item()
                        {
                            ClientID = Spotify.ClientID,
                            MisskeyToken = misskey.i,
                            MisskeyInstanceURL = misskey.instanceurl ?? string.Empty,
                            SpotifyRefToken = Spotify.RefreshToken,
                            alwaytop = SettingwindowViewModel.IsAlwayTop,
                            MisskeyVisibility = SettingwindowViewModel.MisskeyVisibility,
                            BackgroundColorText = SettingwindowViewModel.SettingBackgroundColorText,
                            ForegroundColorText = SettingwindowViewModel.SettingForegroundColorText,
                            AutoChangeColor = SettingwindowViewModel.IsAutoChangeColor,
                            SettingPostDataText = SettingwindowViewModel.SettingPostDataText,
                        };

                        var data = JsonConvert.SerializeObject(items);
                        w.WriteLine(data);
                        try
                        {
                            File.Encrypt("APISetting.json");
                        }
                        catch (Exception e)
                        {
                            NLogService.PrintErrorLog(e, "APISettingFile Encrypte");
                        }
                    }
                    Spotify.Dispose();
                    if (SettingWindow != null)
                    {
                        SettingWindow.Close();
                    }
                    NLogService.PrintInfoLog("Saving SettingFile Done");
                    NLogService.Dispose();
                });
            }
        }

    }
    public static class NLogService
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();

        public static void PrintInfoLog(string str)
        {
            logger.Info(str);
        }
        public static void PrintInfoLog(Exception ex, string str)
        {
            logger.Info(ex, str);
        }
        public static void PrintErrorLog(Exception ex, string str)
        {
            logger.Error(ex, str);
        }
        public static void PrintDebugLog(string str)
        {
            logger.Debug(str);
        }
        public static void PrintDebugLog(Exception ex, string str)
        {
            logger.Debug(ex, str);
        }
        public static void Dispose()
        {
            NLog.LogManager.Shutdown();
        }
    }
}
