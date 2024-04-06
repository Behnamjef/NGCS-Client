using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Entities
{
    public class NetworkConfig
    {
        public string UUId { get; set; }
        public string UdpRoomAddress { get; set; }
        public string BaseApiAddress { get; set; } 
        public string GameCenterBaseUrl { get; set; } 
    }
}
