namespace TransactionProcessor.BusinessLogic.Requests;

using System;
using MediatR;

public class ResendTransactionReceiptRequest : IRequest<Unit>
{
    #region Constructors

    private ResendTransactionReceiptRequest(Guid transactionId,
                                            Guid estateId)
    {
        this.TransactionId = transactionId;
        this.EstateId = estateId;
    }

    public Guid EstateId { get; }

    public Guid TransactionId { get;  }

    #region Methods

    public static ResendTransactionReceiptRequest Create(Guid transactionId,
                                                         Guid estateId)
    {
        return new ResendTransactionReceiptRequest(transactionId,
                                                   estateId);
    }

    #endregion

    #endregion
}