using System.Management;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;

namespace HopiBot.LCU
{
    public class LcuManager
    {
        public static string Port;
        public static string Token;
        public static RestClient Client;

        public static void Load()
        {
            var query = new SelectQuery("SELECT CommandLine FROM Win32_Process WHERE Name = 'LeagueClientUx.exe'");
            using (var searcher = new ManagementObjectSearcher(query))
            {
                foreach (var obj in searcher.Get())
                {
                    var commandLine = (string)obj["CommandLine"];
                    var arguments = commandLine.Split(' ');

                    foreach (var argument in arguments)
                    {
                        if (argument.Contains("--remoting-auth-token="))
                        {
                            Token = argument.Split('=')[1].Replace("\"", "");
                        }
                        else if (argument.Contains("--app-port="))
                        {
                            Port = argument.Split('=')[1].Replace("\"", "");
                        }
                    }
                }
            }

            var options = new RestClientOptions($"https://127.0.0.1:{LcuManager.Port}")
            {
                Authenticator = new HttpBasicAuthenticator("riot", LcuManager.Token),
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            Client = new RestClient(options);
        }

        public static async Task<RestResponse> GetAsync(string path)
        {
            var request = new RestRequest(path);
            var response = await Client.GetAsync(request);
            return response;
        }

        public static async Task<RestResponse> PostAsync(string path, object body = null)
        {
            var request = new RestRequest(path);
            if (body != null) request.AddBody(body);
            var response = await Client.PostAsync(request);
            return response;
        }

        public static async Task<RestResponse> PatchAsync(string path, object body = null)
        {
            var request = new RestRequest(path);
            if (body != null) request.AddBody(body);
            var response = await Client.PatchAsync(request);
            return response;
        }
    }
}
