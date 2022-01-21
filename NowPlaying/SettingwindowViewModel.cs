using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Commands;
using Prism.Mvvm;
namespace NowPlaying
{
    internal class SettingwindowViewModel : BindableBase
    {
        private string _inputMisskeyInstanceURL = "";
        private bool _spotifybuttondisable = true;
        private bool _misskeyButtonDisable = true;
        private string _spotifyConnectButton = "Connect";
        private string _misskeyVisibility = "";
        private bool _IsAlwayTop = false;
        private Brush _backgroundColor = Brushes.White;
        private Brush _foreground = Brushes.Black;
        private string _backgroundColorText = string.Empty;
        private string _foregroundText = string.Empty;
        private bool _isautoChangeColor = false;
        private Brush _autoChangeBackgroundColor = Brushes.White;
        private Brush _autoChangeForegroundColor = Brushes.Black;
        private Dictionary<int, string> _misskeyVisibilitys = new Dictionary<int, string>()
        {
            {0,"public"},
            {1,"home" },
            {2, "followers"},
            {3, "specified"},
        };
        private string _MisskeyConnectButton = "Connect";
        public string MisskeyConnectButton
        {
            get { return _MisskeyConnectButton; }
            set
            {
                _MisskeyConnectButton = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("MisskeyConnectButton"));
            }
        }
        public bool MisskeyButtonDisable
        {
            get { return _misskeyButtonDisable; }
            set
            {
                _misskeyButtonDisable = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("MisskeyButtonDisable"));
            }
        }
        public Dictionary<int, string> MisskeyVisibilitys
        {
            get { return _misskeyVisibilitys; }
        }
        public string InputMisskeyInstanceURL
        {
            get { return _inputMisskeyInstanceURL; }
            set
            {
                if (_inputMisskeyInstanceURL != value)
                {
                    MisskeyButtonDisable = true;
                    _inputMisskeyInstanceURL = value;
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("InputMisskeyInstanceURL"));
                }
            }
        }
        public string MisskeyVisibility
        {
            get { return _misskeyVisibility; }
            set
            {
                if (_misskeyVisibility != value)
                {
                    _misskeyVisibility = value;
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(MisskeyVisibility)));
                }
            }
        }
        public bool Spotifybuttondisable
        {
            get { return _spotifybuttondisable; }
            set
            {
                _spotifybuttondisable = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(Spotifybuttondisable)));
            }
        }
        public string SpotifyConnectButton
        {
            get { return _spotifyConnectButton; }
            set
            {
                _spotifyConnectButton = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(SpotifyConnectButton)));
            }
        }
        public DelegateCommand SpotifyAuth
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SpotifyAuth"));
                });
            }
        }
        public DelegateCommand MisskeyAuth
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("MisskeyAuth"));
                });
            }
        }

        public bool IsAlwayTop
        {
            get { return _IsAlwayTop; }
            set
            {
                _IsAlwayTop = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SettingIsAlwayTop"));
            }
        }

        public Brush SettingBackgroundColor
        {
            get
            {
                if (IsAutoChangeColor) return _autoChangeBackgroundColor;
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SettingBackgroundColor"));
            }
        }
        public Brush SettingForegroundColor
        {
            get
            {
                if (IsAutoChangeColor) return _autoChangeForegroundColor;
                return _foreground;
            }
            set
            {
                _foreground = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SettingForegroundColor"));
            }
        }

        public string SettingBackgroundColorText
        {
            get { return _backgroundColorText; }
            set
            {
                if (_backgroundColorText == value) return;
                _backgroundColorText = value;
                try
                {
                    var color = (SolidColorBrush?)new BrushConverter().ConvertFromString(_backgroundColorText);
                    if (color != null)
                    {
                        SettingBackgroundColor = color;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        public string SettingForegroundColorText
        {
            get { return _foregroundText; }
            set
            {
                if (_foregroundText == value) return;
                _foregroundText = value;
                try
                {
                    var color = (SolidColorBrush?)new BrushConverter().ConvertFromString(_foregroundText);
                    if (color != null)
                    {
                        SettingForegroundColor = color;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        public bool IsAutoChangeColor
        {
            get { return _isautoChangeColor; }
            set
            {
                _isautoChangeColor = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsAutoChangeColor"));
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsColorSettingText"));
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SettingBackgroundColor"));
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SettingForegroundColor"));
            }
        }
        public Brush AutoChangeBackgroundColor
        {
            get { return _autoChangeBackgroundColor; }
            set
            {
                _autoChangeBackgroundColor = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SettingBackgroundColor"));
            }
        }
        public Brush AutoChangeForegroundColor
        {
            get { return _autoChangeForegroundColor; }
            set
            {
                _autoChangeForegroundColor = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SettingForegroundColor"));
            }
        }
        public bool IsColorSettingText
        {
            get
            {
                return !IsAutoChangeColor;
            }
        }
    }
}
