using System.Collections.Generic;
using System.Linq;
using TransactionProcessor.DataTransferObjects.Requests.Contract;
using TransactionProcessor.DataTransferObjects.Responses.Contract;

namespace TransactionProcessor.IntegrationTesting.Helpers;

public static class TransactionFeeMatcher
{
    public static ContractProductTransactionFee? Find(
        IEnumerable<ContractProductTransactionFee> fees,
        AddTransactionFeeForProductToContractRequest request)
    {
        return fees.SingleOrDefault(fee => fee.Description == request.Description &&
                                          fee.CalculationType == request.CalculationType &&
                                          fee.FeeType == request.FeeType &&
                                          fee.Value == request.Value);
    }
}
