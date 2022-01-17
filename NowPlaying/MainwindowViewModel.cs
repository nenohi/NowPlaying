using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Prism.Mvvm;
using Prism.Commands;

namespace NowPlaying
{
    internal class MainwindowViewModel:BindableBase
    {
        private BitmapImage _viewImage = new BitmapImage();
        private string _viewSong ="";
        private string _viewArtist="";
        private string _viewAlbum="";
        public async Task ViewImageURL(string url)
        {
            using (var web = new System.Net.Http.HttpClient())
            {
                var bytes = await web.GetByteArrayAsync(url).ConfigureAwait(false);
                using (var stream = new System.IO.MemoryStream(bytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    ViewImage = bitmap;
                }
            }
        }
        public BitmapImage ViewImage
        {
            get { return _viewImage; }
            set
            {
                _viewImage = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ViewImage"));
            }
        }
        public string ViewSong
        {
            get { return _viewSong; }
            set
            {
                _viewSong = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ViewSong"));
            }
        }
        public string ViewArtist
        {
            get { return _viewArtist; }
            set
            {
                _viewArtist = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ViewArtist"));
            }
        }
        public string ViewAlbum
        {
            get { return _viewAlbum; }
            set
            {
                _viewAlbum = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ViewAlbum"));
            }
        }
        public DelegateCommand OpenSettingWindow
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("OpenSettingWindow"));
                });
            }
        }
        public DelegateCommand PlayingSend
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(PlayingSend)));
                });
            }
        }

        public DelegateCommand<string> PlayerControlCommand
        {
            get
            {
                return new DelegateCommand<string>((par) =>
                {
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("PlayerControlCommand-" + par));
                });
            }
        }
    }
}
