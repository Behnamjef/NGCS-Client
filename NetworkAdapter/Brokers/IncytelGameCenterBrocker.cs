using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NetworkAdapter.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetworkAdapter.Brokers
{
    public class IncytelGameCenterBrocker
    {
        HttpClient httpClient = new HttpClient();
        string BaseUrl = "";

        public IncytelGameCenterBrocker(string BaseUrl)
        {
            this.BaseUrl = BaseUrl;
        }

        public string GetApiUrl(string endpoint) => BaseUrl.TrimEnd('/') + endpoint;

        public Task<T> CallApiGet<T>(string endpoint, string language) => CallApi<T>(HttpMethod.Get, endpoint, language);

        public Task<T> CallApiDelete<T>(string endpoint, string language) =>
            CallApi<T>(HttpMethod.Delete, endpoint, language);

        public Task<T> CallApiPost<T>(string endpoint, object body, string language) =>
            CallApi<T>(HttpMethod.Post, endpoint, language, body);

        public Task<T> CallApiPut<T>(string endpoint, object body, string language) =>
            CallApi<T>(HttpMethod.Put, endpoint, language, body);

        public async Task<string> CallVerfyPhoneApi(HttpMethod method, string endpoint, string language,
            object body = null)
        {
            var url = GetApiUrl(endpoint);
            string bodyStr = null;
            if (body != null)
                bodyStr = JsonConvert.SerializeObject(body);

            HttpResponseMessage result;

            using (var requestMessage = new HttpRequestMessage(method, url))
            {
                requestMessage.Headers.Add("lang", language);
                if (body != null)
                {
                    requestMessage.Content = new StringContent(bodyStr, Encoding.UTF8, "application/json");
                }

                result = await httpClient.SendAsync(requestMessage);
            }

            var resp = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode)
            {
                return resp;
            }

            try
            {
                var j = JObject.Parse(resp);
                throw new ApiException((int)result.StatusCode, (string)j["detail"], (string)j["message"]);
            }
            catch (Exception e)
            {
                throw new ApiException((int)result.StatusCode, resp, e.Message);
            }
        }

        public async Task<T> CallApi<T>(HttpMethod method, string endpoint, string language, object body = null)
        {
            var url = GetApiUrl(endpoint);
            string bodyStr = null;
            if (body != null)
                bodyStr = JsonConvert.SerializeObject(body);


            HttpResponseMessage result;

            using (var requestMessage = new HttpRequestMessage(method, url))
            {
                requestMessage.Headers.Add("lang", language);
                if (body != null)
                {
                    requestMessage.Content = new StringContent(bodyStr, Encoding.UTF8, "application/json");
                }

                result = await httpClient.SendAsync(requestMessage);
            }

            var resp = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(resp);
            }

            try
            {
                var j = JObject.Parse(resp);
                throw new ApiException((int)result.StatusCode, (string)j["detail"], (string)j["message"]);
            }
            catch (Exception e)
            {
                throw new ApiException((int)result.StatusCode, resp, e.Message);
            }
        }
    }
}