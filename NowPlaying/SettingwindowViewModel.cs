using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
namespace NowPlaying
{
    internal class SettingwindowViewModel:BindableBase
    {
        private string _inputMisskeyInstanceURL = "";
        private bool _spotifybuttondisable = true;
        private string _inputSpotifyToken = "";
        private string _misskeyVisibility = "";
        private Dictionary<int,string> _misskeyVisibilitys = new Dictionary<int, string>()
        {
            {0,"public"},
            {1,"home" },
            {2, "followers"},
            {3, "specified"},
        };
        public Dictionary<int,string> MisskeyVisibilitys
        {
            get { return _misskeyVisibilitys; }
        }
        public string InputMisskeyI
        {
            get { return _inputMisskeyInstanceURL; }
            set
            {
                _inputMisskeyInstanceURL = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(InputMisskeyI)));
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
        public string InputSpotifyToken
        {
            get { return _inputSpotifyToken; }
            set
            {
                _inputSpotifyToken = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(InputSpotifyToken)));
            }
        }
        public DelegateCommand SpotifyConnectButton
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SpotifyConnect"));
                });
            }
        }
    }
}
