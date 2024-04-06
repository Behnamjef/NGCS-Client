using System.Collections.Generic;
using System.Threading.Tasks;

namespace NGCS.IAP
{
    public interface IPurchaseManager
    {
        public Task<List<PurchaseFinishModel>> ConsumePendingPurchases(GetPendingPurchasesRequestModel request,
            bool isForce);

        public Task<bool> HasPendingPurchases(GetPendingPurchasesRequestModel request, bool isForce);
        public Task<PurchaseFinishModel> Buy(BuyRequestModel request);
    }
}