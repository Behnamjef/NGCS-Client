using NetworkAdapter.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Services.EventSocket
{
    public enum PushEventType
    {
        TrackerChange = 0,
        FriendlyInvitationSend = 1,
        FriendlyInvitationsUpdate = 2,
    }

    public class DirectMessage
    {
        public PushEventType Type { get; set; }
        public string Message { get; set; }
    }
    public class EventSocketListeners
    {
        public event Action<MatchMakingTracker> OnMatchMakingTrakerRecived;
        public event Action<FriendlyMatchInvitationVM> OnNewInvitationReceived;
        public event Action<List<FriendlyMatchInvitationVM>> OnInvitationListChanged;

        public void CompileEvent(string msg)
        {
            var mainMessage = JsonConvert.DeserializeObject<DirectMessage>(msg);

            switch (mainMessage.Type)
            {
                case PushEventType.TrackerChange:
                    var tracker = JsonConvert.DeserializeObject<MatchMakingTracker>(mainMessage.Message);
                    OnMatchMakingTrakerRecived?.Invoke(tracker);
                    break;

                case PushEventType.FriendlyInvitationSend:
                    var invitaion = JsonConvert.DeserializeObject<FriendlyMatchInvitationVM>(mainMessage.Message);
                    OnNewInvitationReceived?.Invoke(invitaion);
                    break;

                case PushEventType.FriendlyInvitationsUpdate:
                    var invitationsList = JsonConvert.DeserializeObject<List<FriendlyMatchInvitationVM>>(mainMessage.Message);
                    OnInvitationListChanged?.Invoke(invitationsList);
                    break;
            }
        }
    }
}
