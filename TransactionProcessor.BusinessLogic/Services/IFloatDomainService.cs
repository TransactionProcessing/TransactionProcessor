using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.Services
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Common;
    using EstateManagement.Client;
    using EstateManagement.Database.Entities;
    using EstateManagement.DataTransferObjects.Responses;
    using FloatAggregate;
    using Models;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;

    public interface IFloatDomainService{
        Task<CreateFloatForContractProductResponse> CreateFloatForContractProduct(Guid estateId,
                                                                                  Guid contractId,
                                                                                  Guid productId,
                                                                                  DateTime createDateTime, 
                                                                                  CancellationToken cancellationToken);

        Task RecordCreditPurchase(Guid estateId, Guid floatId, Decimal creditAmount, Decimal costPrice, DateTime purchaseDateTime, CancellationToken cancellationToken);
    }

    public class FloatDomainService : IFloatDomainService{
        private readonly IAggregateRepository<FloatAggregate, DomainEvent> FloatAggregateRepository;

        private readonly IEstateClient EstateClient;
        private readonly ISecurityServiceClient SecurityServiceClient;

        public FloatDomainService(IAggregateRepository<FloatAggregate, DomainEvent> floatAggregateRepository,
                                  IEstateClient estateClient,
                                  ISecurityServiceClient securityServiceClient)
        {
            this.FloatAggregateRepository = floatAggregateRepository;
            this.EstateClient = estateClient;
            this.SecurityServiceClient = securityServiceClient;
        }
        
        private TokenResponse TokenResponse;


        private async Task ValidateEstate(Guid estateId, CancellationToken cancellationToken)
        {
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            EstateResponse estate = await this.EstateClient.GetEstate(this.TokenResponse.AccessToken, estateId, cancellationToken);

            if (estate == null){
                throw new InvalidOperationException($"Estate with Id {estateId} not found");
            }
        }

        private async Task ValidateContractProduct(Guid estateId, Guid contractId, Guid productId, CancellationToken cancellationToken)
        {
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            // TODO: validate the estate, contract and product
            ContractResponse contract = await this.EstateClient.GetContract(this.TokenResponse.AccessToken, estateId, contractId, cancellationToken);

            if (contract == null)
            {
                throw new InvalidOperationException($"Contract with Id {contractId} not found for Estate Id {estateId}");
            }

            Boolean productExists = contract.Products.Any(cp => cp.ProductId == productId);

            if (productExists == false)
            {
                throw new InvalidOperationException($"Contract Product with Id {productId} not found in Contract Id {contractId} for Estate Id {estateId}");
            }
        }

        public async Task<CreateFloatForContractProductResponse> CreateFloatForContractProduct(Guid estateId, Guid contractId, Guid productId, 
                                                                                               DateTime createDateTime, CancellationToken cancellationToken){
            await this.ValidateEstate(estateId, cancellationToken);
            await this.ValidateContractProduct(estateId, contractId, productId, cancellationToken);

            // Generate the float id
            Guid floatId = IdGenerationService.GenerateFloatAggregateId(estateId, contractId, productId);

            FloatAggregate floatAggregate = await this.FloatAggregateRepository.GetLatestVersion(floatId, cancellationToken);

            floatAggregate.CreateFloat(estateId,contractId,productId, createDateTime);

            await this.FloatAggregateRepository.SaveChanges(floatAggregate, cancellationToken);

            return new CreateFloatForContractProductResponse{
                                                                FloatId = floatId
                                                            };
        }

        public async Task RecordCreditPurchase(Guid estateId, Guid floatId, Decimal creditAmount, Decimal costPrice, DateTime purchaseDateTime, CancellationToken cancellationToken){
            FloatAggregate floatAggregate = await this.FloatAggregateRepository.GetLatestVersion(floatId, cancellationToken);

            floatAggregate.RecordCreditPurchase(purchaseDateTime, creditAmount, costPrice);

            await this.FloatAggregateRepository.SaveChanges(floatAggregate, cancellationToken);
        }
    }
}
