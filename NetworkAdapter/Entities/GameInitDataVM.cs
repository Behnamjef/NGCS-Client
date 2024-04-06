using NetworkAdapter.Services;
using System;
using System.Collections.Generic;

namespace NetworkAdapter.Entities
{
    public class UserVideoAd
    {
        public DateTime NextAvailableDate { get; set; }
        public int WatchLimitRemain { get; set; }
        public int Prize { get; set; }
    }

    public class UserWOF
    {
        public DateTime NextAvailableTime { get; set; }
        public double HoursToActivate { get; set; }
        public int Prize { get; set; }
        public bool IsPremium { get; set; }
        public int? PremiumPackageId { get; set; }
        public List<WOFItem> Items { get; set; }
        public int Counts { get; set; }
    }

    public class WOFItem
    {
        public int Prize { get; set; }
        public int ChancePercentage { get; set; }
    }

    public class GameInitParams
    {
        public string Platform;
        public string App;
        public string Uuid;
        public string UserId;
        public int ClientVersionNum;
        public string ClientVersionString;
        public int? Catalogversion;
        public int? TrophyLevelVersion;
        public int? XpLevelVersion;
        public string Language;
    }
    public class GameInitDataVM
    {
        public List<NoticeDto> Notices { get; set; }
        public CatalogPackageVM Catalog { get; set; }
        public UserDto User { get; set; }
        public AppConfigDto Config { get; set; }
        public List<RoomsDTO> Rooms { get; set; }
        public List<LevelDTO> TrophyLeagues { get; set; }
        public List<LevelDTO> XpLevels { get; set; }
        public List<FreeCoinsDto> FreeCoins { get; set; }
        public List<UsersInboxDto> UserInboxs { get; set; }
    }

    public class AppConfigDto
    {
        public int id { get; set; }
        public string data { get; set; }
        public ConfigData config_data { get; set; } = new ConfigData();
        public int version { get; set; }
        public int? segment_id { get; set; }

        public static AppConfigDto DefualtConfig = new AppConfigDto();
    }


    public enum AppTheme
    {
        None = 0,
        Noruz = 1,
        HafteNiruyeEntezami = 2,
    }

    public class ConfigData
    {
        public int InitialCoin { get; set; } = 150;
        public int InitialTrophy { get; set; } = 50;
        public int InitialXp { get; set; } = 0;
        public int PWFEntry { get; set; } = 100;
        public int PWFRoomId { get; set; } = 100;
        public int PWFMatchMakingWaitSec { get; set; } = 5 * 60; // 5 min
        public int PWFMaxPlayer { get; set; } = 2;
        public AppTheme AppTheme { get; set; }
    }

    public class UserDto
    {
        public string user_id { get; set; }
        public string name { get; set; } = "";
        public string email { get; set; } = "";
        public string phone { get; set; }
        public int tutorial_step { get; set; } = 0;
        public int wins { get; set; } = 0;
        public int loses { get; set; } = 0;
        public int leaves { get; set; } = 0;
        public int coins { get; set; } = 0;
        public int? segment_id { get; set; }
        public int? exp { get; set; }
        // public int? avatar_enum { get; set; }
        public int? trophy { get; set; }
        public int? name_changes { get; set; }
        public UserVideoAd? video_ad { get; set; }
        public UserWOF? wof { get; set; }

        public UserLevelViewModel UserXPLevelInfo { get; set; }
        public UserLevelViewModel UserTrophyLevelInfo { get; set; }
        public List<UserPrizeViewModel> UserPrizes { get; set; }

        public EquiptedItems EquiptedItems { get; set; }
        public List<int> OwnedItems { get; set; }

        public List<string> UserRefferes { get; set; } = new List<string>();
        public List<int> UserFreeCoins { get; set; } = new List<int>();

        public UserRateUs? rate_us { get; set; }
    }

    public enum RateUsResult
    {
        Never = 0,
        Ok = 1,
        Cancel = 2
    }

    public class UserRateUs
    {
        public RateUsResult RateUsResult { get; set; }
        public int StarCount { get; set; }
    }

    public class EquiptedItems
    {
        public int card { get; set; }
        public int avatar { get; set; }
        public int sigil { get; set; }
        public int belonging { get; set; }
    }




