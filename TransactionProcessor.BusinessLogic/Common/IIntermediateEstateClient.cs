using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EstateManagement.Client;
using EstateManagement.DataTransferObjects.Requests.Merchant;
using EstateManagement.DataTransferObjects.Responses.Contract;
using EstateManagement.DataTransferObjects.Responses.Estate;
using EstateManagement.DataTransferObjects.Responses.Merchant;
using Shared.EventStore.Aggregate;
using SimpleResults;
using TransactionProcessor.Aggregates;

namespace TransactionProcessor.BusinessLogic.Common
{
    public interface IIntermediateEstateClient
    {
        Task<Result<MerchantResponse>> GetMerchant(
            string accessToken,
            Guid estateId,
            Guid merchantId,
            CancellationToken cancellationToken);

        Task<Result<List<ContractProductTransactionFee>>> GetTransactionFeesForProduct(
            string accessToken,
            Guid estateId,
            Guid merchantId,
            Guid contractId,
            Guid productId,
            CancellationToken cancellationToken);

        Task<Result<ContractResponse>> GetContract(
            string accessToken,
            Guid estateId,
            Guid contractId,
            CancellationToken cancellationToken);

        Task<Result> AddDeviceToMerchant(
            string accessToken,
            Guid estateId,
            Guid merchantId,
            AddMerchantDeviceRequest request,
            CancellationToken cancellationToken);

        Task<Result<List<ContractResponse>>> GetMerchantContracts(
            string accessToken,
            Guid estateId,
            Guid merchantId,
            CancellationToken cancellationToken);

        public IEstateClient EstateClient { get; }
    }

    [ExcludeFromCodeCoverage]
    public class IntermediateEstateClient : IIntermediateEstateClient {
        
        public IntermediateEstateClient(IEstateClient estateClient) {
            this.EstateClient = estateClient;
        }

        public async Task<Result<MerchantResponse>> GetMerchant(String accessToken,
                                                                Guid estateId,
                                                                Guid merchantId,
                                                                CancellationToken cancellationToken) {
            return await this.EstateClient.GetMerchant(accessToken, estateId, merchantId, cancellationToken);
        }

        public async Task<Result<List<ContractProductTransactionFee>>> GetTransactionFeesForProduct(String accessToken,
                                                                                                    Guid estateId,
                                                                                                    Guid merchantId,
                                                                                                    Guid contractId,
                                                                                                    Guid productId,
                                                                                                    CancellationToken cancellationToken) {
            return await this.EstateClient.GetTransactionFeesForProduct(accessToken, estateId, merchantId, contractId, productId, cancellationToken);
        }

        public async Task<Result<ContractResponse>> GetContract(String accessToken,
                                                                Guid estateId,
                                                                Guid contractId,
                                                                CancellationToken cancellationToken) {
            return await this.EstateClient.GetContract(accessToken, estateId, contractId, cancellationToken);
        }

        public async Task<Result> AddDeviceToMerchant(String accessToken,
                                                      Guid estateId,
                                                      Guid merchantId,
                                                      AddMerchantDeviceRequest request,
                                                      CancellationToken cancellationToken) {
            return await this.EstateClient.AddDeviceToMerchant(accessToken, estateId, merchantId, request, cancellationToken);
        }

        public async Task<Result<List<ContractResponse>>> GetMerchantContracts(String accessToken,
                                                                               Guid estateId,
                                                                               Guid merchantId,
                                                                               CancellationToken cancellationToken) {
            return await this.EstateClient.GetMerchantContracts(accessToken, estateId, merchantId, cancellationToken);
        }

        public IEstateClient EstateClient { get; }
    }
}
