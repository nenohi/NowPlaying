using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NowPlaying
{
    internal class Spotify
    {
        private static EmbedIOAuthServer _server2;
        private static EmbedIOAuthServer _server3;
        private static SpotifyClient? spotifyClient;
        private static CurrentlyPlaying? Playing;
        public static bool IsGetToken = false;
        private string? refreshtoken;
        private string? _clientID;
        public System.Timers.Timer refreshtimer = new System.Timers.Timer();
        private int _shuffle_status = 0;
        private Device _Device = new Device();
        public int ShuffleStatus
        {
            get
            {
                return _shuffle_status;
            }
        }
        public string ClientID
        {
            get
            {
                if (_clientID == null) return string.Empty;
                return _clientID;
            }
            set { _clientID = value; }
        }
        public string RefreshToken
        {
            get
            {
                if (refreshtoken == null) return string.Empty;
                return refreshtoken;
            }
        }
        public Spotify()
        {
            refreshtimer.Interval = 5000;
            refreshtimer.AutoReset = true;
            refreshtimer.Enabled = true;
            refreshtimer.Elapsed += Refreshtimer_Elapsed;
        }

        private void Refreshtimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {

        }
        public async Task<bool> SetToken(string reftoken)
        {
            try
            {
                _server3 = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
                await _server3.Start();
                var newResponse = await new OAuthClient().RequestToken(
                  new PKCETokenRefreshRequest(ClientID, reftoken)
                );

                spotifyClient = new SpotifyClient(newResponse.AccessToken);
                Playing = await spotifyClient.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track));
                refreshtoken = newResponse.RefreshToken;
                IsGetToken = true;
                await _server3.Stop();
            }
            catch (Exception ex)
            {
                NLogService.logger.Error(ex, "SpotifySetToken Error");
                await _server3.Stop();
                return false;
            }
            return true;
        }
        public async Task GetToken()
        {
            _server2 = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
            await _server2.Start();
            _server2.AuthorizationCodeReceived += _server2_AuthorizationCodeReceived;

            (string verifier, string challenge) = PKCEUtil.GenerateCodes("o5nUHWpSJ3gMP5V3wWMqWwAWo6ikAN5QKi2gkutL5vZyKKkezw6gFGH5KYfc9M5j33mFCCZytMcf4dVh");
            var loginRequest = new LoginRequest(
              new Uri("http://localhost:5000/callback"),
              ClientID,
              LoginRequest.ResponseType.Code
            )
            {
                CodeChallengeMethod = "S256",
                CodeChallenge = challenge,
                Scope = new[] { Scopes.PlaylistReadPrivate, Scopes.PlaylistReadCollaborative, Scopes.UserModifyPlaybackState, Scopes.UserReadRecentlyPlayed, Scopes.UserReadCurrentlyPlaying, Scopes.UserReadPlaybackState }
            };
            var uri = loginRequest.ToUri();
            BrowserUtil.Open(uri);
        }

        private async Task _server2_AuthorizationCodeReceived(object arg1, AuthorizationCodeResponse arg2)
        {
            await _server2.Stop();
            await GetCallback(arg2.Code);
        }
        public async Task GetCallback(string code)
        {
            _server3 = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
            await _server3.Start();
            (string verifier, string challenge) = PKCEUtil.GenerateCodes("o5nUHWpSJ3gMP5V3wWMqWwAWo6ikAN5QKi2gkutL5vZyKKkezw6gFGH5KYfc9M5j33mFCCZytMcf4dVh");
            var initialResponse = await new OAuthClient().RequestToken(
              new PKCETokenRequest(ClientID, code, new Uri("http://localhost:5000/callback"), verifier)
            );
            spotifyClient = new SpotifyClient(initialResponse.AccessToken);
            Playing = await spotifyClient.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track));
            IsGetToken = true;
            refreshtoken = initialResponse.RefreshToken;
            await _server3.Stop();
        }
        public async Task RefreshTokenFunc()
        {
            _server3 = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
            await _server3.Start();
            var newResponse = await new OAuthClient().RequestToken(
              new PKCETokenRefreshRequest(ClientID, RefreshToken)
            );
            spotifyClient = new SpotifyClient(newResponse.AccessToken);
            refreshtoken = newResponse.RefreshToken;
            await _server3.Stop();

        }
        public async Task<CurrentlyPlaying> GetCurrentlyPlaying()
        {
            if (spotifyClient == null) return Playing;
            try
            {
                Playing = await spotifyClient.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track));
            }
            catch (APIUnauthorizedException e)
            {
                NLogService.logger.Info(e, "SpotifyTokenRefresh by GetCurrentPlaying");
                await RefreshTokenFunc();
                Playing = await spotifyClient.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track));
            }
            catch (APIException e)
            {
                NLogService.logger.Error(e, "SpotifyAPIError by GetCurrentPlaying");
            }
            catch (Exception e)
            {
                NLogService.logger.Error(e, "Unknown Error by GetCurrentPlaying");
            }
            return Playing;
        }

        public async Task NextSongs()
        {
            if (spotifyClient == null) return;
            try
            {
                await spotifyClient.Player.SkipNext();
            }
            catch (APIException e)
            {
                NLogService.logger.Error(e, "SpotifyAPIError by NextSongs");
            }
            catch (Exception e)
            {
                NLogService.logger.Error(e, "Unknown Error by NextSongs");
            }
        }
        public async Task PreviousSongs()
        {
            if (spotifyClient == null) return;
            try
            {
                await spotifyClient.Player.SkipPrevious();
            }
            catch (APIException e)
            {
                NLogService.logger.Error(e, "SpotifyAPIError by PreviousSongs");
            }
            catch (Exception e)
            {
                NLogService.logger.Error(e, "Unknown Error by PreviousSongs");
            }
        }
        public async Task PlayResume()
        {
            if (spotifyClient == null || Playing == null) return;
            try
            {
                Playing = await spotifyClient.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track));
            }
            catch (APIUnauthorizedException e)
            {
                NLogService.logger.Error(e, "SpotifyToken Error");
                await RefreshTokenFunc();
                Playing = await spotifyClient.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track));
            }
            try
            {
                if (Playing.IsPlaying)
                {
                    DeviceResponse devices = await spotifyClient.Player.GetAvailableDevices();
                    IEnumerable<Device> _d = devices.Devices.Where(d => d.IsActive == true);
                    _Device = (Device) (_d.FirstOrDefault() ?? new Device());
                    await spotifyClient.Player.PausePlayback();
                    Playing.IsPlaying = false;
                }
                else
                {
                    if (_Device.Id == null)
                    {
                        await spotifyClient.Player.ResumePlayback();
                    }
                    else
                    {
                        await spotifyClient.Player.ResumePlayback(new PlayerResumePlaybackRequest() { DeviceId = _Device.Id, });
                    }
                    Playing.IsPlaying = true;
                }
            }
            catch (APIException e)
            {
                NLogService.logger.Error(e, "Pause Resume Error");
                _Device = new Device();
            }
        }
        public async Task SetRepeat()
        {
            if (spotifyClient == null || Playing == null) return;
            FullTrack fullTrack = (FullTrack)Playing.Item;
            _shuffle_status -= 1;
            if (_shuffle_status < 0)
            {
                _shuffle_status = 2;
            }
            await spotifyClient.Player.SetRepeat(new PlayerSetRepeatRequest((PlayerSetRepeatRequest.State)ShuffleStatus));
        }
        public void Dispose()
        {
            refreshtimer.Stop();
            refreshtimer.Dispose();

        }
    }
}