    public class CatalogPackageVM
    {
        public CatalogDto catalog { get; set; }
        public List<PackageDto> packages { get; set; } = new List<PackageDto>();
        public List<PackageDto> notCoinPackages { get; set; } = new List<PackageDto>();

    }
    public class CatalogDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public int version { get; set; }
        public int referral_prize { get; set; }
        public int? segment_id { get; set; }
        public string view { get; set; }
        public bool active { get; set; }

    }

    public enum PackageTypes : int
    {
        coin = 0,
        card = 1,
        avatar = 2,
        sigil = 3,
        belonging = 4
    }

    public enum FreeCoinType : int
    {
        Refferal = 0,
        Instagram = 1,
        WOF = 2,
        GiftCode = 3,
        enterRefferal = 4,
    }

    public class FreeCoinsDto
    {
        public int id { get; set; }
        public FreeCoinType type { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public string data { get; set; }
        public string icon { get; set; }
        public int? coins { get; set; }
        public bool is_active { get; set; }
        public int? segmetn_id { get; set; }

    }

    public class PackageDto
    {
        public int id { get; set; }
        public int catalog_id { get; set; }
        public int? display_index { get; set; }
        public PackageTypes type { get; set; }
        public string sku { get; set; }
        public int amount { get; set; }
        public int toman { get; set; }
        public int cents { get; set; }
        public bool active { get; set; }
        public string extra { get; set; }
        public DateTime? extra_expiery { get; set; }
        public string? view { get; set; } //{icon=0 , name = "سکه بزرگ "}
        public PckageView view_object { get; set; }
        public int? promotion_pack_id { get; set; }
        public int? coins_for_unlock { get; set; }
        public int? extra_coin { get; set; }
    }

    public class PckageView
    {
        public string icon { get; set; }
        public string name { get; set; }
    }

    public class NoticeDto
    {
        public int id { get; set; }
        public string title_persian { get; set; }
        public string title_english { get; set; }
        public string message_persian { get; set; }
        public string message_english { get; set; }
        public DateTime create_at { get; set; }
        public string? image_url { get; set; }
        public string platforms { get; set; }
        public int? max_client_version { get; set; }
        public int? min_client_version { get; set; }
        public DateTime? expiery { get; set; }
        public int notice_type { get; set; } = (int)noticeTypes.news;
        public bool active { get; set; }
        public string buttons { get; set; } = "[]";
        public List<Button> buttonsCasted { get; set; }
        public int? segment_id { get; set; }


    }

    public class UsersInboxDto
    {
        public int id { get; set; }
        public string user_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime expires_at { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public string buttons { get; set; }
        public bool is_active { get; set; }
    }


    public class Button
    {
        public string data { get; set; }
        public string title { get; set; }
        public ButtonBehaviour behaviour { get; set; }
    }

    public enum ButtonBehaviour : int
    {
        DoNothing = 0, // close popup
        ForceUpdate = 1, // update url is in data
        MinorUpdate = 2,
        OpenUrl = 3, // URL is in data
        GoToShop = 4, // in data should be tab index (0 1 2) if needed
        GoToMenu = 5,
        GoToMatch = 6, // match Id is in data
    }


    public enum EntryMetaTypes
    {
        Xp = 0,
        Trophy = 1,
        Level = 2
    }

    public class EntryMetaVM
    {
        public EntryMetaTypes Type { get; set; }
        public int Value { get; set; }
    }


    public class RoomsDTO
    {
        public int id { get; set; }
        public bool active { get; set; }
        public string name { get; set; }
        public int entry { get; set; }
        public string? entry_meta { get; set; }
        public List<EntryMetaVM>? entry_meta_object { get; set; }
        public int prize { get; set; }
        public string? prize_meta { get; set; }
        public int per_match_sec { get; set; }
        public string? user_requirments { get; set; }
        public bool text_chat_open { get; set; }
        public int? room_time_sec { get; set; }
        public int min_user { get; set; }
        public int max_user { get; set; }
        public int? bots { get; set; }
        public int end_exp { get; set; }
        public string? other_settings { get; set; }
        public int? end_exp_winner { get; set; }
        public int? segmetn_id { get; set; }
        public int trophy_weight { get; set; }
        public string view_options_json { get; set; }

        // Client objects
        public RoomViewOptions? view_options_object { get; set; }
    }
}
