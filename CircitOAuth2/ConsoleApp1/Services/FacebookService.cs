using ConsoleApp1.Configuration;
using ConsoleApp1.Models;
using ConsoleApp1.Services.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleApp1.Services
{
    /// <summary>
    /// Handles Facebook authentication and user data
    /// </summary>
    public class FacebookService : IFacebookService
    {
        private readonly HttpClient HttpClient;
        private readonly AppSettings AppSettings;

        private string RedirectUrl { 
            get { return $"http://localhost:{ AppSettings.FacebookCallbackPort }/{ AppSettings.FacebookCallbackEndpoint }"; }
        }

        public FacebookService(HttpClient httpclient, AppSettings appSettings)
        {
            HttpClient = httpclient;
            AppSettings = appSettings;
        }

        /// <summary>
        /// Gets a Facebook login URL based on application settings
        /// </summary>
        /// <param name="state">(Optional) A string value to validate responses from Facebook</param>
        /// <returns>A Facebook login URL</returns>
        public string GetLoginUrl(string state = null)
        {
            return string.Format(
                "https://www.facebook.com/v13.0/dialog/oauth?client_id={0}&redirect_uri={1}&state={2}&scope={3}",
                AppSettings.FacebookClientId,
                this.RedirectUrl,
                state,
                string.Join(',', AppSettings.FacebookPermissionsScope));
        }

        /// <summary>
        /// Initiates an HTTP listener to listen for login responses from Facebook
        /// </summary>
        /// <returns>A Facebook code to exchange for a user login token</returns>
        public async Task<string> ListenForCallback()
        {
            string code = null;
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add($"http://*:{AppSettings.FacebookCallbackPort}/{ AppSettings.FacebookCallbackEndpoint }/");
                listener.Start();
                HttpListenerContext context = await listener.GetContextAsync().ConfigureAwait(false);
                code = context.Request.QueryString["code"];

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.Close();
                listener.Stop();
            }
            return code;
        }

        /// <summary>
        /// Gets a user access token
        /// </summary>
        /// <param name="code">The code value from a login response</param>
        /// <returns>A FacebookToken response</returns>
        public async Task<FacebookToken> GetAccessToken(string code)
        {
            var response = await HttpClient.GetAsync($"/v13.0/oauth/access_token?" +
                $"client_id={ AppSettings.FacebookClientId }" +
                $"&redirect_uri={ this.RedirectUrl }" +
                $"&client_secret={ AppSettings.FacebookAppSecret }" +
                $"&code={ code }")
                .ConfigureAwait(false);

            return JsonConvert.DeserializeObject<FacebookToken>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Gets context info pertaining to an access token
        /// </summary>
        /// <param name="token">The access token</param>
        /// <returns>A FacebookDebugToken response</returns>
        public async Task<FacebookDebugToken> GetTokenInfo(string token)
        {
            var response = await HttpClient.GetAsync($"/debug_token?" +
                $"input_token={ token }" +
                $"&access_token={ AppSettings.FacebookClientId }|{ AppSettings.FacebookAppSecret }");

            return JsonConvert.DeserializeObject<FacebookDebugToken>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Gets user data
        /// </summary>
        /// <param name="userId">The user's Facebook id</param>
        /// <param name="accessToken">The user's access token</param>
        /// <returns>A FacebookUser response</returns>
        public async Task<FacebookUser> GetUser(string userId, string accessToken)
        {
            var response = await HttpClient.GetAsync($"/v13.0/{ userId }?access_token={ accessToken }&fields={ string.Join(',', AppSettings.FacebookUserFields)}").ConfigureAwait(false);
            return JsonConvert.DeserializeObject<FacebookUser>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
    }
}
