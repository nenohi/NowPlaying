using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Prism.Mvvm;
using Prism.Commands;
using System.Windows.Media;
using System.Windows.Threading;

namespace NowPlaying
{
    internal class MainwindowViewModel : BindableBase
    {
        private BitmapImage _viewImage = new BitmapImage();
        private string _viewSong = "";
        private string _viewArtist = "";
        private string _viewAlbum = "";
        private bool _isPlayingSendButton = true;
        private Brush _mainForegroundColor = Brushes.Black;
        private Brush _mainBackgroundColor = Brushes.White;
        private Brush _imageAverageForegroundColor = Brushes.Black;
        private Brush _imageAverageBackgroundColor = Brushes.White;
        private bool _autochangecolor = false;
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
                    int r = 0;
                    int g = 0;
                    int b = 0;
                    int a = 0;
                    int total = 0;
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        this.ViewImage = bitmap;
                        FormatConvertedBitmap comvertbitmap = new FormatConvertedBitmap(this.ViewImage, PixelFormats.Pbgra32, null, 0);
                        comvertbitmap.Freeze();
                        int imgW = comvertbitmap.PixelWidth;
                        int imgH = comvertbitmap.PixelHeight;
                        byte[] buffer = new byte[(int)(4 * imgW * imgH)];
                        int stride = (imgW * bitmap.Format.BitsPerPixel + 7) / 8;

                        comvertbitmap.CopyPixels(buffer, stride, 0);
                        for (int x = 0; x < buffer.Length; x += 4)
                        {
                            b += Convert.ToInt32(buffer[x]);
                            g += Convert.ToInt32(buffer[x + 1]);
                            r += Convert.ToInt32(buffer[x + 2]);
                            a += Convert.ToInt32(buffer[x + 3]);
                            total++;
                        }
                        a /= total;
                        r /= total;
                        g /= total;
                        b /= total;
                        try
                        {

                            SolidColorBrush bkc = (SolidColorBrush?)new BrushConverter().ConvertFromInvariantString("#" + a.ToString("X2") + r.ToString("X2") + g.ToString("X2") + b.ToString("X2"));
                            SolidColorBrush fec = (SolidColorBrush?)new BrushConverter().ConvertFromInvariantString("#" + (a).ToString("X2") + ((byte)~r).ToString("X2") + ((byte)~g).ToString("X2") + ((byte)~b).ToString("X2"));
                            if (bkc.CanFreeze)
                            {
                                bkc.Freeze();
                            }
                            if (fec.CanFreeze)
                            {
                                fec.Freeze();
                            }
                            this.ImageAverageBackgroundColor = (SolidColorBrush)bkc;
                            this.ImageAverageForegroundColor = (SolidColorBrush)fec;
                        }
                        catch
                        {

                        }

                    });
                }
            }
        }
        public Brush ImageAverageBackgroundColor
        {
            get { return _imageAverageBackgroundColor; }
            set
            {
                if (value != null)
                {
                    _imageAverageBackgroundColor = value;
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ImageAverageBackgroundColor"));
                }

            }
        }
        public Brush ImageAverageForegroundColor
        {
            get { return _imageAverageForegroundColor; }
            set
            {
                if (value != null)
                {
                    _imageAverageForegroundColor = value;
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("ImageAverageForegroundColor"));
                }
            }
        }
        public bool AutoChangeColor
        {
            get { return _autochangecolor; }
            set
            {
                _autochangecolor = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("MainBackgroundColor"));
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("MainForegroundColor"));
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
        public bool IsPlayingSendButton
        {
            get { return _isPlayingSendButton; }
            set
            {
                _isPlayingSendButton = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsPlayingSendButton"));
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
        public Brush MainForegroundColor
        {
            get
            {
                if (AutoChangeColor) return _imageAverageForegroundColor;
                return _mainForegroundColor;
            }
            set
            {
                _mainForegroundColor = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("MainForegroundColor"));
            }
        }
        public Brush MainBackgroundColor
        {
            get
            {
                if (AutoChangeColor) return _imageAverageBackgroundColor;
                return _mainBackgroundColor;
            }
            set
            {
                _mainBackgroundColor = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("MainBackgroundColor"));
            }
        }
    }
}
