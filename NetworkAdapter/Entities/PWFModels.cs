using NetworkAdapter.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Entities
{
    public class UserFriendsSearchVM
    {
        public string UserId { get; set; }
        public string Name { get; set; } = "";
        public int? AvatarId { get; set; }
        public bool IsAlreadyFriend { get; set; }
        public int Trophy { get; set; }
        public int XP { get; set; }
        public UserLevelViewModel UserCurrentXpLevel { get; set; }
    }

    public class UserFriendsListVM
    {
        public string UserId { get; set; }
        public string Name { get; set; } = "";
        public int? AvatarId { get; set; }
        public int Trophy { get; set; }
        public int XP { get; set; }
        public UserLevelViewModel UserCurrentXpLevel { get; set; }
    }


    #region MatchMaking

    public class FriendlyMatchRequestConfig
    {
        public int Rounds { get; set; }
        public List<string> InvitedUsers { get; set; } = new List<string>();
    }


    public class FrindlyMatchInfo
    {
        public string MatchToken { get; set; }
        public string TrackerId { get; set; }
        public DateTime ExpiersAtUTC { get; set; }
        public RoomsDTO RoomConfig { get; set; }
        public List<string> InvitedUsers { get; set; } = new List<string>();
    }

    public class FriendlyMatchInvitationVM
    {
        public string TrackerId { get; set; }
        public string MatchToken { get; set; }
        public DateTime ExpiersAtUTC { get; set; }
        public UserFriendsListVM Sender { get; set; }
        public FriendlyMatchRequestConfig FriendlyMatchConfig { get; set; }
    }

    #endregion
}