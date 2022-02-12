using ConsoleApp1.Models;
using System.Threading.Tasks;

namespace ConsoleApp1.Services.Interfaces
{
    public interface IFacebookService
    {
        string GetLoginUrl(string state = null);
        Task<string> ListenForCallback();
        Task<FacebookUser> GetUser(string userId, string accessToken);
        Task<FacebookToken> GetAccessToken(string code);
        Task<FacebookDebugToken> GetTokenInfo(string token);
    }
}
