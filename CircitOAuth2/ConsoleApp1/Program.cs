using ConsoleApp1.Configuration;
using ConsoleApp1.Helpers;
using ConsoleApp1.Services;
using ConsoleApp1.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        private static AppSettings AppSettings;
        private static IFacebookService FacebookLoginService;

        static async Task Main(string[] args)
        {
            Init();            

            Console.WriteLine("Press enter to login to Facebook");
            Console.ReadLine();

            Console.WriteLine("Waiting for login...");

            string code = null;
            for (int i =0; i < 3 && string.IsNullOrEmpty(code); i++)
            {
                WebBrowserHelper.LaunchUrl(FacebookLoginService.GetLoginUrl());
                code = await FacebookLoginService.ListenForCallback().ConfigureAwait(false);
                if (code == null && i != 2)
                    Console.WriteLine($"It appears an error occurred during login or the login was cancelled. Please try again ({ 3 - (i+1)} attempt(s) remaining).");
            }

            if (code == null)
            {
                Console.WriteLine("Login unsuccessful. Press enter to exit now.");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("√ Login successful" + Environment.NewLine);

            Console.WriteLine("Getting access token...");
            var token = await FacebookLoginService.GetAccessToken(code).ConfigureAwait(false);
            Console.WriteLine("√ Received access token" + Environment.NewLine);

            Console.WriteLine("Validating access token...");
            var tokenInfo = await FacebookLoginService.GetTokenInfo(token.AccessToken).ConfigureAwait(false);
            if (!tokenInfo.Data.ApplicationId.Equals(AppSettings.FacebookClientId))
            {
                Console.WriteLine("Token invalid. Exiting now.");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("√ Access token validated successfully" + Environment.NewLine);

            Console.WriteLine("Fetching user info...");
            var userInfo = await FacebookLoginService.GetUser(tokenInfo.Data.UserId, token.AccessToken).ConfigureAwait(false);
            Console.WriteLine($"User Id: { userInfo.Id }");
            Console.WriteLine($"First name: { userInfo.FirstName }");
            Console.WriteLine($"Last name: { userInfo.LastName }");
            if (!string.IsNullOrEmpty(userInfo.MiddleName))
            {
                Console.WriteLine($"Middle name: { userInfo.MiddleName }");
            }
            Console.WriteLine($"Email: { userInfo.Email }");

            Console.WriteLine(Environment.NewLine + "Press enter to exit...");
            Console.ReadLine();
        }
        
        private static void Init()
        {
            // initialize config
            var serviceCollection = new ServiceCollection();
            var config = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", false)
                            .Build();
            AppSettings = config.Get<AppSettings>();

            // configure services
            serviceCollection.AddTransient<IFacebookService, FacebookService>();
            serviceCollection.AddSingleton(AppSettings);

            // configure service http clients
            serviceCollection
                .AddHttpClient<IFacebookService, FacebookService>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri("https://graph.facebook.com");
                });

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            FacebookLoginService = serviceProvider.GetService<IFacebookService>();
        }
    }
}
