using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TransactionProcessor.Models.Merchant;

[ExcludeFromCodeCoverage]
public class MerchantStatementForDate
{
    #region Fields

    private readonly List<MerchantStatementLine> StatementLines;

    #endregion

    #region Constructors

    public MerchantStatementForDate()
    {
        this.StatementLines = new List<MerchantStatementLine>();
    }

    #endregion

    #region Properties

    public Guid EstateId { get; set; }
    public Boolean IsCreated { get; set; }
    //public Boolean IsGenerated { get; set; }
    //public Boolean HasBeenEmailed { get; set; }
    public Guid MerchantId { get; set; }
    public Guid MerchantStatementId { get; set; }
    public Guid MerchantStatementForDateId { get; set; }
    public DateTime StatementDate { get; set; }
    public DateTime ActivityDate { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Adds the statement line.
    /// </summary>
    /// <param name="statementLine">The statement line.</param>
    public void AddStatementLine(MerchantStatementLine statementLine)
    {
        this.StatementLines.Add(statementLine);
    }

    /// <summary>
    /// Gets the statement lines.
    /// </summary>
    /// <returns></returns>
    public List<MerchantStatementLine> GetStatementLines()
    {
        return this.StatementLines.OrderBy(s => s.DateTime).ToList();
    }

    #endregion
}

[ExcludeFromCodeCoverage]
public class MerchantStatement
{
    #region Fields

    private readonly List<(Guid merchantStatementForDateId, DateTime activityDate)> StatementActivityDates;

    #endregion

    #region Constructors

    public MerchantStatement()
    {
        this.StatementActivityDates = new();
    }

    #endregion

    #region Properties

    public Guid EstateId { get; set; }
    public Boolean IsCreated { get; set; }
    public Boolean IsGenerated { get; set; }
    public Boolean HasBeenEmailed { get; set; }
    public Guid MerchantId { get; set; }
    public Guid MerchantStatementId { get; set; }
    public DateTime StatementDate { get; set; }
    #endregion

    #region Methods

    /// <summary>
    /// Adds the statement line.
    /// </summary>
    /// <param name="statementLine">The statement line.</param>
    public void AddStatementActivityDate(Guid merchantStatementForDateId, DateTime activityDate)
    {
        this.StatementActivityDates.Add((merchantStatementForDateId, activityDate));
    }

    public List<(Guid merchantStatementForDateId, DateTime activityDate)> GetActivityDates()
    {
        return this.StatementActivityDates.OrderBy(s => s.activityDate).ToList();
    }

    #endregion
}