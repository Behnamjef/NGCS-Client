using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NGCS.Network;
using NGCS.MultiBuild;
using NetworkAdapter.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.Utilities;
using UnityEngine;
using Zenject;

namespace NGCS.IAP
{
    public class PurchaseManager : MonoBehaviour, IPurchaseManager
    {
        private IPurchaseHandler _currentPurchaseHandler;
        private MarketType _marketType;
        private string _storeKey;

        private bool _isInitialized;
        private ClientApiService _clientApiService;

        [Inject]
        private void Init(IMultiBuildConfig multiBuildConfig, NetworkInstaller networkInstaller)
        {
            _storeKey = multiBuildConfig.GetCurrentConfig().StoreKey;
            _marketType = multiBuildConfig.GetCurrentMarketType();
            _clientApiService = networkInstaller.ClientApiService;
            Initialize();
        }

        private async Task<bool> Initialize()
        {
            try
            {
                
                Debug.Log($">>>>> Purchase Init starts: isInitialized {_isInitialized}");
                if (_isInitialized)
                    return true;

                if (_currentPurchaseHandler == null)
                {
#if ZarinPalPayment || UNITY_EDITOR
                    Debug.Log($">>>>> Purchase Init: Zarinpal");
                    _currentPurchaseHandler = gameObject.AddComponent<ZarinPalPurchaseHandler>();
#elif Bazaar
                    Debug.Log($">>>>> Purchase Init: Bazaar");
                    _currentPurchaseHandler = new BazaarPurchaseHandler();
#elif Myket
                    Debug.Log($">>>>> Purchase Init: Myket");
                    _currentPurchaseHandler = new MyketPurchaseHandler();
#endif
                }

                _isInitialized = await _currentPurchaseHandler?.Init(_storeKey, _clientApiService)!;
                
                Debug.Log($">>>>> Purchase Init finished: isInitialized {_isInitialized}");
                return _isInitialized;
            }
            catch (Exception e)
            {
                Debug.Log($">>>>> Purchase failed - Init: {e.Message}");

                return false;
            }
        }

        public async Task<List<PurchaseFinishModel>> ConsumePendingPurchases(GetPendingPurchasesRequestModel request,
            bool isForce)
        {
            try
            {
                Debug.Log($">>>>> Purchase consume pending starts: request {JsonConvert.SerializeObject(request)}");
                
                if (!await Initialize())
                    throw new Exception("Not Initialized");

                var itemsToConsume = await GetPendingPurchases(request, isForce);
                var consumedItems = new List<PurchaseFinishModel>();
                Debug.Log($">>>>> Purchase consume pending items: {JsonConvert.SerializeObject(itemsToConsume)}");
                foreach (var item in itemsToConsume)
                {
                    var verifyResponse = await Verify(new VerifyRequestModel
                    {
                        UserId = request.UserId,
                        LoginToken = request.LoginToken,
                        PackageId = request.CoinPackages.FirstOrDefault(p => p.sku == item.Sku)!.id,
                        PaymentId = item.PaymentId,
                        Sku = item.Sku,
                        Token = item.Token,
                        Meta = item.Meta,
                    });

                    if (!verifyResponse.IsSuccess)
                        throw new Exception("Buy item was not successful");


                    var consumeResponse = await Consume(new ConsumeRequestModel
                    {
                        UserId = request.UserId,
                        Token = item.Token,
                        Sku = item.Sku
                        
                    });

                    if (!consumeResponse.IsSuccess)
                        throw new Exception("Consume item was not successful");

                    consumedItems.Add(new PurchaseFinishModel
                    {
                        IsSuccess = true,
                        CoinAfter = verifyResponse.CoinsAfter
                    });
                }
                Debug.Log($">>>>> Purchase consume pending finished. consumed items: {JsonConvert.SerializeObject(consumedItems)}");

                return consumedItems;
            }
            catch (Exception e)
            {
                Debug.Log($">>>>> Purchase failed - ConsumePendingPurchases: {e.Message}");

                return new List<PurchaseFinishModel>();
            }
        }

        public async Task<bool> HasPendingPurchases(GetPendingPurchasesRequestModel request, bool isForce)
        {
            try
            {
                if (!await Initialize())
                    throw new Exception("Not Initialized");

                return !(await GetPendingPurchases(request, isForce)).IsNullOrEmpty();
            }
            catch (Exception e)
            {
                Debug.Log($">>>>> Purchase failed - HasPendingPurchases: {e.Message}");
                return false;
            }
        }

