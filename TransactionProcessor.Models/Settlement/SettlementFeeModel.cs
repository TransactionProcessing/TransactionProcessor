using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Models.Settlement;

[ExcludeFromCodeCoverage]
public class SettlementFeeModel
{
    #region Properties

    public Decimal CalculatedValue { get; set; }

    public String FeeDescription { get; set; }

    public Boolean IsSettled { get; set; }

    public Guid MerchantId { get; set; }

    public String MerchantName { get; set; }

    public DateTime SettlementDate { get; set; }

    public Guid SettlementId { get; set; }

    public Guid TransactionId { get; set; }

    public String OperatorIdentifier { get; set; }

    #endregion
}