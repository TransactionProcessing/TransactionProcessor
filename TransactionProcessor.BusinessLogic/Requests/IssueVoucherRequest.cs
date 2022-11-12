namespace TransactionProcessor.BusinessLogic.Requests;

using System;
using MediatR;
using Models;

public class IssueVoucherRequest : IRequest<IssueVoucherResponse>
{
    /// <summary>
    /// Gets the voucher identifier.
    /// </summary>
    /// <value>
    /// The voucher identifier.
    /// </value>
    public Guid VoucherId { get; }

    /// <summary>
    /// Gets the operator identifier.
    /// </summary>
    /// <value>
    /// The operator identifier.
    /// </value>
    public String OperatorIdentifier { get; }

    /// <summary>
    /// Gets the estate identifier.
    /// </summary>
    /// <value>
    /// The estate identifier.
    /// </value>
    public Guid EstateId { get; }

    /// <summary>
    /// Gets the transaction identifier.
    /// </summary>
    /// <value>
    /// The transaction identifier.
    /// </value>
    public Guid TransactionId { get; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    public Decimal Value { get; }

    /// <summary>
    /// Gets the recipient email.
    /// </summary>
    /// <value>
    /// The recipient email.
    /// </value>
    public String RecipientEmail { get; }

    /// <summary>
    /// Gets the recipient mobile.
    /// </summary>
    /// <value>
    /// The recipient mobile.
    /// </value>
    public String RecipientMobile { get; }

    /// <summary>
    /// Gets or sets the issue date time.
    /// </summary>
    /// <value>
    /// The issue date time.
    /// </value>
    public DateTime IssuedDateTime { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IssueVoucherRequest" /> class.
    /// </summary>
    /// <param name="voucherId">The voucher identifier.</param>
    /// <param name="operatorIdentifier">The operator identifier.</param>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="issuedDateTime">The issued date time.</param>
    /// <param name="value">The value.</param>
    /// <param name="recipientEmail">The recipient email.</param>
    /// <param name="recipientMobile">The recipient mobile.</param>
    private IssueVoucherRequest(Guid voucherId,
                                String operatorIdentifier,
                                Guid estateId,
                                Guid transactionId,
                                DateTime issuedDateTime,
                                Decimal value,
                                String recipientEmail,
                                String recipientMobile)
    {
        this.VoucherId = voucherId;
        this.OperatorIdentifier = operatorIdentifier;
        this.EstateId = estateId;
        this.TransactionId = transactionId;
        this.Value = value;
        this.RecipientEmail = recipientEmail;
        this.RecipientMobile = recipientMobile;
        this.IssuedDateTime = issuedDateTime;
    }

    /// <summary>
    /// Creates the specified operator identifier.
    /// </summary>
    /// <param name="voucherId">The voucher identifier.</param>
    /// <param name="operatorIdentifier">The operator identifier.</param>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="issuedDateTime">The issued date time.</param>
    /// <param name="value">The value.</param>
    /// <param name="recipientEmail">The recipient email.</param>
    /// <param name="recipientMobile">The recipient mobile.</param>
    /// <returns></returns>
    public static IssueVoucherRequest Create(Guid voucherId,
                                             String operatorIdentifier,
                                             Guid estateId,
                                             Guid transactionId,
                                             DateTime issuedDateTime,
                                             Decimal value,
                                             String recipientEmail,
                                             String recipientMobile)
    {
        return new IssueVoucherRequest(voucherId, operatorIdentifier, estateId, transactionId, issuedDateTime, value, recipientEmail, recipientMobile);
    }
}