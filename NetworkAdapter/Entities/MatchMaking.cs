using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Entities
{

    public class UserVM
    {
        public string user_id { get; set; }
        public string name { get; set; } = "";
        public int? exp { get; set; }
        public int? avatar_id { get; set; } = 0;
        public int? trophy { get; set; }

    }

    public class MatchParticipant
    {
        public string userId;
        public int playerIndex;
        public bool reject;
        public bool pending;
        public UserVM metaData;
    }

    public class MatchMakingTracker
    {
        public string id;
        public int version;
        public int mmr;
        public MatchMakingOptions options;
        public DateTime expiry;
        public bool ended;
        public bool matched;
        public List<MatchParticipant> participants;

        public MatchMakingTracker GetMatchMakingTrackerClone()
        {
            return JToken.FromObject(this).ToObject<MatchMakingTracker>();
        }
    }

    public class PrivateMatchMakingTracker : MatchMakingTracker
    {
        public HashSet<string> botUserIds;
    }

    public struct MatchMakingOptions
    {
        public string category;
        /// <summary>
        /// Note: Only participants with same offset will match if their mmrs are in range,
        /// e.g. mmr1=58, mmr2=70 will match if offset for both requests are 50, but won't match if one is 50 and other 100
        /// </summary>
        public int offset;
        public int minCount;
        public int maxCount;
        public TimeSpan timeout;

        public MatchMakingOptions GetClone()
        {
            return this;
        }

        public int GetGroupNumber(int mmr)
        {
            return ((int)(mmr / (offset + 1))) * (offset + 1);
        }
    }

    public class PlannedBot
    {
        public DateTime due;
        public int maxMatched;
        public string userId;

        public bool CanApply(int currentCount, DateTime now) => currentCount <= maxMatched && now >= due;
    }

}
