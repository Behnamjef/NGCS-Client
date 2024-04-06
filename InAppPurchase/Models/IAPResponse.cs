using Newtonsoft.Json.Linq;

namespace NGCS.IAP
{
    public class IAPResponse
    {
        public string Sku;
        public string Token;
        public bool Status;
        public string Message;
        public JObject Meta;

        public IAPResponse(string sku, string token, bool status, string message, JObject meta)
        {
            Sku = sku;
            Token = token;
            Status = status;
            Message = message;
            Meta = meta;
        }
    }
}