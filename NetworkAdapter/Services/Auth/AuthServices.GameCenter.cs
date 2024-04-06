using NetworkAdapter.Entities;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetworkAdapter.Services
{
    public class LinkEmailResponse
    {
        public long resendLimitExpiresInMS;
        public long tokenExpiresInMS;
    }

    public class LinkPhoneResponse
    {
        public long resendLimitExpiresInMS;
        public long tokenExpiresInMS;
    }

    public class RecoveryResponde
    {
        public string method;
        public DateTime tempPassExpiration;
    }

    public enum RecoveryMethod
    {
        Telegram,
        Email,
        UUID,
        Phone,
        Unverified
    }

    public class LoginWithTempPassResponse
    {
        public string userId;
        public string loginToken;
    }

    public partial class AuthServices
    {
        private string CurrentLanguage => "Persian";//LocalizationManager.CurrentLanguage;


        public async Task<V2GameLogic> LoginIGCAsync()
        {
            return await gameCenter.CallApiPost<V2GameLogic>("/v2/auth/login", new
            {
                uuid = this.UUId,
                appid = gameKey
            }, CurrentLanguage);
        }

        public async Task<UserInfo> LoginNGCSAsync(string userId, string loginToken)
        {
            return await LoginUserAcync(userId, loginToken);
        }

        /// <summary>
        /// user this to make game center service loged in when you read user auth info from file 
        /// </summary>
        public void SetLoginInfoExternaly(UserInfo userInfo)
        {
            this.UserInfo = userInfo;
            httpClient.SetUserInfoExternaly(userInfo);
        }


        public async Task<LinkEmailResponse> LinkEmailAsync(string email)
        {
            if (!ApplicationConsts.IsEmailValid(email))
            {
                // throw new InvalidOperationException(ScriptLocalization.ServerError.InvalidEmail);
                throw new InvalidOperationException("");
            }

            var response = await gameCenter.CallApiPost<LinkEmailResponse>("/v2/auth/link-email",
                new
                {
                    userId = this.UserInfo.userId,
                    loginToken = this.UserInfo.gameCenterToken,
                    email = email,
                    useCode = true,
                    appid = gameKey
                }, CurrentLanguage);

            return (response);
        }

        public async Task<LinkPhoneResponse> LinkPhoneNumberAsync(string phoneNumber)
        {
            if (!ApplicationConsts.IsPhoneNumberValid(phoneNumber))
            {
                // throw new InvalidOperationException(ScriptLocalization.ServerError.InvalidPhoneNumber);
                throw new InvalidOperationException("");
            }

            var response = await gameCenter.CallApiPost<LinkPhoneResponse>("/v2/auth/link-phone",
                new
                {
                    userId = this.UserInfo.userId,
                    loginToken = this.UserInfo.gameCenterToken,
                    phone = phoneNumber,
                    appid = gameKey
                }, CurrentLanguage);

            return (response);
        }

        public async Task<bool> VerifyEmailAsync(string verificationToken)
        {
            var response = await gameCenter.CallApiPost<object>("/v2/auth/verify-email",
                new
                {
                    userId = this.UserInfo.userId,
                    token = verificationToken,
                    appid = gameKey
                }, CurrentLanguage);

            if (response != null)
                return true;
            else
                return false;
        }

        public async Task<bool> VerifyPhoneNumberAsync(string smsToken)
        {
            var response = await gameCenter.CallVerfyPhoneApi(HttpMethod.Post, "/v2/auth/verify-phone", CurrentLanguage,
                new
                {
                    userId = this.UserInfo.userId,
                    loginToken = this.UserInfo.gameCenterToken,
                    code = smsToken,
                    appid = gameKey
                });

            if (response != null)
                return true;
            else
                return false;
        }

        public async Task<RecoveryResponde> RequestRecoveryAsync(RecoveryMethod method, string address)
        {
            if (method == RecoveryMethod.Phone)
            {
                if (!ApplicationConsts.IsPhoneNumberValid(address))
                {
                    // throw new InvalidOperationException(ScriptLocalization.ServerError.InvalidPhoneNumber);
                    throw new InvalidOperationException("");
                }
            }

            if (method == RecoveryMethod.Email)
            {
                if (!ApplicationConsts.IsEmailValid(address))
                {
                    // throw new InvalidOperationException(ScriptLocalization.ServerError.InvalidEmail);
                    throw new InvalidOperationException("");
                }
            }

            var result = await gameCenter.CallApiPost<RecoveryResponde>("/v2/auth/recovery",
                new
                {
                    method = method.ToString(),
                    address = address,
                    appid = gameKey
                }, CurrentLanguage);

            return result;
        }

        /// <summary>
        /// this is a login based on temp password 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="method"></param>
        /// <param name="temppass"></param>
        /// <param name="uuid"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public async Task<UserInfo> RecoveryVerifyAsync(string address, RecoveryMethod method, string temppass,
            string uuid)
        {
            var respond = await gameCenter.CallApiPost<LoginWithTempPassResponse>("/v2/auth/recovery/verify",
                new
                {
                    method = method.ToString(),
                    address = address,
                    tempPass = temppass,
                    uuid = uuid,
                    appid = gameKey
                }, CurrentLanguage);

            if (respond != null && !string.IsNullOrEmpty(respond.loginToken))
            {
                // re login user 
                var model = await LoginUserAcync(respond.userId, respond.loginToken);
                model.gameCenterToken = respond.loginToken;

                this.UserInfo = model;

                return model;
            }
            else
                throw new ApiException("recovery verify", "invalid recovery request");
        }

        public async Task<PaymentVM> RequestPaymentAsync(PaymentRequestParams paymentRequestParams)
        {
            var RequestPaymentUrl = "/v2/payment/request";
            try
            {
                var respond = await gameCenter.CallApiPost<Payment>(RequestPaymentUrl, new
                {
                    userId = paymentRequestParams.userId,
                    loginToken = paymentRequestParams.loginToken,
                    gateway = paymentRequestParams.gateway,
                    appPackage = paymentRequestParams.appPackage,
                    sku = paymentRequestParams.sku,
                    callbackUrl = paymentRequestParams.callbackUrl,
                }, CurrentLanguage);

                if (respond != null && !string.IsNullOrEmpty(respond.paymentUrl))
                {
                    var model = new PaymentVM
                    {
                        IsSuccess = true,
                        PaymentInfo = respond
                    };
                    return model;
                }
            }
            catch (Exception e)
            {
                return new PaymentVM
                {
                    IsSuccess = false,
                    Message = e.Message
                };
            }

            throw new ApiException("Request Payment", "Invalid request payment");
        }
    }
}