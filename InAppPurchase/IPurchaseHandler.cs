using System.Collections.Generic;
using System.Threading.Tasks;
using NGCS.Network;

namespace NGCS.IAP
{
    public interface IPurchaseHandler
    {
        public Task<bool> Init(string key, ClientApiService clientApiService);
        public Task<List<BuyResponseModel>> GetPendingPurchases(GetPendingPurchasesRequestModel request, bool isForce);
        public Task<BuyResponseModel> Buy(BuyRequestModel product);
        public Task<VerifyResponseModel> Verify(VerifyRequestModel request);
        public Task<ConsumeResponseModel> Consume(ConsumeRequestModel response);
        public void Dispose();
    }
}