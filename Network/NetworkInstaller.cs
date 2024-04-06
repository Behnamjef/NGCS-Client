using NetworkAdapter.Brokers;
using System;
using System.Threading.Tasks;
using NetworkAdapter.Services.EventSocket;
using NGCS.Network;
using UnityEngine;

namespace NGCS.Network
{
    public class NetworkInstaller : MonoBehaviour
    {
        public NetworkHttpBrocker NetworkHttpBrocker { get; private set; }
        public ClientApiService ClientApiService { get; private set; }
        public EventSocketServices EventSocketServices { get; set; }
        public EventSocketListeners EventSocketListeners => EventSocketServices.EventListeners;

        private string BaseUrl => ServerConfig.Instance.BaseUrl;

        public void InitNetwork(Action onUnauthorized)
        {
            if (NetworkHttpBrocker != null) return;
            NetworkHttpBrocker = new NetworkHttpBrocker(BaseUrl);

            var UUID = GetUUID();
            Debug.Log("Init network With UUID : " + UUID);
            ClientApiService = new ClientApiService(NetworkHttpBrocker, UUID);
            ClientApiService.OnUnauthorizedCall += onUnauthorized;
        }

        public async Task InitEventSocket()
        {
            EventSocketServices?.Dispose();
            var tcs = new TaskCompletionSource<bool>();
            EventSocketServices = new EventSocketServices(NetworkHttpBrocker, BaseUrl);
            EventSocketServices.OnConnect += () => { tcs.TrySetResult(true); };
            EventSocketServices.StartSocket();
            await tcs.Task;
        }

        public string GetUUID()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public async Task InitMatchmaking()
        {
        }

        private void OnApplicationQuit()
        {
            Dispose();
        }

        private void Dispose()
        {
        }
    }
}