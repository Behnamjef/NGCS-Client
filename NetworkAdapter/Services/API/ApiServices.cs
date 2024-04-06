using NetworkAdapter.Entities;
using NetworkAdapter.Brokers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Code.Scripts.NetworkAdapter.Entities;

namespace NetworkAdapter.Services
{
    public partial class ApiServices
    {
        public event Action OnUnauthorizedCall;

        private static readonly string matchMakingUri = "matchmaking/request/{0}?isFourPlayer=true";
        private static readonly string trackUri = "matchmaking/track/{0}?maxWait={1}&isFourPlayer=true";
        private static readonly string matchCancelUri = "matchmaking/cancel/{0}";
        private static readonly string getRoomIdUri = "room-url/{0}";
        private static readonly string getLeaderboardUri = "Leaderboard/getLeaderboard/{0}";
        private static readonly string PurchasVerify = "Purchas/Verify";
        private static readonly string ConsumeWof = "Meta/ConsumeWof/v2";
        private static readonly string ConsumeAd = "Meta/ConsumeAd";
        private static readonly string ConsumeGiftCodeUri = "Meta/ConsumeGiftCode/{0}";
        private static readonly string ConsumeFreePackage = "Purchas/ConsumeFreePackage";
        private static readonly string RequestPaymentUri = "Payment/Request";
        private static readonly string VerifyPaymentUri = "Payment/Verify";
        private static readonly string GetPendingUri = "Payment/GetPending";

        // General
        private static readonly string GetInitData = "General/InitData";
        private static readonly string EmergencyUri = "General/Emergency";

        // User
        private static readonly string SetCampaignStatusUri = "user/SetCampaignStatus";
        private static readonly string ConsumeRefferal = "user/ConsumeRefferalCode/{0}/{1}";
        private static readonly string ConsumeFreeCode = "user/ConsumeFreeCoin/{0}";
        private static readonly string updateDisplayName = "user/updateDisplayName";
        private static readonly string UpdateUserAvatar = "user/updateUserAvatar";
        private static readonly string UserPrizeConsume = "user/userPrizeConsume";
        private static readonly string equipptItem = "user/equipptItem";
        private static readonly string changeEmail = "user/changeEmail/{0}";
        private static readonly string changePhone = "user/changePhone/{0}";
        private static readonly string updateTutorialStepUri = "user/updateTutorialStep";
        private static readonly string updateTutorialUri = "user/updateTutorial";
        private static readonly string rateUsUri = "user/SetRateUs";

        //PWF
        private static readonly string pwfSearchUri = "PWF/search/{0}";
        private static readonly string friendsListUri = "PWF/friendsList";
        private static readonly string addFriendUri = "PWF/addFriend";
        private static readonly string deleteFriendsUri = "PWF/DeleteFriends";

        private static readonly string createFriendlyMatchUri = "matchmaking/pwf/create";
        private static readonly string requestJoinFriendlyMatchUri = "matchmaking/pwf/join/{0}";
        private static readonly string getFriendlyMatchInvitationsUri = "matchmaking/pwf/Invitations";
        private static readonly string CancelFrindlyMatchUri = "matchmaking/pwf/cancel/{0}";
        private static readonly string CancelFrindlyInvitation = "matchmaking/pwf/invitationCancel/{0}";

        private NetworkHttpBrocker httpClient;

        public ApiServices(NetworkHttpBrocker networkHttpBrocker)
        {
            this.httpClient = networkHttpBrocker;
            this.httpClient.OnUnauthorizedCall += this.OnUnauthorizedCall;
        }

