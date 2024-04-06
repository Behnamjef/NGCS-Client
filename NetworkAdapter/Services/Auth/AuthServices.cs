using NetworkAdapter.Brokers;
using NetworkAdapter.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace NetworkAdapter.Services
{
    public partial class AuthServices
    {
        // this will fill after login
        private UserInfo UserInfo { get; set; } = null;
        private string UUId { get; set; }

        private static readonly int gameKey = 4;
        private static readonly string loginUri = "auth/login";

        private NetworkHttpBrocker httpClient;
        private IncytelGameCenterBrocker gameCenter;

        public AuthServices(NetworkHttpBrocker networkHttpBrocker, string UUId, string GameCenterBaseUrl)
        {
            this.UUId = UUId;
            this.httpClient = networkHttpBrocker;
            this.gameCenter = new IncytelGameCenterBrocker(GameCenterBaseUrl);
        }

    }
}
