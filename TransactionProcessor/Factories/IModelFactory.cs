namespace TransactionProcessor.Factories
{
    using BusinessLogic.Requests;
    using DataTransferObjects;
    using Models;
    using IssueVoucherResponse = DataTransferObjects.IssueVoucherResponse;
    using RedeemVoucherResponse = DataTransferObjects.RedeemVoucherResponse;

    /// <summary>
    /// 
    /// </summary>
    public interface IModelFactory
    {
        #region Methods
        
        SerialisedMessage ConvertFrom(ProcessLogonTransactionResponse processLogonTransactionResponse);

        SerialisedMessage ConvertFrom(ProcessSaleTransactionResponse processSaleTransactionResponse);

        SerialisedMessage ConvertFrom(ProcessReconciliationTransactionResponse processReconciliationTransactionResponse);

        IssueVoucherResponse ConvertFrom(Models.IssueVoucherResponse issueVoucherResponse);

        GetVoucherResponse ConvertFrom(Models.Voucher voucherModel);

        RedeemVoucherResponse ConvertFrom(Models.RedeemVoucherResponse redeemVoucherResponse);

        #endregion
    }
}