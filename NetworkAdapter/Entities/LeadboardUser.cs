using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Entities
{
    public class LeadboardUser
    {
        public string Name { get; set; } = "";
        public int? AvatarId { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int Trophy { get; set; }
        public long Rank { get; set; } = 0;
    }

    public class HokmLeaderboard
    {
        public DateTime NextReset;
        public DateTime EndDate;
        public List<LeadboardUser> TopUsers = new List<LeadboardUser>();
        public LeadboardUser CurrentUser = new LeadboardUser();
    }
}
