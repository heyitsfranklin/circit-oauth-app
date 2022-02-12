namespace ConsoleApp1.Configuration
{
    public class AppSettings
    {
        public string FacebookCallbackPort { get; set; }
        public string FacebookCallbackEndpoint { get; set; }
        public string FacebookClientId { get; set; }
        public string FacebookAppSecret { get; set; }
        public string[] FacebookPermissionsScope { get; set; }
        public string[] FacebookUserFields { get; set; }
    }
}
