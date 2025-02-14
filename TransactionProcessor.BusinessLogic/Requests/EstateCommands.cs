using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;
using TransactionProcessor.DataTransferObjects.Requests.Estate;

namespace TransactionProcessor.BusinessLogic.Requests
{
    [ExcludeFromCodeCoverage]
    public class EstateCommands
    {
        public record CreateEstateCommand(CreateEstateRequest RequestDto) : IRequest<Result>;

        public record CreateEstateUserCommand(Guid EstateId, CreateEstateUserRequest RequestDto) : IRequest<Result>;

        public record AddOperatorToEstateCommand(Guid EstateId, AssignOperatorRequest RequestDto) : IRequest<Result>;

        public record RemoveOperatorFromEstateCommand(Guid EstateId, Guid OperatorId) : IRequest<Result>;
    }
}