        public async Task<string> RequestMatchAsync(int roomName)
        {
            try
            {
                var trackerId = await httpClient.Get<string>(string.Format(matchMakingUri, roomName));
                return trackerId;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<PrivateMatchMakingTracker> TrackAsync(string matchId)
        {
            try
            {
                var model = await httpClient.Get<PrivateMatchMakingTracker>
                    (string.Format(trackUri, matchId, 20));
                //SetMetaForBots(model);
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<bool> MatchCancelAsync(string matchId)
        {
            try
            {
                var model = await httpClient.Get<bool>
                    (string.Format(matchCancelUri, matchId));

                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<string> GetRoomIdAsync(string matchId)
        {
            try
            {
                var model = await httpClient.Get<string>(string.Format(getRoomIdUri, matchId));

                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<UserNameChangeResult> UpdateDisplayNameAsync(string displayName)
        {
            try
            {
                var model = await httpClient.Post<object, UserNameChangeResult>(updateDisplayName, new { displayName });

                // update inner models
                if (model.IsSuccess)
                    httpClient.userInfo.name = model.NewName;

                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<bool> UpdateAvatarAsync(int avatar)
        {
            try
            {
                var model = await httpClient.Post<object, bool>(UpdateUserAvatar, new { avatar });

                // update inner models
                if (model)
                    httpClient.userInfo.avatar = avatar;

                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<int> UpdateTutorialStepAsync(int stepIncrement)
        {
            try
            {
                var model = await httpClient.Post<object, int>(updateTutorialStepUri, new { stepIncrement });

                // update inner models
                httpClient.userInfo.tutorial_step = model;

                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        /// <summary>
        /// Update entier step
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public async Task<int> UpdateTutorialAsync(int step)
        {
            try
            {
                var model = await httpClient.Post<object, int>(updateTutorialUri, new { step });

                // update inner models
                httpClient.userInfo.tutorial_step = model;

                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<UserNewLvlsAndPrize> UserPrizeConsumeAsync(long prizeId)
        {
            try
            {
                var model = await httpClient.Post<object, UserNewLvlsAndPrize>(UserPrizeConsume, new { prizeId });

                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<HokmLeaderboard> GetLeaderboardAsync(string boardName = "default")
        {
            try
            {
                var model = await httpClient.Get<HokmLeaderboard>(String.Format(getLeaderboardUri, boardName));
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<GameInitDataVM> GetGameInitDataAsync(GameInitParams gameInitParams)
        {
            try
            {
                var model = await httpClient.Post<GameInitParams, GameInitDataVM>(GetInitData, gameInitParams);
                HandleNotice(model.Notices);
                HandleRooms(model.Rooms);
                HandleConfig(model.Config);

                // update inner models
                httpClient.userInfo.clientVersion = gameInitParams.ClientVersionString;
                httpClient.userInfo.lang = gameInitParams.Language;

                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        private void HandleConfig(AppConfigDto config)
        {
            if (string.IsNullOrWhiteSpace(config.data))
            {
                config.config_data = JsonConvert.DeserializeObject<ConfigData>(config.data);
            }
        }

        private static void HandleNotice(List<NoticeDto> notices)
        {
            notices.ForEach(i => i.buttonsCasted = JsonConvert.DeserializeObject<List<Button>>(i.buttons));
        }

        private static void HandleRooms(List<RoomsDTO> rooms)
        {
            rooms.ForEach(i =>
            {
                if (!string.IsNullOrEmpty(i.entry_meta))
                    i.entry_meta_object = JsonConvert.DeserializeObject<List<EntryMetaVM>>(i.entry_meta);
                if (!string.IsNullOrEmpty(i.view_options_json))
                    i.view_options_object = JsonConvert.DeserializeObject<RoomViewOptions>(i.view_options_json);
            });
        }



        // return user coins after verification
        public async Task<PurchaseResult> PurchaseVerifyAsync(IAPBuyRequest iAPBuyRequest)
        {
            try
            {
                UnityEngine.Debug.Log(
                    $"requesting verify IAP with URL {httpClient.apiBasicUri + PurchasVerify} with data {JsonConvert.SerializeObject(iAPBuyRequest)}");
                var model = await httpClient.Post<IAPBuyRequest, PurchaseResult>(PurchasVerify, iAPBuyRequest);
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        // return user purchase free package result 
        public async Task<PurchaseResult> ConsumeFreePackageAsync(int packageId)
        {
            try
            {
                var model = await httpClient.Post<object, PurchaseResult>(ConsumeFreePackage,
                    new { packageId = packageId });
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        // return user new equippted items
        public async Task<EquiptedItems> EquipptItemAsync(int packageId)
        {
            try
            {
                var model = await httpClient.Post<object, EquiptedItems>(equipptItem, new { packageId = packageId });
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        // return next WOF after consume
        public async Task<WofConsumeResult> ConsumeWofAsync()
        {
            try
            {
                var model = await httpClient.Get<WofConsumeResult>(ConsumeWof);
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        // return next AD after consume
        public async Task<UserVideoAd> ConsumeAdAsync()
        {
            try
            {
                var model = await httpClient.Get<UserVideoAd>(ConsumeAd);
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<CodeConsumedResult> ConsumeGiftCode(string giftCode)
        {
            try
            {
                var model = await httpClient.Get<CodeConsumedResult>(String.Format(ConsumeGiftCodeUri, giftCode));
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<ChangeEmailOrPhoneResult> EditEmailAsync(string newEmail)
        {
            try
            {
                var model = await httpClient.Get<ChangeEmailOrPhoneResult>(String.Format(changeEmail, newEmail));
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<ChangeEmailOrPhoneResult> EditPhonelAsync(string newPhone)
        {
            try
            {
                var model = await httpClient.Get<ChangeEmailOrPhoneResult>(String.Format(changePhone, newPhone));
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<FreeCoinConsumResult> ConsumeRefferalAsync(int freeCoinId, string referralCode = "")
        {
            try
            {
                var model = await httpClient.Get<FreeCoinConsumResult>(string.Format(ConsumeRefferal, freeCoinId,
                    referralCode));
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<FreeCoinConsumResult> ConsumeFreeCoinAsync(int freeCoinId)
        {
            try
            {
                var model = await httpClient.Get<FreeCoinConsumResult>(string.Format(ConsumeFreeCode, freeCoinId));
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<EmergencyVM> GetEmergencyAsync(EmergencyParams emergencyParams)
        {
            try
            {
                var model = await httpClient.Post<EmergencyParams, EmergencyVM>(EmergencyUri, emergencyParams);
                model.ForceUpdates.ForEach(
                    i => i.buttonsCasted = JsonConvert.DeserializeObject<List<Button>>(i.buttons));
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<ConsumVM> ConsumePaymentAsync(VerifyConsumRequestParams requestParams)
        {
            try
            {
                var model = await httpClient.Post<VerifyConsumRequestParams, ConsumVM>(VerifyPaymentUri, requestParams);
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<List<Payment>> GetPendingPaymentAsync(GetPendingPaymentsParams requestParams)
        {
            try
            {
                var model = await httpClient.Post<GetPendingPaymentsParams, List<Payment>>(GetPendingUri,
                    requestParams);
                return model;
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<bool> SetRateUsAsync(UserRateUs rateUsDate)
        {
            try
            {
                return await httpClient.Post<object, bool>(rateUsUri, rateUsDate);
            }
            catch (ApiException e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<bool> SetCampaignStatus(TrackerData trackerData)
        {
            try
            {
                return await httpClient.Post<object, bool>(SetCampaignStatusUri, trackerData);
            }
            catch (ApiException e)
            {
                return false;
            }
        }


        /// <summary>
        /// on return null = could not found user
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"> display : shekeli pish amade</exception>
        public async Task<UserFriendsSearchVM?> PwfSearchUserAsync(string searchString)
        {
            try
            {
                var model = await httpClient.Get<UserFriendsSearchVM>(string.Format(pwfSearchUri, searchString));
                return model;
            }
            catch (ApiException e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<List<UserFriendsListVM>?> GetFriendsListAsync()
        {
            try
            {
                var model = await httpClient.Get<List<UserFriendsListVM>>(friendsListUri);

                return model;
            }
            catch (ApiException e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        /// <summary>
        /// on return false = user already exist
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"> display : shekeli pish amade</exception>
        public async Task<bool> AddFriendAsync(string friendUserId)
        {
            try
            {
                return await httpClient.Post<object, bool>(addFriendUri, new { friendId = friendUserId });
            }
            catch (ApiException e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        /// <summary>
        /// on return false = on or more users do not exist on user friends list
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"> display : shekeli pish amade</exception>
        public async Task<bool> RemoveFriendsAsync(List<string> friendsUserId)
        {
            try
            {
                return await httpClient.Post<object, bool>(deleteFriendsUri, new { friendsToRemove = friendsUserId });
            }
            catch (ApiException e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }


        public async Task<FrindlyMatchInfo> CreateFriendlyMatchAsync(FriendlyMatchRequestConfig requestConfig)
        {
            try
            {
                // if user ids are invalid in invitation list we do not check them and request is still valid
                // respond atleast have a 200ms delay
                var info = await httpClient.Post<FriendlyMatchRequestConfig, FrindlyMatchInfo>(createFriendlyMatchUri, requestConfig);
                info.RoomConfig.view_options_object = JsonConvert.DeserializeObject<RoomViewOptions>(info.RoomConfig.view_options_json);
                return info;
            }
            catch (ApiException e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        /// <summary>
        ///  join a friendly match by matchToken
        /// </summary>
        /// <param name="matchToken">matchToken Given from CreateFriendly and invitations</param>
        /// <returns></returns>
        /// <exception cref="ApiException">shekeli pish amade</exception>
        public async Task<FrindlyMatchInfo> RequestJoinFriendlyMatchAsync(string matchToken)
        {
            try
            {
                var info = await httpClient.Get<FrindlyMatchInfo>(string.Format(requestJoinFriendlyMatchUri, matchToken));
                info.RoomConfig.view_options_object = JsonConvert.DeserializeObject<RoomViewOptions>(info.RoomConfig.view_options_json);
                return info;
            }
            catch (ApiException e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

        public async Task<List<FriendlyMatchInvitationVM>> GetUserInvitationsAsync()
        {
            try
            {
                return await httpClient.Get<List<FriendlyMatchInvitationVM>>(getFriendlyMatchInvitationsUri);
            }
            catch (ApiException e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }


        public async Task<bool> CancelFriendlyMatchAsync(string matchToken)
        {
            try
            {
                return await httpClient.Get<bool>(string.Format(CancelFrindlyMatchUri, matchToken));
            }
            catch (ApiException e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }


        public async Task<bool> CancelFriendlyInvitationAsync(string matchToken)
        {
            try
            {
                return await httpClient.Get<bool>(string.Format(CancelFrindlyInvitation, matchToken));
            }
            catch (ApiException e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }

    }
}