using System.Management;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;

namespace HopiBot.LCU
{
    public class LcuManager
    {
        public static LcuManager Instance = new LcuManager();

        private string _port;
        private string _token;
        private RestClient _client;
        private RestClient _gameClient;

        private LcuManager()
        {
            Load();
        }

        private void Load()
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
                            _token = argument.Split('=')[1].Replace("\"", "");
                        }
                        else if (argument.Contains("--app-port="))
                        {
                            _port = argument.Split('=')[1].Replace("\"", "");
                        }
                    }
                }
            }

            var options = new RestClientOptions($"https://127.0.0.1:{_port}")
            {
                Authenticator = new HttpBasicAuthenticator("riot", _token),
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            _client = new RestClient(options);

            options = new RestClientOptions($"https://127.0.0.1:2999")
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            _gameClient = new RestClient(options);
        }

        public RestResponse GetClient(string path)
        {
            var request = new RestRequest(path);
            var response = _client.Get(request);
            return response;
        }

        public RestResponse PostClient(string path, object body = null)
        {
            var request = new RestRequest(path);
            if (body != null) request.AddBody(body);
            var response = _client.Post(request);
            return response;
        }

        public RestResponse PatchClient(string path, object body = null)
        {
            var request = new RestRequest(path);
            if (body != null) request.AddBody(body);
            var response = _client.Patch(request);
            return response;
        }

        public RestResponse GetGameClient(string path, int timeout = 0)
        {
            var request = new RestRequest(path);
            if (timeout > 0) request.Timeout = timeout;
            var response = _gameClient.Get(request);
            return response;
        }

        public RestResponse PostGameClient(string path, object body = null, int timeout = 0)
        {
            var request = new RestRequest(path);
            if (body != null) request.AddBody(body);
            if (timeout > 0) request.Timeout = timeout;
            var response = _gameClient.Post(request);
            return response;
        }

        public RestResponse PatchGameClient(string path, object body = null, int timeout = 0)
        {
            var request = new RestRequest(path);
            if (body != null) request.AddBody(body);
            if (timeout > 0) request.Timeout = timeout;
            var response = _gameClient.Patch(request);
            return response;
        }
    }
}
