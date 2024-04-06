namespace NetworkAdapter.Entities
{
    public enum PaymentStatus
    {
        Valid,
        Invalid,
        Pending,
        Consumed
    }
    public enum Gateway
    {
        zarinpal
    }
    public class Payment
    {
        public string paymentId;
        public string paymentUrl;
        public string app;
        public string sku;
        public string token;
        public PaymentStatus state;
    }

    public class GetPendingPaymentsParams
    {
        public string userId { get; set; }
        public string loginToken { get; set; }
        public string app { get; set; }
    }

    public class PaymentRequestParams
    {
        public string userId { get; set; }
        public string loginToken { get; set; }
        public string gateway { get; set; }
        public string appPackage { get; set; }
        public string sku { get; set; }
        public string callbackUrl { get; set; }
    }

    public class VerifyConsumRequestParams
    {
        public string userId { get; set; }
        public string loginToken { get; set; }
        public string paymentId { get; set; }
        public string gateway { get; set; }
        public string appPackage { get; set; }
        public string sku { get; set; }
    }

    public class PaymentVM
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public Payment PaymentInfo { get; set; }
    }

    public class ConsumVM
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int CoinBefor { get; set; }
        public int CoinAfter { get; set; }
        public Payment PaymentInfo { get; set; }
    }


}