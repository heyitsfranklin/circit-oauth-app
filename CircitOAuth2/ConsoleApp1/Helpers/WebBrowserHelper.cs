using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ConsoleApp1.Helpers
{
    /// <summary>
    /// Helper class for launching URLs in web browsers across different operating systems
    /// </summary>
    /// <remarks>Source code obtained from https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/</remarks>
    public static class WebBrowserHelper
    {
        /// <summary>
        /// Opens a URL in a web browser
        /// </summary>
        /// <param name="url">The URL to launch</param>
        public static void LaunchUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
