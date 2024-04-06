using System.Threading.Tasks;
using NetworkAdapter.Brokers;
using NetworkAdapter.Entities;
using NetworkAdapter.Services;
using UnityEngine;

namespace NGCS.Network
{
    public class ClientApiService : ApiServices
    {
        public readonly NetworkHttpBrocker Broker;

        public readonly AuthServices _authService;

        public ClientApiService(NetworkHttpBrocker networkHttpBroker, string customIdentifier = null) : base(networkHttpBroker)
        {
            Broker = networkHttpBroker;
            _authService = new AuthServices(networkHttpBroker, string.IsNullOrEmpty(customIdentifier) ? SystemInfo.deviceUniqueIdentifier : customIdentifier,
                ServerConfig.Instance.GameCenterUrl);
        }

        // this is just for test. fix it later dear parsa
        public async Task<GameInitDataVM> InitData(GameInitParams initParams)
        {
            Debug.Log("initData Begin ...");
            var initData = await this.GetGameInitDataAsync(initParams);
            if (initData == null) return null;

            Debug.Log($"init data recived");
            return initData;
        }

        // this is just for test. fix it later dear parsa
        public async Task<HokmLeaderboard> Getleaderboard()
        {
            Debug.Log("getLeaderboard Begin ...");
            var leaderboard = await this.GetLeaderboardAsync();
            if (leaderboard == null) return null;

            Debug.Log($"leaderboard data recived");
            return leaderboard;
        }
    }
}