using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.RequestHandlers
{
    using System.Threading;
    using MediatR;
    using Models;
    using Requests;
    using TransactionProcessor.BusinessLogic.Services;

    public class FloatRequestHandler :IRequestHandler<CreateFloatForContractProductRequest, CreateFloatForContractProductResponse>,
                                      IRequestHandler<RecordCreditPurchaseForFloatRequest>{
        private readonly IFloatDomainService FloatDomainService;

        public FloatRequestHandler(IFloatDomainService floatDomainService){
            this.FloatDomainService = floatDomainService;
        }

        public async Task<CreateFloatForContractProductResponse> Handle(CreateFloatForContractProductRequest request, CancellationToken cancellationToken){
            CreateFloatForContractProductResponse response = await this.FloatDomainService.CreateFloatForContractProduct(request.EstateId,
                                                                                                                         request.ContractId,
                                                                                                                         request.ProductId,
                                                                                                                         request.CreateDateTime,
                                                                                                                         cancellationToken);

            return response;
        }

        public async Task Handle(RecordCreditPurchaseForFloatRequest request, CancellationToken cancellationToken){
            await this.FloatDomainService.RecordCreditPurchase(request.EstateId, request.FloatId, request.CreditAmount, request.CostPrice, request.PurchaseDateTime, cancellationToken);
        }
    }
}
