using ConsoleApp1.Configuration;
using ConsoleApp1.Models;
using ConsoleApp1.Services;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class FacebookServiceTests
    {
        private AppSettings AppSettings = new AppSettings
        {
            FacebookClientId = "123",
            FacebookCallbackPort = "8080",
            FacebookCallbackEndpoint = "unittest",
            FacebookAppSecret = "123456",
            FacebookPermissionsScope = new string[] { "public_profile" },
            FacebookUserFields = new string[] { "first_name" }
        };

        private string RedirectUrl
        {
            get { return $"http://localhost:{ AppSettings.FacebookCallbackPort }/{ AppSettings.FacebookCallbackEndpoint }"; }
        }

        [Fact]
        public void ShouldGetLoginUrl()
        {
            var facebookService = new FacebookService(new HttpClient(), AppSettings);
            Assert.Equal(
                $"https://www.facebook.com/v13.0/dialog/oauth?client_id={ AppSettings.FacebookClientId }&redirect_uri={ this.RedirectUrl }&state=&scope=public_profile", facebookService.GetLoginUrl());
        }

        [Fact]
        public async Task ShouldStartListenerAndGetCodeAsync()
        {
            var httpClient = new HttpClient();
            var myCode = "123456789";
            var facebookService = new FacebookService(httpClient, AppSettings);
            string result = null;

            _ = Task.Run(async () => result = await facebookService.ListenForCallback().ConfigureAwait(false));

            await httpClient.GetAsync($"http://localhost:{AppSettings.FacebookCallbackPort}/{ AppSettings.FacebookCallbackEndpoint }?code={myCode}");
            Assert.Equal(myCode, result);
        }

        [Fact]
        public async Task ShouldReturnAnAccessToken()
        {
            var handler = new Mock<HttpMessageHandler>();
            var code = "mycode123";
            var expected = new FacebookToken
            {
                AccessToken = "123",
                TokenType = "Bearer",
                ExpiresIn = 1238485
            };
            var content = new StringContent(@$"{{ ""access_token"": ""{ expected.AccessToken }"", ""token_type"": ""{ expected.TokenType }"", ""expires_in"": ""{ expected.ExpiresIn }"" }}", Encoding.UTF8, "application/json");
            var httpResponseMessage = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = content };

            string requestUrl = "https://graph.facebook.com/v13.0/oauth/access_token?" +
                $"client_id={ AppSettings.FacebookClientId }" +
                $"&redirect_uri={ this.RedirectUrl }" +
                $"&client_secret={ AppSettings.FacebookAppSecret }" +
                $"&code={ code }";

            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString() == requestUrl), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://graph.facebook.com") };
            var facebookService = new FacebookService(httpClient, AppSettings);
            var result = await facebookService.GetAccessToken(code);

            var resultJson = JsonConvert.SerializeObject(result);
            var expectedResultJson = JsonConvert.SerializeObject(expected);
            Assert.Equal(expectedResultJson, resultJson);
        }

        [Fact]
        public async Task ShouldGetTokenInfo()
        {
            var handler = new Mock<HttpMessageHandler>();
            var token = "mytoken123";
            var expected = new FacebookDebugToken
            {
                Data = new FacebookDebugTokenData
                {
                    ApplicationId = AppSettings.FacebookClientId,
                    Application = "MyApp",
                    ExpiresAt = "132123",
                    UserId = "123"
                }
            };
            var content = new StringContent($@"{{ ""data"": {{ ""app_id"": ""{ expected.Data.ApplicationId }"", ""application"": ""{ expected.Data.Application }"", ""expires_at"": ""{ expected.Data.ExpiresAt }"", ""user_id"": ""{ expected.Data.UserId }"" }} }}", Encoding.UTF8, "application/json");
            var httpResponseMessage = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = content };

            string requestUrl = "https://graph.facebook.com/debug_token?" +
                $"input_token={ token }" +
                $"&access_token={ AppSettings.FacebookClientId }|{ AppSettings.FacebookAppSecret }";

            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString() == requestUrl), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://graph.facebook.com") };
            var facebookService = new FacebookService(httpClient, AppSettings);
            var result = await facebookService.GetTokenInfo(token);

            var resultJson = JsonConvert.SerializeObject(result);
            var expectedResultJson = JsonConvert.SerializeObject(expected);
            Assert.Equal(expectedResultJson, resultJson);
        }

        [Fact]
        public async Task ShouldGetUserInfo()
        {
            var handler = new Mock<HttpMessageHandler>();
            var accessToken = "mytoken123";
            var expected = new FacebookUser
            {
                Id = "123",
                Name = "John Smith",
                FirstName = "John",
                LastName = "Smith",
                Email = "jsmith@gmail.com"
            };
            var content = new StringContent(@$"{{ ""id"": ""{ expected.Id }"", ""name"": ""{ expected.Name }"", ""first_name"": ""{ expected.FirstName }"", ""last_name"": ""{ expected.LastName }"", ""email"": ""{ expected.Email }"" }}", Encoding.UTF8, "application/json");
            var httpResponseMessage = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = content };

            string requestUrl = $"https://graph.facebook.com/v13.0/{ expected.Id }?" +
                $"access_token={ accessToken }" +
                $"&fields={ string.Join(',', AppSettings.FacebookUserFields)}";

            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString() == requestUrl), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://graph.facebook.com") };
            var facebookService = new FacebookService(httpClient, AppSettings);
            var result = await facebookService.GetUser(expected.Id, accessToken);

            var resultJson = JsonConvert.SerializeObject(result);
            var expectedResultJson = JsonConvert.SerializeObject(expected);
            Assert.Equal(expectedResultJson, resultJson);
        }
    }
}