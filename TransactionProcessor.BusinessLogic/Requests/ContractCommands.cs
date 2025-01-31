using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;
using TransactionProcessor.DataTransferObjects.Requests.Contract;

namespace TransactionProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record ContractCommands {
    public record CreateContractCommand(Guid EstateId,
                                        Guid ContractId,
                                        CreateContractRequest RequestDTO) : IRequest<Result>;

    public record AddProductToContractCommand(Guid EstateId,
                                              Guid ContractId,
                                              Guid ProductId,
                                              AddProductToContractRequest
                                                  RequestDTO) : IRequest<Result>;

    public record AddTransactionFeeForProductToContractCommand(Guid EstateId,
                                                               Guid ContractId,
                                                               Guid ProductId,
                                                               Guid TransactionFeeId,
                                                               AddTransactionFeeForProductToContractRequest
                                                                   RequestDTO) : IRequest<Result>;

    public record DisableTransactionFeeForProductCommand(Guid EstateId,
                                                         Guid ContractId,
                                                         Guid ProductId,
                                                         Guid TransactionFeeId) : IRequest<Result>;
}