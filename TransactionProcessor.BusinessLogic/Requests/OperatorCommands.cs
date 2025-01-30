using MediatR;
using SimpleResults;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.DataTransferObjects.Requests.Operator;

namespace TransactionProcessor.BusinessLogic.Requests
{
    [ExcludeFromCodeCoverage]
    public class OperatorCommands
    {
        public record CreateOperatorCommand(Guid EstateId, CreateOperatorRequest RequestDto) : IRequest<Result>;

        public record UpdateOperatorCommand(Guid EstateId, Guid OperatorId, UpdateOperatorRequest RequestDto) : IRequest<Result>;
    }
}