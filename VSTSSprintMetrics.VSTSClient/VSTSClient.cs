using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace VSTSSprintMetrics.VSTSClient
{
    internal class VSTSClient
    {
        private readonly VSTSApiSettings mOptions;
        private readonly HttpClient mClient;

        public VSTSClient(VSTSApiSettings options, HttpClient client)
        {
            mOptions = options;
            mClient = client;
        } 

        public async Task<T> ExecuteActionAsync<T>(string url)
        {
            
            var response = await mClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }
    }
}
