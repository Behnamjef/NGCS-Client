using System.Collections.Generic;
using NetworkAdapter.Entities;
using Newtonsoft.Json.Linq;

namespace NGCS.IAP
{
    public struct GetPendingPurchasesRequestModel
    {
        public string UserId;
        public string LoginToken;
        public List<PackageDto> CoinPackages;
    }
    
    public struct BuyRequestModel
    {
        public string Sku;
        public string UserId;
        public string LoginToken;
        public int PackageId;
    }

    public struct BuyResponseModel
    {
        public bool IsSuccess;
        public string Sku;
        public string Token;
        public string PaymentId;
        public JObject Meta;
    }

    public struct VerifyRequestModel
    {
        public string UserId { get; set; }
        public string LoginToken;
        public string PaymentId;
        public int PackageId { get; set; }
        public PurchaseReceipt Receipt { get; set; }

        public string Sku;
        public string Token;
        public JObject Meta;
    }

    public struct VerifyResponseModel
    {
        public bool IsSuccess { get; set; }
        public int CoinsBefore { get; set; }
        public int CoinsAfter { get; set; }
    }

    public struct ConsumeRequestModel
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public string Sku { get; set; }
    }

    public struct ConsumeResponseModel
    {
        public bool IsSuccess { get; set; }
    }


    public struct PurchaseFinishModel
    {
        public bool IsSuccess;
        public int CoinAfter;
    }
}