namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    /// <summary>
    /// 
    /// </summary>
    public interface ISettlementDomainService
    {
        Task<ProcessSettlementResponse> ProcessSettlement(DateTime pendingSettlementDate,
                                                          Guid estateId,
                                                          Guid merchantId,
                                                          CancellationToken cancellationToken);
    }
}