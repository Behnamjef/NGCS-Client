using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Entities
{
    public class SignUpResponse
    {
        public string userId;
        public string password;
        public string loginToken;
        public bool newUser;
    }

    public class LoginTokenResponse
    {
        public string loginToken;
        public string newPassword;
    }

    public enum RecoveryMethod
    {
        Telegram, Email, UUID, Unverified
    }

    public class RecoveryUser
    {
        public string userId;
        public List<RecoveryOption> options;
        public List<RecoveryMethod> matched;
    }

    public class RecoveryOption
    {
        public RecoveryMethod method;
        public bool matched;
        public string displayData;
        public string displayField;
    }

    public class RecoveryResponse
    {
        public string userId;
        public string tempPass;
        public DateTime tempPassExpiration;
    }

    public class V2GameLogic
    {
        public string userId;
        public string loginToken;
    }

    public class LinkEmailResponse
    {
        public long resendLimitExpiresInMS;
        public long tokenExpiresInMS;
    }
}
