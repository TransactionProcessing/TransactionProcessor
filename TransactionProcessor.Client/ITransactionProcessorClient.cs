using SimpleResults;
using TransactionProcessor.DataTransferObjects.Requests.Contract;
using TransactionProcessor.DataTransferObjects.Requests.Estate;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.DataTransferObjects.Requests.Operator;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.DataTransferObjects.Responses.Estate;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;
using TransactionProcessor.DataTransferObjects.Responses.Operator;
using AssignOperatorRequest = TransactionProcessor.DataTransferObjects.Requests.Estate.AssignOperatorRequest;

namespace TransactionProcessor.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;

    public interface ITransactionProcessorClient
    {
        #region Methods
        Task<Result<SerialisedMessage>> PerformTransaction(String accessToken, 
                                                           SerialisedMessage transactionRequest,
                                                           CancellationToken cancellationToken);

        Task<Result<SettlementResponse>> GetSettlementByDate(String accessToken, 
                                                                   DateTime settlementDate,
                                                                   Guid estateId,
                                                                   Guid merchantId,
                                                                   CancellationToken cancellationToken);

        Task<Result> ProcessSettlement(String accessToken,
                               DateTime settlementDate,
                               Guid estateId,
                               Guid merchantId,
                               CancellationToken cancellationToken);

        Task<Result> ResendEmailReceipt(String accessToken,
                                        Guid estateId,
                                        Guid transactionId,
                                        CancellationToken cancellationToken);

        Task<Result<MerchantBalanceResponse>> GetMerchantBalance(String accessToken,
                                                         Guid estateId,
                                                         Guid merchantId,
                                                         CancellationToken cancellationToken,
                                                         Boolean liveBalance = true);

        Task<Result<List<MerchantBalanceChangedEntryResponse>>> GetMerchantBalanceHistory(String accessToken,
                                                                                  Guid estateId,
                                                                                  Guid merchantId,
                                                                                  DateTime startDate,
                                                                                  DateTime endDate,
                                                                                  CancellationToken cancellationToken);

        Task<Result<GetVoucherResponse>> GetVoucherByCode(String accessToken,
                                            Guid estateId,
                                            String voucherCode,
                                            CancellationToken cancellationToken);

        Task<Result<GetVoucherResponse>> GetVoucherByTransactionId(String accessToken,
                                                  Guid estateId,
                                                  Guid transactionId,
                                                  CancellationToken cancellationToken);

        Task<Result<RedeemVoucherResponse>> RedeemVoucher(String accessToken,
                                                  RedeemVoucherRequest redeemVoucherRequest,
                                                  CancellationToken cancellationToken);

        Task<Result> CreateFloatForContractProduct(String accessToken,
                                                                                  Guid estateId,
                         CreateFloatForContractProductRequest createFloatForContractProductRequest,
                         CancellationToken cancellationToken);

        Task<Result> RecordFloatCreditPurchase(String accessToken,
                                       Guid estateId,
                                       RecordFloatCreditPurchaseRequest recordFloatCreditPurchaseRequest,
                                       CancellationToken cancellationToken);

        Task<Result<EstateResponse>> GetEstate(String accessToken,
                                               Guid estateId,
                                               CancellationToken cancellationToken);

        Task<Result<List<EstateResponse>>> GetEstates(String accessToken,
                                                      Guid estateId,
                                                      CancellationToken cancellationToken);

        Task<Result> CreateEstate(String accessToken,
                                  CreateEstateRequest createEstateRequest,
                                  CancellationToken cancellationToken);

        Task<Result> CreateEstateUser(String accessToken,
                                      Guid estateId,
                                      CreateEstateUserRequest createEstateUserRequest,
                                      CancellationToken cancellationToken);

        Task<Result> CreateMerchant(String accessToken,
                                    Guid estateId,
                                    CreateMerchantRequest createMerchantRequest,
                                    CancellationToken cancellationToken);

        Task<Result<MerchantResponse>> GetMerchant(String accessToken,
                                                   Guid estateId,
                                                   Guid merchantId,
                                                   CancellationToken cancellationToken);

        Task<Result> AssignOperatorToMerchant(String accessToken,
                                              Guid estateId,
                                              Guid merchantId,
                                              DataTransferObjects.Requests.Merchant.AssignOperatorRequest assignOperatorRequest,
                                              CancellationToken cancellationToken);

        Task<Result> CreateOperator(String accessToken,
                                    Guid estateId,
                                    CreateOperatorRequest createOperatorRequest,
                                    CancellationToken cancellationToken);

        Task<Result<List<OperatorResponse>>> GetOperators(String accessToken,
                                                          Guid estateId,
                                                          CancellationToken cancellationToken);

        Task<Result> AssignOperatorToEstate(String accessToken,
                                            Guid estateId,
                                            AssignOperatorRequest assignOperatorRequest,
                                            CancellationToken cancellationToken);

        Task<Result> CreateContract(String accessToken,
                                    Guid estateId,
                                    CreateContractRequest createContractRequest,
                                    CancellationToken cancellationToken);

        Task<Result<List<ContractResponse>>> GetContracts(String accessToken,
                                                          Guid estateId,
                                                          CancellationToken cancellationToken);

        Task<Result> AddProductToContract(String accessToken,
                                          Guid estateId,
                                          Guid contractId,
                                          AddProductToContractRequest addProductToContractRequest,
                                          CancellationToken cancellationToken);

        Task<Result<ContractResponse>> GetContract(String accessToken,
                                                   Guid estateId,
                                                   Guid contractId,
                                                   CancellationToken cancellationToken);

        Task<Result> AddTransactionFeeForProductToContract(String accessToken,
                                                           Guid estateId,
                                                           Guid contractId,
                                                           Guid productId,
                                                           AddTransactionFeeForProductToContractRequest
                                                               addTransactionFeeForProductToContractRequest,
                                                           CancellationToken cancellationToken);

        Task<Result<List<ContractProductTransactionFee>>> GetTransactionFeesForProduct(String accessToken,
                                                                                       Guid estateId,
                                                                                       Guid merchantId,
                                                                                       Guid contractId,
                                                                                       Guid productId,
                                                                                       CancellationToken cancellationToken);

        Task<Result> AddDeviceToMerchant(String accessToken,
                                         Guid estateId,
                                         Guid merchantId,
                                         AddMerchantDeviceRequest request,
                                         CancellationToken cancellationToken);

        Task<Result> AddContractToMerchant(String accessToken,
                                           Guid estateId,
                                           Guid merchantId,
                                           AddMerchantContractRequest request,
                                           CancellationToken cancellationToken);

        Task<Result> MakeMerchantDeposit(String accessToken,
                                         Guid estateId,
                                         Guid merchantId,
                                         MakeMerchantDepositRequest makeMerchantDepositRequest,
                                         CancellationToken cancellationToken);

        Task<Result> CreateMerchantUser(String accessToken,
                                        Guid estateId,
                                        Guid merchantId,
                                        CreateMerchantUserRequest createMerchantUserRequest,
                                        CancellationToken cancellationToken);

        Task<Result> RemoveOperatorFromEstate(String accessToken,
                                              Guid estateId,
                                              Guid operatorId,
                                              CancellationToken cancellationToken);

        Task<Result<OperatorResponse>> GetOperator(String accessToken,
                                                   Guid estateId,
                                                   Guid operatorId,
                                                   CancellationToken cancellationToken);

        Task<Result> UpdateOperator(String accessToken,
                                    Guid estateId,
                                    Guid operatorId,
                                    UpdateOperatorRequest updateOperatorRequest,
                                    CancellationToken cancellationToken);

        Task<Result> UpdateMerchantContact(String accessToken,
                                           Guid estateId,
                                           Guid merchantId,
                                           Guid contactId,
                                           Contact updatedContactRequest,
                                           CancellationToken cancellationToken);

        Task<Result> UpdateMerchantAddress(String accessToken,
                                           Guid estateId,
                                           Guid merchantId,
                                           Guid addressId,
                                           Address updatedAddressRequest,
                                           CancellationToken cancellationToken);

        Task<Result> UpdateMerchant(String accessToken,
                                    Guid estateId,
                                    Guid merchantId,
                                    UpdateMerchantRequest request,
                                    CancellationToken cancellationToken);

        Task<Result> SetMerchantSettlementSchedule(String accessToken,
                                                   Guid estateId,
                                                   Guid merchantId,
                                                   SetSettlementScheduleRequest setSettlementScheduleRequest,
                                                   CancellationToken cancellationToken);

        Task<Result> SwapDeviceForMerchant(String accessToken,
                                           Guid estateId,
                                           Guid merchantId,
                                           String deviceIdentifier,
                                           SwapMerchantDeviceRequest request,
                                           CancellationToken cancellationToken);

        Task<Result> MakeMerchantWithdrawal(String accessToken,
                                            Guid estateId,
                                            Guid merchantId,
                                            MakeMerchantWithdrawalRequest makeMerchantWithdrawalRequest,
                                            CancellationToken cancellationToken);

        Task<Result<DataTransferObjects.Responses.Settlement.SettlementResponse>> GetSettlement(String accessToken,
                                                       Guid estateId,
                                                       Guid? merchantId,
                                                       Guid settlementId,
                                                       CancellationToken cancellationToken);

        Task<Result<List<DataTransferObjects.Responses.Settlement.SettlementResponse>>> GetSettlements(String accessToken,
                                                                                                       Guid estateId,
                                                                                                       Guid? merchantId,
                                                                                                       String startDate,
                                                                                                       String endDate,
                                                                                                       CancellationToken cancellationToken);

        Task<Result<List<MerchantResponse>>> GetMerchants(String accessToken,
                                                          Guid estateId,
                                                          CancellationToken cancellationToken);

        Task<Result<List<ContractResponse>>> GetMerchantContracts(String accessToken,
                                                                  Guid estateId,
                                                                  Guid merchantId,
                                                                  CancellationToken cancellationToken);

        Task<Result> DisableTransactionFeeForProduct(String accessToken,
                                                     Guid estateId,
                                                     Guid contractId,
                                                     Guid productId,
                                                     Guid transactionFeeId,
                                                     CancellationToken cancellationToken);

        Task<Result> RemoveContractFromMerchant(String accessToken,
                                                Guid estateId,
                                                Guid merchantId,
                                                Guid contractId,
                                                CancellationToken cancellationToken);

        Task<Result> RemoveOperatorFromMerchant(String accessToken,
                                                Guid estateId,
                                                Guid merchantId,
                                                Guid operatorId,
                                                CancellationToken cancellationToken);

        Task<Result> AddMerchantContact(String accessToken,
                                        Guid estateId,
                                        Guid merchantId,
                                        Contact newContactRequest,
                                        CancellationToken cancellationToken);

        Task<Result> AddMerchantAddress(String accessToken,
                                        Guid estateId,
                                        Guid merchantId,
                                        Address newAddressRequest,
                                        CancellationToken cancellationToken);
        #endregion
    }
}