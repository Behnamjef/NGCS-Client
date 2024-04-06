#if Myket
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NGCS.Network;
using MyketPlugin;
using NetworkAdapter.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NGCS.IAP
{
    public class MyketPurchaseHandler : IPurchaseHandler
    {
        private TaskCompletionSource<(bool, string)> _initTask;
        private TaskCompletionSource<(MyketPurchase, string)> _purchaseTask;
        private TaskCompletionSource<(List<MyketPurchase>, string)> _purchaseQueryTask;
        private TaskCompletionSource<(MyketPurchase, string)> _consumeTask;
        private TaskCompletionSource<(MyketSkuInfo, string)> _skuDetail;
        private ClientApiService _clientApiService;

        public MyketPurchaseHandler()
        {
            _purchaseTask = new TaskCompletionSource<(MyketPurchase, string)>();
            _initTask = new TaskCompletionSource<(bool, string)>();
            _consumeTask = new TaskCompletionSource<(MyketPurchase, string)>();
            _purchaseQueryTask = new TaskCompletionSource<(List<MyketPurchase>, string)>();
            _skuDetail = new TaskCompletionSource<(MyketSkuInfo, string)>();

            //Init
            IABEventManager.billingNotSupportedEvent += (message) =>
            {
                Debug.Log($"Billing Not Supported {message}");
                _initTask.TrySetResult((false, message));
            };
            IABEventManager.billingSupportedEvent += () =>
            {
                Debug.Log("Billing Supported!");
                _initTask.TrySetResult((true, "Success"));
            };

            //Inventory query
            IABEventManager.queryPurchasesSucceededEvent += (list) =>
            {
                _purchaseQueryTask.TrySetResult((list, "Success"));
            };
            IABEventManager.queryPurchasesFailedEvent += (message) =>
            {
                _purchaseQueryTask.TrySetResult((null, message));
            };

            //Purchase
            IABEventManager.purchaseFailedEvent += (message) =>
            {
                Debug.Log($"Purchase failed {message}");
                _purchaseTask.TrySetResult((null, message));
            };
            IABEventManager.purchaseSucceededEvent += (result) =>
            {
                Debug.Log("Purchase Successful!");
                _purchaseTask.TrySetResult((result, "Success"));
            };

            //Consume
            IABEventManager.consumePurchaseFailedEvent += message =>
            {
                Debug.Log($"Consume failed {message}");
                _consumeTask.TrySetResult((null, message));
            };
            IABEventManager.consumePurchaseSucceededEvent += purchase =>
            {
                Debug.Log("Consume Success!");
                _consumeTask.TrySetResult((purchase, "Success"));
            };

            //SKU Detail
            IABEventManager.querySkuDetailsFailedEvent += message =>
            {
                Debug.Log($"Consume failed {message}");
                _skuDetail.TrySetResult((null, message));
            };
            IABEventManager.querySkuDetailsSucceededEvent += purchase =>
            {
                Debug.Log("Consume Success!");
                _skuDetail.TrySetResult((purchase.FirstOrDefault(), "Success"));
            };
        }

        public async Task<bool> Init(string key,ClientApiService clientApiService)
        {
            _clientApiService = clientApiService;
            MyketIAB.init(key);
            var result = await _initTask.Task;
            _initTask = new TaskCompletionSource<(bool, string)>();

            return result.Item1;
        }

        public async Task<List<BuyResponseModel>> GetPendingPurchases(GetPendingPurchasesRequestModel request, bool isForce)
        {
            MyketIAB.queryPurchases();
            var queryResult = await _purchaseQueryTask.Task;
            _purchaseQueryTask = new TaskCompletionSource<(List<MyketPurchase>, string)>();

            if (queryResult.Item1 != null)
            {
                var notConsumedItems = queryResult.Item1
                    .Select(pi => (new BuyResponseModel
                    {
                        IsSuccess = true,
                        Sku = pi.ProductId,
                        Token = pi.PurchaseToken, 
                        Meta = JObject.FromObject(pi)
                    })).ToList();
                
                Debug.LogWarning($"Myket -  Not consumed items count {notConsumedItems.Count}");
                return notConsumedItems;
            }
            else
            {
                Debug.LogWarning($"Myket -  Couldn't query old purchases {queryResult.Item2}");
                return null;
            }
        }

        public async Task<BuyResponseModel> Buy(BuyRequestModel product)
        {
            MyketIAB.purchaseProduct(product.Sku, $"UserId: {product.UserId}");
            var result = await _purchaseTask.Task;
            _purchaseTask = new TaskCompletionSource<(MyketPurchase, string)>();

            if (result.Item1 == null) //Purchase failed
                return new BuyResponseModel{IsSuccess = false};

            //Purchase Success
            return new BuyResponseModel
            {
                IsSuccess = true,
                Token = result.Item1.PurchaseToken, 
                Meta = JObject.FromObject(result)
            };;
        }
        
        public async Task<VerifyResponseModel> Verify(VerifyRequestModel request)
        {
            var iapRequest = new IAPBuyRequest
            {
                UserId = request.UserId,
                IAPGatewayType = IAPGatewayType.myket,
                PackageId = request.PackageId,
                Receipt = request.Receipt
            };
            var result = await _clientApiService.PurchaseVerifyAsync(iapRequest);

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
            MyketIAB.consumeProduct(request.Sku);
            var consumeResult = await _consumeTask.Task;
            _consumeTask = new TaskCompletionSource<(MyketPurchase, string)>();

            if (consumeResult.Item1 == null) //Consume failed
                return new ConsumeResponseModel
                {
                    IsSuccess = false
                };

            //Consume success
            return new ConsumeResponseModel
            {
                IsSuccess = true
            };
        }

        public void Dispose()
        {
            MyketIAB.unbindService();
        }
    }
}
#endif