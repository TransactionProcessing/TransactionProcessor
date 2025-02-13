using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TransactionProcessor.Models.Merchant;

[ExcludeFromCodeCoverage]
public class MerchantStatement
{
    #region Fields

    /// <summary>
    /// The statement lines
    /// </summary>
    private readonly List<MerchantStatementLine> StatementLines;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MerchantStatement"/> class.
    /// </summary>
    public MerchantStatement()
    {
        this.StatementLines = new List<MerchantStatementLine>();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the estate identifier.
    /// </summary>
    /// <value>
    /// The estate identifier.
    /// </value>
    public Guid EstateId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is created.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is created; otherwise, <c>false</c>.
    /// </value>
    public Boolean IsCreated { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is generated.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is generated; otherwise, <c>false</c>.
    /// </value>
    public Boolean IsGenerated { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is emailed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is emailed; otherwise, <c>false</c>.
    /// </value>
    public Boolean HasBeenEmailed { get; set; }

    /// <summary>
    /// Gets or sets the merchant identifier.
    /// </summary>
    /// <value>
    /// The merchant identifier.
    /// </value>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// Gets or sets the merchant statement identifier.
    /// </summary>
    /// <value>
    /// The merchant statement identifier.
    /// </value>
    public Guid MerchantStatementId { get; set; }

    /// <summary>
    /// Gets or sets the statement created date time.
    /// </summary>
    /// <value>
    /// The statement created date time.
    /// </value>
    public DateTime StatementCreatedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the statement generated date time.
    /// </summary>
    /// <value>
    /// The statement generated date time.
    /// </value>
    public DateTime StatementGeneratedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the statement number.
    /// </summary>
    /// <value>
    /// The statement number.
    /// </value>
    public Int32 StatementNumber { get; set; } // TODO: How is this allocated??

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
        //return this.StatementLines.OrderBy(s => s.DateTime).ToList();
        return this.StatementLines.OrderBy(s => s.DateTime).ToList();
    }

    #endregion
}