        private async Task<List<BuyResponseModel>> GetPendingPurchases(GetPendingPurchasesRequestModel request,
            bool isForce)
        {
            try
            {
                if (!await Initialize())
                    throw new Exception("Not Initialized");

                return await _currentPurchaseHandler.GetPendingPurchases(request, isForce);
            }
            catch (Exception e)
            {
                Debug.Log($">>>>> Purchase failed - GetPendingPurchases: {e.Message}");
                return new List<BuyResponseModel>();
            }
        }

        public async Task<PurchaseFinishModel> Buy(BuyRequestModel buyRequest)
        {
            try
            {
                Debug.Log($">>>>> Purchase buy starts.: {JsonConvert.SerializeObject(buyRequest)}");

                if (!await Initialize())
                    throw new Exception("Not Initialized");

                var buyResponse = await _currentPurchaseHandler.Buy(buyRequest);
                if (!buyResponse.IsSuccess)
                    throw new Exception("Buy item was not successful");

                var verifyResponse = await Verify(new VerifyRequestModel
                {
                    UserId = buyRequest.UserId,
                    LoginToken = buyRequest.LoginToken,
                    PackageId = buyRequest.PackageId,
                    PaymentId = buyResponse.PaymentId,
                    Sku = buyRequest.Sku,
                    Token = buyResponse.Token,
                    Meta = buyResponse.Meta,
                });

                if (!verifyResponse.IsSuccess)
                    throw new Exception("Buy item was not successful");


                var consumeResponse = await Consume(new ConsumeRequestModel
                {
                    UserId = buyRequest.UserId,
                    Token = buyResponse.Token,
                    Sku = buyRequest.Sku
                });

                if (!consumeResponse.IsSuccess)
                    throw new Exception("Consume item was not successful");
                
                Debug.Log($">>>>> Purchase buy finished.: {JsonConvert.SerializeObject(consumeResponse)}");

                return new PurchaseFinishModel
                {
                    IsSuccess = true,
                    CoinAfter = verifyResponse.CoinsAfter
                };
            }
            catch (Exception e)
            {
                Debug.Log($">>>>> Purchase failed - Buy: {e.Message}");
                return new PurchaseFinishModel { IsSuccess = false };
            }
        }

        private async Task<VerifyResponseModel> Verify(VerifyRequestModel verifyRequest)
        {
            try
            {
                Debug.Log($">>>>> Purchase verify starts.: {JsonConvert.SerializeObject(verifyRequest)}");
                
                if (!await Initialize())
                    throw new Exception("Not Initialized");

                var bundleName = Application.identifier;
                var marketType = GetMarketType();

                var receipt = new PurchaseReceipt()
                {
                    market = marketType.ToString().ToLower(),
                    userId = verifyRequest.UserId,
                    appPackage = bundleName,
                    sku = verifyRequest.Sku,
                    token = verifyRequest.Token,
                    metaData = verifyRequest.Meta
                };
                verifyRequest.Receipt = receipt;

                var result = await _currentPurchaseHandler.Verify(verifyRequest);
                if (!result.IsSuccess)
                    throw new Exception("Verify item was not successful");

                Debug.Log($">>>>> Purchase verify finished. result: {JsonConvert.SerializeObject(result)}");
                return result;
            }
            catch (Exception e)
            {
                Debug.Log($">>>>> Purchase failed - Verify: {e.Message}");
                return new VerifyResponseModel
                {
                    IsSuccess = false
                };
            }
        }

        private async Task<ConsumeResponseModel> Consume(ConsumeRequestModel product)
        {
            try
            {
                Debug.Log($">>>>> Purchase Consume starts.: {JsonConvert.SerializeObject(product)}");
                if (!await Initialize())
                    throw new Exception("Not Initialized");

                Debug.Log($">>>>> consuming product: {JsonConvert.SerializeObject(product)}");

                var response = await _currentPurchaseHandler.Consume(product);
                if (!response.IsSuccess)
                    throw new Exception("Consume item was not successful");
                
                Debug.Log($">>>>> Purchase Consume finished.: {JsonConvert.SerializeObject(response)}");

                return response;
            }
            catch (Exception e)
            {
                Debug.Log($">>>>> Purchase failed - Consume: {e.Message}");
                return new ConsumeResponseModel
                {
                    IsSuccess = false
                };
            }
        }

        public MarketType GetMarketType()
        {
            return _marketType;
        }

        private void OnApplicationQuit()
        {
            Dispose();
        }

        private void Dispose()
        {
            _currentPurchaseHandler?.Dispose();
        }
    }
}