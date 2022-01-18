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
        private static SpotifyClient spotifyClient;
        private static CurrentlyPlaying Playing;
        public static bool IsGetToken = false;
        private string refreshtoken;
        private string _clientID;
        public System.Timers.Timer refreshtimer = new System.Timers.Timer();

        public string ClientID
        {
            get { return _clientID; }
            set { _clientID = value; }
        }
        public string RefreshToken
        {
            get { return refreshtoken; }
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
            catch
            {
                await _server3.Stop();
                return false;
            }
            return true;
        }
        public async Task GetToken2()
        {
            _server2 = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
            await _server2.Start();
            _server2.AuthorizationCodeReceived += _server2_AuthorizationCodeReceived;

            (string verifier, string challenge) = PKCEUtil.GenerateCodes("o5nUHWpSJ3gMP5V3wWMqWwAWo6ikAN5QKi2gkutL5vZyKKkezw6gFGH5KYfc9M5j33mFCCZytMcf4dVh");
            // Returns the passed string and its challenge (Make sure it's random and long enough)
            var loginRequest = new LoginRequest(
              new Uri("http://localhost:5000/callback"),
              ClientID,
              LoginRequest.ResponseType.Code
            )
            {
                CodeChallengeMethod = "S256",
                CodeChallenge = challenge,
                Scope = new[] { Scopes.PlaylistReadPrivate, Scopes.PlaylistReadCollaborative, Scopes.UserModifyPlaybackState, Scopes.UserReadRecentlyPlayed, Scopes.UserReadCurrentlyPlaying }
            };
            var uri = loginRequest.ToUri();
            BrowserUtil.Open(uri);
        }

        private async Task _server2_AuthorizationCodeReceived(object arg1, AuthorizationCodeResponse arg2)
        {
            await _server2.Stop();
            await GetCallback(arg2.Code);
        }

        // This method should be called from your web-server when the user visits "http://localhost:5000/callback"
        public async Task GetCallback(string code)
        {
            _server3 = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
            await _server3.Start();

            (string verifier, string challenge) = PKCEUtil.GenerateCodes("o5nUHWpSJ3gMP5V3wWMqWwAWo6ikAN5QKi2gkutL5vZyKKkezw6gFGH5KYfc9M5j33mFCCZytMcf4dVh");
            // Note that we use the verifier calculated above!

            var initialResponse = await new OAuthClient().RequestToken(
              new PKCETokenRequest(ClientID, code, new Uri("http://localhost:5000/callback"), verifier)
            );

            spotifyClient = new SpotifyClient(initialResponse.AccessToken);
            Playing = await spotifyClient.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track));

            // Also important for later: response.RefreshToken
            IsGetToken = true;
            refreshtoken = initialResponse.RefreshToken;
            await _server3.Stop();
        }
        public async Task RefreshTokenFunc()
        {
            _server3 = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
            await _server3.Start();
            var newResponse = await new OAuthClient().RequestToken(
              new PKCETokenRefreshRequest(ClientID, refreshtoken)
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
                await RefreshTokenFunc();
                Playing = await spotifyClient.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track));
            }
            return Playing;
        }

        public async Task NextSongs()
        {
            if (spotifyClient == null) return;
            await spotifyClient.Player.SkipNext();
        }
        public async Task PreviousSongs()
        {
            if (spotifyClient == null) return;
            await spotifyClient.Player.SkipPrevious();
        }
        public async Task PlayResume()
        {
            if (spotifyClient == null && Playing == null) return;
            //Playing = await spotifyClient.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track));
            if (Playing.IsPlaying)
            {
                await spotifyClient.Player.PausePlayback();
            }
            else
            {
                await spotifyClient.Player.ResumePlayback();
            }
        }
        public void Dispose()
        {
            refreshtimer.Stop();
            refreshtimer.Dispose();

        }
    }
}
