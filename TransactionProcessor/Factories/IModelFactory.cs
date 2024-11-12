using SimpleResults;

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

        Result<SerialisedMessage> ConvertFrom(ProcessLogonTransactionResponse processLogonTransactionResponse);

        Result<SerialisedMessage> ConvertFrom(ProcessSaleTransactionResponse processSaleTransactionResponse);

        Result<SerialisedMessage> ConvertFrom(ProcessReconciliationTransactionResponse processReconciliationTransactionResponse);

        IssueVoucherResponse ConvertFrom(Models.IssueVoucherResponse issueVoucherResponse);

        Result<GetVoucherResponse> ConvertFrom(Models.Voucher voucherModel);

        Result<RedeemVoucherResponse> ConvertFrom(Models.RedeemVoucherResponse redeemVoucherResponse);

        #endregion
    }
}