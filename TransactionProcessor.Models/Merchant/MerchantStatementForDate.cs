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
    public Guid MerchantId { get; set; }
    public Guid MerchantStatementId { get; set; }
    public Guid MerchantStatementForDateId { get; set; }
    public DateTimeOffset StatementDate { get; set; }
    public DateTimeOffset ActivityDate { get; set; }

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

    private readonly List<(Guid merchantStatementForDateId, DateTimeOffset activityDate)> StatementActivityDates;

    #endregion

    #region Constructors

    public MerchantStatement()
    {
        this.StatementActivityDates = new();
        this.StatementLines = new Dictionary<Int32, MerchantStatementLine>();
    }

    #endregion

    #region Properties

    public Guid EstateId { get; set; }
    public Boolean IsCreated { get; set; }
    public Boolean IsGenerated { get; set; }
    public Boolean IsBuilt { get; set; }
    public Boolean HasBeenEmailed { get; set; }
    public Guid MerchantId { get; set; }
    public Guid MerchantStatementId { get; set; }
    public DateTimeOffset StatementDate { get; set; }
    public DateTimeOffset BuiltDateTime { get; set; }
    #endregion

    #region Methods

    /// <summary>
    /// Adds the statement line.
    /// </summary>
    /// <param name="statementLine">The statement line.</param>
    public void AddStatementActivityDate(Guid merchantStatementForDateId, DateTimeOffset activityDate)
    {
        this.StatementActivityDates.Add((merchantStatementForDateId, activityDate));
    }

    public List<(Guid merchantStatementForDateId, DateTimeOffset activityDate)> GetActivityDates()
    {
        return this.StatementActivityDates.OrderBy(s => s.activityDate).ToList();
    }

    public List<(Int32 lineNumber, MerchantStatementLine statementLine)> GetStatementLines() {
        List<(Int32 lineNumber, MerchantStatementLine statementLine)> result = new List<(Int32 lineNumber, MerchantStatementLine statementLine)>();
        foreach (KeyValuePair<Int32, MerchantStatementLine> merchantStatementLine in this.StatementLines) {
            result.Add((merchantStatementLine.Key, merchantStatementLine.Value));    
        }
        return result.OrderBy(s => s.lineNumber).ToList();
    }

    public void AddStatementDailySummary(DateTimeOffset activityDate, Decimal transactionAmount,Decimal feeAmount) {
        // Build statement lines
        var lineNumber = this.StatementLines.Count;
        MerchantStatementLine transactionLine = new() {
            DateTime = activityDate,
            Amount = transactionAmount,
            LineType = 1, // Transactions
            Description = $"Total Transactions Processed"
        };
        this.StatementLines.Add(lineNumber, transactionLine);

        lineNumber++;
        MerchantStatementLine feeLine = new() {
            DateTime = activityDate,
            Amount = feeAmount,
            LineType = 2, // Fees
            Description = $"Total Fees Settleed"
        };
        this.StatementLines.Add(lineNumber,feeLine);
    }


    private readonly Dictionary<Int32, MerchantStatementLine> StatementLines;
    #endregion
}