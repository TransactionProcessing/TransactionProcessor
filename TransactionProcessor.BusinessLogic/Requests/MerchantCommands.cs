using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;

namespace TransactionProcessor.BusinessLogic.Requests
{
    [ExcludeFromCodeCoverage]
    public class MerchantBalanceCommands {
        public record RecordDepositCommand(Guid EstateId, Guid MerchantId, Guid DepositId, Decimal DepositAmount, DateTime DepositDateTime) : IRequest<Result>;
        public record RecordWithdrawalCommand(Guid EstateId, Guid MerchantId, Guid WithdrawalId, Decimal WithdrawalAmount, DateTime WithdrawalDateTime) : IRequest<Result>;
        public record RecordAuthorisedSaleCommand(Guid EstateId, Guid MerchantId,Guid TransactionId, Decimal TransactionAmount, DateTime TransactionDateTime) : IRequest<Result>;
        public record RecordDeclinedSaleCommand(Guid EstateId, Guid MerchantId, Guid TransactionId, Decimal TransactionAmount, DateTime TransactionDateTime) : IRequest<Result>;
        public record RecordSettledFeeCommand(Guid EstateId, Guid MerchantId, Guid FeeId, Decimal FeeAmount, DateTime FeeDateTime) : IRequest<Result>;
    }

    [ExcludeFromCodeCoverage]
    public class MerchantCommands{

        public record CreateMerchantCommand(Guid EstateId, CreateMerchantRequest RequestDto) : IRequest<Result>;

        public record AssignOperatorToMerchantCommand(Guid EstateId, Guid MerchantId, AssignOperatorRequest RequestDto) : IRequest<Result>;

        public record RemoveOperatorFromMerchantCommand(Guid EstateId, Guid MerchantId, Guid OperatorId) : IRequest<Result>;

        public record AddMerchantDeviceCommand(Guid EstateId, Guid MerchantId, AddMerchantDeviceRequest RequestDto) : IRequest<Result>;

        public record AddMerchantContractCommand(Guid EstateId, Guid MerchantId, AddMerchantContractRequest RequestDto) : IRequest<Result>;

        public record RemoveMerchantContractCommand(Guid EstateId, Guid MerchantId, Guid ContractId) : IRequest<Result>;

        public record CreateMerchantUserCommand(Guid EstateId, Guid MerchantId, CreateMerchantUserRequest RequestDto) : IRequest<Result>;

        public record MakeMerchantDepositCommand(Guid EstateId, Guid MerchantId, MerchantDepositSource DepositSource, MakeMerchantDepositRequest RequestDto) : IRequest<Result>;

        public record MakeMerchantWithdrawalCommand(Guid EstateId, Guid MerchantId, MakeMerchantWithdrawalRequest RequestDto) : IRequest<Result>;

        public record SwapMerchantDeviceCommand(Guid EstateId, Guid MerchantId, String DeviceIdentifier, SwapMerchantDeviceRequest RequestDto): IRequest<Result>;

        public record GenerateMerchantStatementCommand(Guid EstateId, Guid MerchantId, GenerateMerchantStatementRequest RequestDto) : IRequest<Result>;
        
        public record UpdateMerchantCommand(Guid EstateId, Guid MerchantId, UpdateMerchantRequest RequestDto) : IRequest<Result>;

        public record AddMerchantAddressCommand(Guid EstateId, Guid MerchantId,  Address RequestDto) : IRequest<Result>;
        public record UpdateMerchantAddressCommand(Guid EstateId, Guid MerchantId, Guid AddressId, Address RequestDto) : IRequest<Result>;

        public record AddMerchantContactCommand(Guid EstateId, Guid MerchantId, Contact RequestDto) : IRequest<Result>;

        public record UpdateMerchantContactCommand(Guid EstateId, Guid MerchantId, Guid ContactId, Contact RequestDto) : IRequest<Result>;
    }

    
}