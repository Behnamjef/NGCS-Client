using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Entities
{
    public class LevelDTO
    {
        public int id { get; set; }
        public int type { get; set; }
        public int version { get; set; }
        public int? segment_id { get; set; }
        public string title { get; set; }
        public List<LvlMeta> Lvl_meta_casted { get; set; }
        public int from_amount { get; set; }
        public int to_amount { get; set; }
        public string? lvl_meta { get; set; }
        public string? prize_meta { get; set; }

    }

    public enum LevelType : int
    {
        XPBase = 0,
        TrophyBase = 1,
        WinBase = 2,
        Achivment = 3
    }

    public enum LevelMetaType : int
    {
        Asset = 0,
        Url = 1,
        WinAmount = 2,
        LoseAmount = 3,
        DefaultFocusTableId = 4
    }

    public enum PrizeMetaTypes : int
    {
        Message = 0,
        Coin = 1,
        Deck = 2,
        Equipment = 3,
        Avatar = 4,
        Achivment = 5,
        Trophy = 6,
        Table = 7,
    }

    public class LvlMeta
    {
        public LevelMetaType Type { get; set; }
        public string Amount { get; set; }
    }
    public class UserLevelViewModel
    {
        public int LevelId { get; set; }
        public int? PreviosLevelId { get; set; }
        public string LevelTitle { get; set; }
        public List<LvlMeta> LevelMetaData { get; set; } = new List<LvlMeta>();
        public List<PrizeMetaViewModel> PrizeMetaData { get; set; } = new List<PrizeMetaViewModel>();
        public int? LevelBottomAmount { get; set; }
        public int? LevelTopAmount { get; set; }
        public int? UserCurrentAmount { get; set; }
        public int? LevelProgressPrc { get; set; }

    }


    public class PrizeMetaViewModel
    {
        public PrizeMetaTypes Type { get; set; }
        public string Icon { get; set; }
        public string Data { get; set; }
    }

    public class UserPrizeViewModel
    {
        public long Id { get; set; }
        public int? LeveId { get; set; }
        public int? EventId { get; set; }
        public string? LeaderbordEnum { get; set; }
        public LevelType PrizeLevelType { get; set; }
        public List<PrizeMetaViewModel> PrizeMeta { get; set; }
        public bool IsConsumed { get; set; }
        public DateTime EarnedDdate { get; set; }
        public DateTime? Consumed_date { get; set; }
    }

    public class UserNewLvlsAndPrize
    {
        public UserLevelViewModel? UserNewXpLevel { get; set; }
        public UserLevelViewModel? UserNewTrophyLevel { get; set; }
        public List<UserPrizeViewModel> UserNewPrizes { get; set; } = new List<UserPrizeViewModel>();
    }

}
