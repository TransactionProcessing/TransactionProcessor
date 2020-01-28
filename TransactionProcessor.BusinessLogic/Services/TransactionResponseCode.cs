namespace TransactionProcessor.BusinessLogic.Services
{
    public enum TransactionResponseCode
    {
        Success = 0,

        InvalidDeviceIdentifier = 1000,
        InvalidEstateId = 1001,
        InvalidMerchantId = 1002
    }
}