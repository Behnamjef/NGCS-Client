#if ZarinPalPayment || UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NGCS.Network;
using NetworkAdapter.Entities;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NGCS.IAP
{
    public class ZarinPalPurchaseHandler : MonoBehaviour, IPurchaseHandler
    {
        private ClientApiService _clientApiService;

        private TaskCompletionSource<bool> _paymentTCS;

        public async Task<bool> Init(string key, ClientApiService clientApiService)
        {
            _clientApiService = clientApiService;
            return true;
        }

        public async Task<List<BuyResponseModel>> GetPendingPurchases(GetPendingPurchasesRequestModel request,bool isForce)
        {
            if (!isForce)
                return new List<BuyResponseModel>();
            
            var parameters = new GetPendingPaymentsParams
            {
                userId = request.UserId,
                loginToken = request.LoginToken,
                app = Application.identifier,
            };
            var result = await _clientApiService.GetPendingPaymentAsync(parameters);

            return result.Select(p=> new BuyResponseModel
            {
                IsSuccess = true,
                Sku = p.sku,
                PaymentId = p.paymentId,
                Token = p.token,
                Meta = JObject.FromObject(p)
            }).ToList();
        }

        public async Task<BuyResponseModel> Buy(BuyRequestModel product)
        {
            _paymentTCS = new TaskCompletionSource<bool>();

            var parameters = new PaymentRequestParams
            {
                userId = product.UserId,
                appPackage = Application.identifier,
                callbackUrl = "hakemsho://",
                gateway = Gateway.zarinpal.ToString(),
                loginToken = product.LoginToken,
                sku = product.Sku
            };

            var response = await _clientApiService._authService.RequestPaymentAsync(parameters);
            if (response is not { IsSuccess: true })
                return new BuyResponseModel
                {
                    IsSuccess = false
                };

            Debug.Log($">>>>> payment id: {response.PaymentInfo.paymentId}");
            Application.OpenURL(response.PaymentInfo.paymentUrl);
            await _paymentTCS.Task;

            return new BuyResponseModel
            {
                IsSuccess = true,
                Sku = product.Sku,
                PaymentId = response.PaymentInfo.paymentId,
                Token = response.PaymentInfo.token,
                Meta = JObject.FromObject(response)
            };
        }

        public async Task<VerifyResponseModel> Verify(VerifyRequestModel request)
        {
            var parameters = new VerifyConsumRequestParams
            {
                userId = request.UserId,
                loginToken = request.LoginToken,
                paymentId = request.PaymentId,
                gateway = Gateway.zarinpal.ToString(),
                sku = request.Sku,
                appPackage = Application.identifier
            };
            var result = await _clientApiService.ConsumePaymentAsync(parameters);

            return new VerifyResponseModel
            {
                IsSuccess = result.IsSuccess,
                CoinsAfter = result.CoinAfter,
                CoinsBefore = result.CoinBefor
            };
        }

        public async Task<ConsumeResponseModel> Consume(ConsumeRequestModel request)
        {
            return new ConsumeResponseModel
            {
                IsSuccess = true
            };
        }

        public async void Dispose()
        {
            _paymentTCS?.TrySetCanceled();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                ResumePayment();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
                ResumePayment();
        }

        private void ResumePayment()
        {
            _paymentTCS?.TrySetResult(true);
        }
    }
}
#endif