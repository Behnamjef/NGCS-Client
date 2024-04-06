using NetworkAdapter.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace NetworkAdapter.Brokers
{
    public  class NetworkHttpBrocker
    {
        public readonly string apiBasicUri;
        public UserInfo userInfo { private set; get; } // this will fill on auth service
        public event Action OnUnauthorizedCall;

        public NetworkHttpBrocker(string apiBasicUri)
        {
            this.apiBasicUri = apiBasicUri;
        }

        public void SetUserInfoExternaly(UserInfo userInfo)
        {
            this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
        }

        public async Task<TResult> Post<TPara, TResult>(string url, TPara contentValue)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiBasicUri);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userInfo?.token);

                var content = new StringContent(JsonConvert.SerializeObject(contentValue), Encoding.UTF8, "application/json");
                var result = await client.PostAsync(url, content);
                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    OnUnauthorizedCall?.Invoke();
                    throw new UnauthorizedAccessException($"_URL: {url}");
                }

                var data = await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TResult>(data);
            }
        }

        public async Task<T> Get<T>(string url)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiBasicUri);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userInfo?.token);

                var result = await client.GetAsync(url);

                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    OnUnauthorizedCall?.Invoke();
                    throw new UnauthorizedAccessException($"_URL: {url}");
                }

                result.EnsureSuccessStatusCode();
                string resultContentString = await result.Content.ReadAsStringAsync();
                T resultContent = JsonConvert.DeserializeObject<T>(resultContentString);
                return resultContent;
            }
        }

    }
}
