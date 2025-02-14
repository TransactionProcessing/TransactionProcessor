using System.Threading.Tasks;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.RequestHandlers
{
    using System.Threading;
    using MediatR;
    using Requests;
    using TransactionProcessor.BusinessLogic.Services;

    public class FloatRequestHandler :IRequestHandler<FloatCommands.CreateFloatForContractProductCommand, Result>,
                                      IRequestHandler<FloatCommands.RecordCreditPurchaseForFloatCommand, Result>,
    IRequestHandler<FloatActivityCommands.RecordCreditPurchaseCommand, Result>,
    IRequestHandler<FloatActivityCommands.RecordTransactionCommand, Result>
    {
        private readonly IFloatDomainService FloatDomainService;

        public FloatRequestHandler(IFloatDomainService floatDomainService){
            this.FloatDomainService = floatDomainService;
        }

        

        public async Task<Result> Handle(FloatCommands.CreateFloatForContractProductCommand command, CancellationToken cancellationToken){
            return await this.FloatDomainService.CreateFloatForContractProduct(command, cancellationToken);
        }

        public async Task<Result> Handle(FloatCommands.RecordCreditPurchaseForFloatCommand command, CancellationToken cancellationToken){
            return await this.FloatDomainService.RecordCreditPurchase(command, cancellationToken);
        }

        public async Task<Result> Handle(FloatActivityCommands.RecordCreditPurchaseCommand command,
                                         CancellationToken cancellationToken) {
            return await this.FloatDomainService.RecordCreditPurchase(command, cancellationToken);
        }

        public async Task<Result> Handle(FloatActivityCommands.RecordTransactionCommand request,
                                         CancellationToken cancellationToken) {
            return await this.FloatDomainService.RecordTransaction(request, cancellationToken);
        }
    }
}
