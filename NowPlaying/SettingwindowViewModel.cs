using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private bool _spotifybuttondisable { get; set; } = true;
        private bool _misskeyButtonDisable { get; set; } = true;
        private bool _IsAlwayTop { get; set; } = false;
        private bool _isautoChangeColor { get; set; } = false;
        private string _inputMisskeyInstanceURL { get; set; } = "";
        private string _misskeyVisibility { get; set; } = "";
        private string _spotifyConnectButton { get; set; } = "Connect";
        private string _backgroundColorText { get; set; } = string.Empty;
        private string _foregroundText { get; set; } = string.Empty;
        private string _settingPostDataText { get; set; } = "Song:${Song}\nArtist:${Artist}\nAlbum:${Album}\n";
        private Brush _backgroundColor { get; set; } = Brushes.White;
        private Brush _foreground { get; set; } = Brushes.Black;
        private Brush _autoChangeBackgroundColor { get; set; } = Brushes.White;
        private Brush _autoChangeForegroundColor { get; set; } = Brushes.Black;
        private Dictionary<int, string> _misskeyVisibilitys { get; } = new Dictionary<int, string>()
        {
            {0,"public"},
            {1,"home" },
            {2, "followers"},
            {3, "specified"},
        };
        private string _MisskeyConnectButton = "Connect";
        private ObservableCollection<UserAccount> _AccountList { get; set; } = new ObservableCollection<UserAccount>();
        private UserAccount _selectedAccount { get; set; } = new UserAccount();
        private int _selectedAccountIndex { get; set; } = 0;
        public SettingwindowViewModel()
        {
            AccountLists = new ObservableCollection<UserAccount>();
            _AccountList.CollectionChanged += AccountList_CollectionChanged;
        }

        private void AccountList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AccountLists"));
        }

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
        public DelegateCommand SettingCheckPostButton
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SettingCheckPostButton"));
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
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsAlwayTop"));
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
        public string SettingPostDataText
        {
            get { return _settingPostDataText; }
            set
            {
                _settingPostDataText = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SettingPostDataText"));

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
        public ObservableCollection<UserAccount> AccountLists
        {
            get
            {
                return _AccountList;
            }
            set
            {
                if (_AccountList != value && value != null)
                {
                    _AccountList = value;
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AccountLists"));
                }
            }

        }
        public void UpdateAccountlist()
        {
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AccountLists"));
        }
        public UserAccount SelectedUser
        {
            get
            {
                return _selectedAccount;
            }
            set
            {
                _selectedAccount = value;

                InputMisskeyInstanceURL = _selectedAccount.userurl;
                MisskeyConnectButton = "Connected";
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SelectedUser"));
            }
        }
        public int SelectedAccountIndex
        {
            get
            {
                return _selectedAccountIndex;
            }
            set
            {
                _selectedAccountIndex = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SettingAccountChanged"));
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SelectedAccountIndex"));
                if (AccountLists.Count <= 0 || AccountLists[value] == null) return;
                InputMisskeyInstanceURL = AccountLists[value].userurl;
            }
        }
        public class UserAccount
        {
            public string username { get; set; } = "";
            public string userId { get; set; } = "";
            public string usertoken { get; set; } = "";
            public string userurl { get; set; } = "";
            public string usernameinstance
            {
                get
                {
                    return "@"+username + "@" + uri.Host;
                }
            }
            private Uri uri { get { return new Uri(userurl); } }
        }
    }
}
