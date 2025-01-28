using EstateManagement.DataTransferObjects.Requests.Estate;
using MediatR;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
