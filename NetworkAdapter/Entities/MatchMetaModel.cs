using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Entities
{
    public enum MatchResult
    {
        Winner = 1,
        Looser = 0,
    }

    public class EndMatchUserResult
    {
        public string UserId { get; set; }
        public MatchResult MatchResult { get; set; }
        public int CoinsBefor { get; set; }
        public int CoinsAfter { get; set; }
        public int TrophyBefor { get; set; }
        public int TrophyAfter { get; set; }
        public int XpBefor { get; set; }
        public int XpAfter { get; set; }
        public int TotalWins { get; set; }
        public int TotalLoses { get; set; }
        public long TotalTimeBefor { get; set; }
        public long TotalTimeAfter { get; set; }
        public UserNewLvlsAndPrize UserNewLvlsAndPrizes { get; set; } = new UserNewLvlsAndPrize();
    }

    public class EndMatchResult
    {
        public List<EndMatchUserResult> EndUsersResult { get; set; } = new List<EndMatchUserResult>();

        public int TotalPlayedMinuts { get; set; }

    }
}
