using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Models.Settlement;

[ExcludeFromCodeCoverage]
public class SettlementModel
{
    #region Constructors

    public SettlementModel()
    {
        this.SettlementFees = new List<SettlementFeeModel>();
    }

    #endregion

    #region Properties

    public Boolean IsCompleted { get; set; }

    public Int32 NumberOfFeesSettled { get; set; }

    public DateTime SettlementDate { get; set; }

    public List<SettlementFeeModel> SettlementFees { get; set; }

    public Guid SettlementId { get; set; }

    public Decimal ValueOfFeesSettled { get; set; }

    #endregion
}