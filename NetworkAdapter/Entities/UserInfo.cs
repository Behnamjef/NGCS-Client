using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Entities
{
    public class UserInfo
	{
		public string userId { get; set; }
        public string lang { get; set; }
        public string clientVersion { get; set; }
        public string token { get; set; }
        public string gameCenterToken { get; set; }
        public DateTime validUntil { get; set; }
		public string name { get; set; }
		public int avatar { get; set; }
		public string email { get; set; }
		public int tutorial_step { get; set; }
        public DateTime? userTokenExpiry { get; set; } = null;
	}
}
