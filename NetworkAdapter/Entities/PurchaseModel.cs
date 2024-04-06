using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Entities
{


    public class CodeConsumedResult
    {
        public bool IsSuccess { get; set; }
        public int BeforCoin { get; set; }
        public int AfterCoin { get; set; }
        public string Message { get; set; }
        public List<int> OwnedItems { get; set; }
    }

    public class ChangeEmailOrPhoneResult
    {
        public bool IsSuccess { get; set; }
        public int BeforCoin { get; set; }
        public int AfterCoin { get; set; }
        public string Message { get; set; }
    }


    public enum IAPGatewayType
    {
        bazaar = 0,
        myket = 1,
        sibApp = 2,
        googleplay = 3,
        gift = 4,
    }


    public class PurchaseResult
    {
        public bool IsSuccess { get; set; }
        public bool IsConsume { get; set; }
        public bool IsVerify { get; set; }
        public string Message { get; set; }
        public int CoinsBefor { get; set; }
        public int CoinsAfter { get; set; }
        public List<int> OwnedItems { get; set; }
    }

    public class PurchaseReceipt
    {
        public string market;
        public string userId;
        public string appPackage; // => bundle name
        public string sku;
        public string token;
        public JToken metaData;
        public int toman;
        public int cents;
    }

    public class IAPBuyRequest
    {
        public string UserId { get; set; }
        public int PackageId { get; set; }
        public IAPGatewayType IAPGatewayType { get; set; }
        public PurchaseReceipt Receipt { get; set; }
    }

}
