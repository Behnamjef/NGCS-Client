#if Bazaar
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bazaar.Data;
using UnityEngine;
using Bazaar.Poolakey;
using Bazaar.Poolakey.Data;
using NGCS.Network;
using NetworkAdapter.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Payment = Bazaar.Poolakey.Payment;

namespace NGCS.IAP
{
    public class BazaarPurchaseHandler : IPurchaseHandler
    {
        private Payment _payment;
        private SecurityCheck _securityCheck;
        private ClientApiService _clientApiService;

        public async Task<bool> Init(string key, ClientApiService clientApiService)
        {
            _clientApiService = clientApiService;

            _securityCheck = SecurityCheck.Enable(key);
            _payment = new Payment(new PaymentConfiguration(_securityCheck));

            var result = await _payment.Connect();

            return result.status == Status.Success;
        }

        public async Task<List<BuyResponseModel>> GetPendingPurchases(GetPendingPurchasesRequestModel request,
            bool isForce)
        {
            if (!isForce) return null;
            var inventory = await _payment.GetPurchases();
            if (inventory.status == Status.Success)
            {
                var notConsumedItems = inventory.data.Where(x => x.purchaseState == PurchaseInfo.State.Purchased)
                    .Select(pi => (new BuyResponseModel
                    {
                        IsSuccess = true,
                        Sku = pi.productId,
                        Token = pi.purchaseToken,
                        Meta = JObject.FromObject(pi)
                    })).ToList();

                Debug.LogWarning($"Bazaar - Not consumed items count {notConsumedItems.Count}");
                return notConsumedItems;
            }
            else
            {
                Debug.LogWarning($"Bazaar - Couldn't query old purchases {inventory.message}");
                return null;
            }
        }

        public async Task<BuyResponseModel> Buy(BuyRequestModel product)
        {
            var result = await _payment.Purchase(product.Sku, SKUDetails.Type.inApp, null, null,$"UserId: {product.UserId}");
            return new BuyResponseModel
            {
                IsSuccess = result.status == Status.Success,
                Token = result.data.purchaseToken,
                Meta = JObject.FromObject(result)
            };
        }

        public async Task<VerifyResponseModel> Verify(VerifyRequestModel request)
        {
            var iapRequest = new IAPBuyRequest
            {
                UserId = request.UserId,
                IAPGatewayType = IAPGatewayType.bazaar,
                PackageId = request.PackageId,
                Receipt = request.Receipt
            };
            var result = await _clientApiService.PurchaseVerifyAsync(iapRequest);
            Logger.Log($">>>>>> bazaar verify: {JsonConvert.SerializeObject(result)}");
            var response = new VerifyResponseModel
            {
                IsSuccess = result.IsSuccess || result.IsConsume,
                CoinsBefore = result.CoinsBefor,
                CoinsAfter = result.CoinsAfter
            };

            return response;
        }

        public async Task<ConsumeResponseModel> Consume(ConsumeRequestModel request)
        {
            var consumeResponse = await _payment.Consume(request.Token);
            return new ConsumeResponseModel
            {
                IsSuccess = consumeResponse.status == Status.Success
            };
        }

        public void Dispose()
        {
            _payment?.Disconnect();
        }
    }
}

#endif