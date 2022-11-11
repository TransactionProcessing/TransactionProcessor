namespace TransactionProcessor.BusinessLogic.Requests;

using System;
using MediatR;
using Models;

public class RedeemVoucherRequest : IRequest<RedeemVoucherResponse>
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RedeemVoucherRequest"/> class.
    /// </summary>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="voucherCode">The voucher code.</param>
    /// <param name="redeemedDateTime">The redeemed date time.</param>
    private RedeemVoucherRequest(Guid estateId,
                                 String voucherCode,
                                 DateTime redeemedDateTime)
    {
        this.EstateId = estateId;
        this.VoucherCode = voucherCode;
        this.RedeemedDateTime = redeemedDateTime;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the estate identifier.
    /// </summary>
    /// <value>
    /// The estate identifier.
    /// </value>
    public Guid EstateId { get; }

    /// <summary>
    /// Gets the redeemed date time.
    /// </summary>
    /// <value>
    /// The redeemed date time.
    /// </value>
    public DateTime RedeemedDateTime { get; }

    /// <summary>
    /// Gets or sets the voucher code.
    /// </summary>
    /// <value>
    /// The voucher code.
    /// </value>
    public String VoucherCode { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Creates the specified estate identifier.
    /// </summary>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="voucherCode">The voucher code.</param>
    /// <param name="redeemedDateTime">The redeemed date time.</param>
    /// <returns></returns>
    public static RedeemVoucherRequest Create(Guid estateId,
                                              String voucherCode,
                                              DateTime redeemedDateTime)
    {
        return new RedeemVoucherRequest(estateId, voucherCode, redeemedDateTime);
    }

    #endregion
}