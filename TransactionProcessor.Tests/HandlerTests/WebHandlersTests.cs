using MediatR;
using Microsoft.AspNetCore.Http;
using Imposter.Abstractions;
using Shouldly;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects;
using TransactionProcessor.DataTransferObjects.Requests.Contract;
using TransactionProcessor.DataTransferObjects.Requests.Estate;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.DataTransferObjects.Requests.MerchantSchedule;
using TransactionProcessor.DataTransferObjects.Requests.Operator;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.Handlers;
using TransactionProcessor.Models;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Estate;
using TransactionProcessor.Models.MerchantSchedule;
using TransactionProcessor.Models.Operator;
using TransactionProcessor.Models.Settlement;
using TransactionProcessor.ProjectionEngine.Models;
using TransactionProcessor.ProjectionEngine.State;
using Xunit;
using MerchantModel = TransactionProcessor.Models.Merchant.Merchant;
using EstateModel = TransactionProcessor.Models.Estate.Estate;
using ProjectionMerchant = TransactionProcessor.ProjectionEngine.State.Merchant;
using OperatorModel = TransactionProcessor.Models.Operator.Operator;
using RedeemVoucherModel = TransactionProcessor.Models.RedeemVoucherResponse;

namespace TransactionProcessor.Tests.HandlerTests;

public class WebHandlersTests
{
    private static readonly Guid EstateId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid MerchantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ContractId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ProductId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid OperatorId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid TransactionId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid FloatId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid SettlementId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    private static readonly Guid AddressId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    private static readonly Guid ContactId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid VoucherTransactionId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly string VoucherCode = "VOUCHER-1";
    private static readonly DateTime Instant = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    [Theory]
    [MemberData(nameof(ContractCases))]
    public async Task ContractHandlers_AreCovered(HandlerCase testCase)
    {
        IMediatorImposter mediator = new();
        testCase.Setup(mediator);

        IResult result = await testCase.Invoke(mediator.Instance());

        result.ShouldNotBeNull();
        testCase.Verify?.Invoke(mediator);
    }

    [Theory]
    [MemberData(nameof(EstateCases))]
    public async Task EstateHandlers_AreCovered(HandlerCase testCase)
    {
        IMediatorImposter mediator = new();
        testCase.Setup(mediator);

        IResult result = await testCase.Invoke(mediator.Instance());

        result.ShouldNotBeNull();
        testCase.Verify?.Invoke(mediator);
    }

    [Theory]
    [MemberData(nameof(FloatCases))]
    public async Task FloatHandlers_AreCovered(HandlerCase testCase)
    {
        IMediatorImposter mediator = new();
        testCase.Setup(mediator);

        IResult result = await testCase.Invoke(mediator.Instance());

        result.ShouldNotBeNull();
        testCase.Verify?.Invoke(mediator);
    }

    [Theory]
    [MemberData(nameof(MerchantCases))]
    public async Task MerchantHandlers_AreCovered(HandlerCase testCase)
    {
        IMediatorImposter mediator = new();
        testCase.Setup(mediator);

        IResult result = await testCase.Invoke(mediator.Instance());

        result.ShouldNotBeNull();
        testCase.Verify?.Invoke(mediator);
    }

    [Theory]
    [MemberData(nameof(OperatorCases))]
    public async Task OperatorHandlers_AreCovered(HandlerCase testCase)
    {
        IMediatorImposter mediator = new();
        testCase.Setup(mediator);

        IResult result = await testCase.Invoke(mediator.Instance());

        result.ShouldNotBeNull();
        testCase.Verify?.Invoke(mediator);
    }

    [Theory]
    [MemberData(nameof(SettlementCases))]
    public async Task SettlementHandlers_AreCovered(HandlerCase testCase)
    {
        IMediatorImposter mediator = new();
        testCase.Setup(mediator);

        IResult result = await testCase.Invoke(mediator.Instance());

        result.ShouldNotBeNull();
        testCase.Verify?.Invoke(mediator);
    }

    [Theory]
    [MemberData(nameof(VoucherCases))]
    public async Task VoucherHandlers_AreCovered(HandlerCase testCase)
    {
        IMediatorImposter mediator = new();
        testCase.Setup(mediator);

        IResult result = await testCase.Invoke(mediator.Instance());

        result.ShouldNotBeNull();
        testCase.Verify?.Invoke(mediator);
    }

    public static IEnumerable<object[]> ContractCases()
    {
        yield return Case(
            "GetContract",
            SetupSuccess<ContractQueries.GetContractQuery, Contract>(new Contract
            {
                ContractId = ContractId,
                EstateId = EstateId,
                Description = "Contract"
            }),
            mediator => ContractHandlers.GetContract(mediator, new DefaultHttpContext(), EstateId, ContractId, CancellationToken.None));

        yield return Case(
            "GetContracts",
            SetupSuccess<ContractQueries.GetContractsQuery, List<Contract>>(new List<Contract>
            {
                new()
                {
                    ContractId = ContractId,
                    EstateId = EstateId,
                    Description = "Contract"
                }
            }),
            mediator => ContractHandlers.GetContracts(mediator, new DefaultHttpContext(), EstateId, CancellationToken.None));

        yield return Case(
            "AddProductToContract",
            SetupSuccess<ContractCommands.AddProductToContractCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.ContractId.ShouldBe(ContractId);
                command.ProductId.ShouldNotBe(Guid.Empty);
                command.RequestDTO.DisplayText.ShouldBe("Display");
            }),
            mediator => ContractHandlers.AddProductToContract(mediator,
                                                              new DefaultHttpContext(),
                                                              EstateId,
                                                              ContractId,
                                                              new AddProductToContractRequest
                                                              {
                                                                  DisplayText = "Display",
                                                                  ProductName = "Product",
                                                                  ProductType = TransactionProcessor.DataTransferObjects.Responses.Contract.ProductType.MobileTopup,
                                                                  Value = 12.34m
                                                              },
                                                              CancellationToken.None));

        yield return Case(
            "AddTransactionFeeForProductToContract",
            SetupSuccess<ContractCommands.AddTransactionFeeForProductToContractCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.ContractId.ShouldBe(ContractId);
                command.ProductId.ShouldBe(ProductId);
                command.TransactionFeeId.ShouldNotBe(Guid.Empty);
            }),
            mediator => ContractHandlers.AddTransactionFeeForProductToContract(mediator,
                                                                                new DefaultHttpContext(),
                                                                                EstateId,
                                                                                ContractId,
                                                                                ProductId,
                                                                                new AddTransactionFeeForProductToContractRequest
                                                                                {
                                                                                    CalculationType = TransactionProcessor.DataTransferObjects.Responses.Contract.CalculationType.Fixed,
                                                                                    Description = "Fee",
                                                                                    FeeType = TransactionProcessor.DataTransferObjects.Responses.Contract.FeeType.Merchant,
                                                                                    Value = 1.23m
                                                                                },
                                                                                CancellationToken.None));

        yield return Case(
            "DisableTransactionFeeForProduct",
            SetupSuccess<ContractCommands.DisableTransactionFeeForProductCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.ContractId.ShouldBe(ContractId);
                command.ProductId.ShouldBe(ProductId);
                command.TransactionFeeId.ShouldBe(SettlementId);
            }),
            mediator => ContractHandlers.DisableTransactionFeeForProduct(mediator,
                                                                          new DefaultHttpContext(),
                                                                          EstateId,
                                                                          ContractId,
                                                                          ProductId,
                                                                          SettlementId,
                                                                          CancellationToken.None));

        yield return Case(
            "CreateContract",
            SetupSuccess<ContractCommands.CreateContractCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.ContractId.ShouldNotBe(Guid.Empty);
                command.RequestDTO.Description.ShouldBe("Contract");
            }),
            mediator => ContractHandlers.CreateContract(mediator,
                                                        new DefaultHttpContext(),
                                                        EstateId,
                                                        new CreateContractRequest
                                                        {
                                                            OperatorId = OperatorId,
                                                            Description = "Contract"
                                                        },
                                                        CancellationToken.None));
    }

    public static IEnumerable<object[]> EstateCases()
    {
        yield return Case(
            "CreateEstate",
            SetupSuccess<EstateCommands.CreateEstateCommand>(command =>
            {
                command.RequestDto.EstateId.ShouldBe(EstateId);
                command.RequestDto.EstateName.ShouldBe("Estate");
            }),
            mediator => EstateHandlers.CreateEstate(mediator,
                                                    new DefaultHttpContext(),
                                                    new CreateEstateRequest
                                                    {
                                                        EstateId = EstateId,
                                                        EstateName = "Estate"
                                                    },
                                                    CancellationToken.None));

        yield return Case(
            "GetEstate",
            SetupSuccess<EstateQueries.GetEstateQuery, EstateModel>(new EstateModel
            {
                EstateId = EstateId,
                Name = "Estate"
            }),
            mediator => EstateHandlers.GetEstate(mediator, new DefaultHttpContext(), EstateId, CancellationToken.None));

        yield return Case(
            "GetEstates",
            SetupSuccess<EstateQueries.GetEstatesQuery, List<EstateModel>>(new List<EstateModel>
            {
                new()
                {
                    EstateId = EstateId,
                    Name = "Estate"
                }
            }),
            mediator => EstateHandlers.GetEstates(mediator, new DefaultHttpContext(), EstateId, CancellationToken.None));

        yield return Case(
            "CreateEstateUser",
            SetupSuccess<EstateCommands.CreateEstateUserCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.RequestDto.EmailAddress.ShouldBe("user@test.local");
            }),
            mediator => EstateHandlers.CreateEstateUser(mediator,
                                                        new DefaultHttpContext(),
                                                        EstateId,
                                                        new CreateEstateUserRequest
                                                        {
                                                            EmailAddress = "user@test.local",
                                                            Password = "password",
                                                            GivenName = "Given",
                                                            FamilyName = "Family"
                                                        },
                                                        CancellationToken.None));

        yield return Case(
            "AssignOperator",
            SetupSuccess<EstateCommands.AddOperatorToEstateCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.RequestDto.OperatorId.ShouldBe(OperatorId);
            }),
            mediator => EstateHandlers.AssignOperator(mediator,
                                                      new DefaultHttpContext(),
                                                      EstateId,
                                                      new TransactionProcessor.DataTransferObjects.Requests.Estate.AssignOperatorRequest { OperatorId = OperatorId },
                                                      CancellationToken.None));

        yield return Case(
            "RemoveOperator",
            SetupSuccess<EstateCommands.RemoveOperatorFromEstateCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.OperatorId.ShouldBe(OperatorId);
            }),
            mediator => EstateHandlers.RemoveOperator(mediator,
                                                       new DefaultHttpContext(),
                                                       EstateId,
                                                       OperatorId,
                                                       CancellationToken.None));
    }

    public static IEnumerable<object[]> FloatCases()
    {
        yield return Case(
            "CreateFloatForContractProduct",
            SetupSuccess<FloatCommands.CreateFloatCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
            }),
            mediator => FloatHandlers.CreateFloat(mediator,
                                                                    new DefaultHttpContext(),
                                                                    EstateId,
                                                                    new CreateFloatRequest
                                                                    {
                                                                        FloatId = FloatId,
                                                                        CreateDateTime = Instant
                                                                    },
                                                                    CancellationToken.None));

        yield return Case(
            "RecordFloatCreditPurchase",
            SetupSuccess<FloatCommands.RecordCreditPurchaseForFloatCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.FloatId.ShouldBe(FloatId);
                command.CreditAmount.ShouldBe(10m);
                command.CostPrice.ShouldBe(8m);
            }),
            mediator => FloatHandlers.RecordFloatCreditPurchase(mediator,
                                                                new DefaultHttpContext(),
                                                                EstateId,
                                                                new RecordFloatCreditPurchaseRequest
                                                                {
                                                                    FloatId = FloatId,
                                                                    PurchaseDateTime = Instant,
                                                                    CreditAmount = 10m,
                                                                    CostPrice = 8m
                                                                },
                                                                CancellationToken.None));
    }

    public static IEnumerable<object[]> MerchantCases()
    {
        yield return Case(
            "GetMerchantBalance",
            SetupSuccess<MerchantQueries.GetMerchantBalanceQuery, MerchantBalanceState>(new MerchantBalanceState
            {
                EstateId = EstateId,
                MerchantId = MerchantId,
                Balance = 12m,
                AvailableBalance = 8m
            }),
            mediator => MerchantHandlers.GetMerchantBalance(mediator, new DefaultHttpContext(), EstateId, MerchantId, CancellationToken.None));

        yield return Case(
            "GetMerchantBalanceLive",
            SetupSuccess<MerchantQueries.GetMerchantLiveBalanceQuery, MerchantBalanceProjectionState1>(
                new MerchantBalanceProjectionState1(new ProjectionMerchant(MerchantId.ToString(), "Merchant", 1, 9m))),
            mediator => MerchantHandlers.GetMerchantBalanceLive(mediator, new DefaultHttpContext(), EstateId, MerchantId, CancellationToken.None));

        yield return Case(
            "GetMerchantBalanceHistory",
            SetupSuccess<MerchantQueries.GetMerchantBalanceHistoryQuery, List<MerchantBalanceChangedEntry>>(new List<MerchantBalanceChangedEntry>
            {
                new()
                {
                    EstateId = EstateId,
                    MerchantId = MerchantId,
                    ChangeAmount = 3m,
                    Balance = 12m,
                    DateTime = Instant,
                    DebitOrCredit = "Credit",
                    OriginalEventId = TransactionId,
                    Reference = "REF"
                }
            }),
            mediator => MerchantHandlers.GetMerchantBalanceHistory(mediator,
                                                                    new DefaultHttpContext(),
                                                                    EstateId,
                                                                    MerchantId,
                                                                    Instant.AddDays(-1),
                                                                    Instant,
                                                                    CancellationToken.None));

        yield return Case(
            "CreateMerchant",
            SetupSuccess<MerchantCommands.CreateMerchantCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.RequestDto.Name.ShouldBe("Merchant");
            }),
            mediator => MerchantHandlers.CreateMerchant(mediator,
                                                        new DefaultHttpContext(),
                                                        EstateId,
                                                        new CreateMerchantRequest
                                                        {
                                                            MerchantId = MerchantId,
                                                            Name = "Merchant"
                                                        },
                                                        CancellationToken.None));

        yield return Case(
            "AssignOperator",
            SetupSuccess<MerchantCommands.AssignOperatorToMerchantCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.RequestDto.OperatorId.ShouldBe(OperatorId);
            }),
            mediator => MerchantHandlers.AssignOperator(mediator,
                                                        new DefaultHttpContext(),
                                                        EstateId,
                                                        MerchantId,
                                                        new TransactionProcessor.DataTransferObjects.Requests.Merchant.AssignOperatorRequest
                                                        {
                                                            OperatorId = OperatorId
                                                        },
                                                        CancellationToken.None));

        yield return Case(
            "RemoveOperator",
            SetupSuccess<MerchantCommands.RemoveOperatorFromMerchantCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.OperatorId.ShouldBe(OperatorId);
            }),
            mediator => MerchantHandlers.RemoveOperator(mediator,
                                                         new DefaultHttpContext(),
                                                         EstateId,
                                                         MerchantId,
                                                         OperatorId,
                                                         CancellationToken.None));

        yield return Case(
            "AddDevice",
            SetupSuccess<MerchantCommands.AddMerchantDeviceCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.RequestDto.DeviceIdentifier.ShouldBe("device-1");
            }),
            mediator => MerchantHandlers.AddDevice(mediator,
                                                   new DefaultHttpContext(),
                                                   EstateId,
                                                   MerchantId,
                                                   new AddMerchantDeviceRequest
                                                   {
                                                       DeviceIdentifier = "device-1"
                                                   },
                                                   CancellationToken.None));

        yield return Case(
            "SwapMerchantDevice",
            SetupSuccess<MerchantCommands.SwapMerchantDeviceCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.DeviceIdentifier.ShouldBe("device-old");
                command.RequestDto.NewDeviceIdentifier.ShouldBe("device-new");
            }),
            mediator => MerchantHandlers.SwapMerchantDevice(mediator,
                                                            new DefaultHttpContext(),
                                                            EstateId,
                                                            MerchantId,
                                                            "device-old",
                                                            new SwapMerchantDeviceRequest
                                                            {
                                                                NewDeviceIdentifier = "device-new"
                                                            },
                                                            CancellationToken.None));

        yield return Case(
            "AddContract",
            SetupSuccess<MerchantCommands.AddMerchantContractCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.RequestDto.ContractId.ShouldBe(ContractId);
            }),
            mediator => MerchantHandlers.AddContract(mediator,
                                                     new DefaultHttpContext(),
                                                     EstateId,
                                                     MerchantId,
                                                     new AddMerchantContractRequest
                                                     {
                                                         ContractId = ContractId
                                                     },
                                                     CancellationToken.None));

        yield return Case(
            "RemoveContract",
            SetupSuccess<MerchantCommands.RemoveMerchantContractCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.ContractId.ShouldBe(ContractId);
            }),
            mediator => MerchantHandlers.RemoveContract(mediator,
                                                        new DefaultHttpContext(),
                                                        EstateId,
                                                        MerchantId,
                                                        ContractId,
                                                        CancellationToken.None));

        yield return Case(
            "CreateMerchantUser",
            SetupSuccess<MerchantCommands.CreateMerchantUserCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.RequestDto.EmailAddress.ShouldBe("user@test.local");
            }),
            mediator => MerchantHandlers.CreateMerchantUser(mediator,
                                                             new DefaultHttpContext(),
                                                             EstateId,
                                                             MerchantId,
                                                             new CreateMerchantUserRequest
                                                             {
                                                                 EmailAddress = "user@test.local",
                                                                 Password = "password",
                                                                 GivenName = "Given",
                                                                 MiddleName = "Middle",
                                                                 FamilyName = "Family"
                                                             },
                                                             CancellationToken.None));

        yield return Case(
            "MakeDeposit",
            SetupSuccess<MerchantCommands.MakeMerchantDepositCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.DepositSource.ShouldBe(TransactionProcessor.DataTransferObjects.Requests.Merchant.MerchantDepositSource.Manual);
                command.RequestDto.Amount.ShouldBe(15.50m);
            }),
            mediator => MerchantHandlers.MakeDeposit(mediator,
                                                     new DefaultHttpContext(),
                                                     EstateId,
                                                     MerchantId,
                                                     new MakeMerchantDepositRequest
                                                     {
                                                         Amount = 15.50m,
                                                         DepositDateTime = Instant,
                                                         Reference = "DEP-1"
                                                     },
                                                     CancellationToken.None));

        yield return Case(
            "MakeWithdrawal",
            SetupSuccess<MerchantCommands.MakeMerchantWithdrawalCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.RequestDto.Amount.ShouldBe(9.25m);
            }),
            mediator => MerchantHandlers.MakeWithdrawal(mediator,
                                                        new DefaultHttpContext(),
                                                        EstateId,
                                                        MerchantId,
                                                        new MakeMerchantWithdrawalRequest
                                                        {
                                                            Amount = 9.25m,
                                                            WithdrawalDateTime = Instant,
                                                            Reference = "WDL-1"
                                                        },
                                                        CancellationToken.None));

        yield return Case(
            "UpdateMerchant",
            SetupSuccess<MerchantCommands.UpdateMerchantCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.RequestDto.Name.ShouldBe("Merchant");
            }),
            mediator => MerchantHandlers.UpdateMerchant(mediator,
                                                        new DefaultHttpContext(),
                                                        EstateId,
                                                        MerchantId,
                                                        new UpdateMerchantRequest
                                                        {
                                                            Name = "Merchant"
                                                        },
                                                        CancellationToken.None));

        yield return Case(
            "UpdateMerchantOpening",
            SetupSuccess<MerchantCommands.UpdateMerchantOpeningHoursCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.RequestDto.Sunday.ShouldNotBeNull();
            }),
            mediator => MerchantHandlers.UpdateMerchantOpening(mediator,
                                                               new DefaultHttpContext(),
                                                               EstateId,
                                                               MerchantId,
                                                               new MerchantOpeningRequest
                                                               {
                                                                   Sunday = new TransactionProcessor.DataTransferObjects.Requests.Merchant.OpeningHours
                                                                   {
                                                                       Opening = "08:00",
                                                                       Closing = "17:00"
                                                                   }
                                                               },
                                                               CancellationToken.None));

        yield return Case(
            "CreateMerchantSchedule",
            SetupSuccess<MerchantCommands.CreateMerchantScheduleCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.RequestDto.Year.ShouldBe(2024);
            }),
            mediator => MerchantHandlers.CreateMerchantSchedule(mediator,
                                                                new DefaultHttpContext(),
                                                                EstateId,
                                                                MerchantId,
                                                                new CreateMerchantScheduleRequest
                                                                {
                                                                    Year = 2024,
                                                                    Months = new List<MerchantScheduleMonthRequest>
                                                                    {
                                                                        new()
                                                                        {
                                                                            Month = 1,
                                                                            ClosedDays = new List<int> { 1, 2 }
                                                                        }
                                                                    }
                                                                },
                                                                CancellationToken.None));

        yield return Case(
            "UpdateMerchantSchedule",
            SetupSuccess<MerchantCommands.UpdateMerchantScheduleCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.Year.ShouldBe(2024);
            }),
            mediator => MerchantHandlers.UpdateMerchantSchedule(mediator,
                                                                new DefaultHttpContext(),
                                                                EstateId,
                                                                MerchantId,
                                                                2024,
                                                                new UpdateMerchantScheduleRequest
                                                                {
                                                                    Months = new List<MerchantScheduleMonthRequest>
                                                                    {
                                                                        new()
                                                                        {
                                                                            Month = 2,
                                                                            ClosedDays = new List<int> { 10 }
                                                                        }
                                                                    }
                                                                },
                                                                CancellationToken.None));

        yield return Case(
            "GetMerchantSchedule",
            SetupSuccess<MerchantQueries.GetMerchantScheduleQuery, TransactionProcessor.Models.MerchantSchedule.MerchantSchedule>(new TransactionProcessor.Models.MerchantSchedule.MerchantSchedule
            {
                MerchantScheduleId = Guid.NewGuid(),
                EstateId = EstateId,
                MerchantId = MerchantId,
                Year = 2024,
                Months = new List<TransactionProcessor.Models.MerchantSchedule.MerchantScheduleMonth>
                {
                    new()
                    {
                        Month = 3,
                        ClosedDays = new List<int> { 5, 6 }
                    }
                }
            }),
            mediator => MerchantHandlers.GetMerchantSchedule(mediator,
                                                             new DefaultHttpContext(),
                                                             EstateId,
                                                             MerchantId,
                                                             2024,
                                                             CancellationToken.None));

        yield return Case(
            "GetMerchantScheduleFromReadModel",
            SetupSuccess<MerchantQueries.GetMerchantScheduleFromReadModelQuery, TransactionProcessor.Models.MerchantSchedule.MerchantSchedule>(new TransactionProcessor.Models.MerchantSchedule.MerchantSchedule
            {
                MerchantScheduleId = Guid.NewGuid(),
                EstateId = EstateId,
                MerchantId = MerchantId,
                Year = 2025,
                Months = new List<TransactionProcessor.Models.MerchantSchedule.MerchantScheduleMonth>
                {
                    new()
                    {
                        Month = 8,
                        ClosedDays = new List<int> { 1 }
                    }
                }
            }),
            mediator => MerchantHandlers.GetMerchantScheduleFromReadModel(mediator,
                                                                           new DefaultHttpContext(),
                                                                           EstateId,
                                                                           MerchantId,
                                                                           2025,
                                                                           CancellationToken.None));

        yield return Case(
            "AddMerchantAddress",
            SetupSuccess<MerchantCommands.AddMerchantAddressCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.RequestDto.AddressLine1.ShouldBe("Line 1");
            }),
            mediator => MerchantHandlers.AddMerchantAddress(mediator,
                                                            new DefaultHttpContext(),
                                                            EstateId,
                                                            MerchantId,
                                                            new TransactionProcessor.DataTransferObjects.Requests.Merchant.Address
                                                            {
                                                                AddressLine1 = "Line 1",
                                                                AddressLine2 = "Line 2",
                                                                Country = "GB",
                                                                PostalCode = "PC1 1AA",
                                                                Region = "Region",
                                                                Town = "Town"
                                                            },
                                                            CancellationToken.None));

        yield return Case(
            "UpdateMerchantAddress",
            SetupSuccess<MerchantCommands.UpdateMerchantAddressCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.AddressId.ShouldBe(AddressId);
                command.RequestDto.AddressLine1.ShouldBe("Updated Line 1");
            }),
            mediator => MerchantHandlers.UpdateMerchantAddress(mediator,
                                                               new DefaultHttpContext(),
                                                               EstateId,
                                                               MerchantId,
                                                               AddressId,
                                                               new TransactionProcessor.DataTransferObjects.Requests.Merchant.Address
                                                               {
                                                                   AddressLine1 = "Updated Line 1",
                                                                   Country = "GB",
                                                                   PostalCode = "PC1 2BB",
                                                                   Region = "Updated Region",
                                                                   Town = "Updated Town"
                                                               },
                                                               CancellationToken.None));

        yield return Case(
            "AddMerchantContact",
            SetupSuccess<MerchantCommands.AddMerchantContactCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.RequestDto.ContactName.ShouldBe("Contact");
            }),
            mediator => MerchantHandlers.AddMerchantContact(mediator,
                                                            new DefaultHttpContext(),
                                                            EstateId,
                                                            MerchantId,
                                                            new TransactionProcessor.DataTransferObjects.Requests.Merchant.Contact
                                                            {
                                                                ContactName = "Contact",
                                                                EmailAddress = "contact@test.local",
                                                                PhoneNumber = "01234 567890"
                                                            },
                                                            CancellationToken.None));

        yield return Case(
            "UpdateMerchantContact",
            SetupSuccess<MerchantCommands.UpdateMerchantContactCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.ContactId.ShouldBe(ContactId);
                command.RequestDto.EmailAddress.ShouldBe("updated@test.local");
            }),
            mediator => MerchantHandlers.UpdateMerchantContact(mediator,
                                                               new DefaultHttpContext(),
                                                               EstateId,
                                                               MerchantId,
                                                               ContactId,
                                                               new TransactionProcessor.DataTransferObjects.Requests.Merchant.Contact
                                                               {
                                                                   ContactName = "Updated Contact",
                                                                   EmailAddress = "updated@test.local",
                                                                   PhoneNumber = "09876 543210"
                                                               },
                                                               CancellationToken.None));

        yield return Case(
            "GetMerchant",
            SetupSuccess<MerchantQueries.GetMerchantQuery, MerchantModel>(new MerchantModel
            {
                MerchantId = MerchantId,
                EstateId = EstateId,
                MerchantName = "Merchant"
            }),
            mediator => MerchantHandlers.GetMerchant(mediator, new DefaultHttpContext(), EstateId, MerchantId, CancellationToken.None));

        yield return Case(
            "GetMerchantContracts",
            SetupSuccess<MerchantQueries.GetMerchantContractsQuery, List<Contract>>(new List<Contract>
            {
                new()
                {
                    ContractId = ContractId,
                    EstateId = EstateId,
                    Description = "Contract"
                }
            }),
            mediator => MerchantHandlers.GetMerchantContracts(mediator, new DefaultHttpContext(), EstateId, MerchantId, CancellationToken.None));

        yield return Case(
            "GetMerchants",
            SetupSuccess<MerchantQueries.GetMerchantsQuery, List<MerchantModel>>(new List<MerchantModel>
            {
                new()
                {
                    MerchantId = MerchantId,
                    EstateId = EstateId,
                    MerchantName = "Merchant"
                }
            }),
            mediator => MerchantHandlers.GetMerchants(mediator, new DefaultHttpContext(), EstateId, CancellationToken.None));

        yield return Case(
            "GetTransactionFeesForProduct",
            SetupSuccess<MerchantQueries.GetTransactionFeesForProductQuery, List<global::TransactionProcessor.Models.Contract.ContractProductTransactionFee>>(new List<global::TransactionProcessor.Models.Contract.ContractProductTransactionFee>()),
            mediator => MerchantHandlers.GetTransactionFeesForProduct(mediator,
                                                                     new DefaultHttpContext(),
                                                                     EstateId,
                                                                     MerchantId,
                                                                     ContractId,
                                                                     ProductId,
                                                                     CancellationToken.None));

        yield return Case(
            "GenerateMerchantStatement",
            SetupSuccess<MerchantCommands.GenerateMerchantStatementCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.MerchantId.ShouldBe(MerchantId);
                command.RequestDto.MerchantStatementDate.ShouldBe(Instant);
            }),
            mediator => MerchantHandlers.GenerateMerchantStatement(mediator,
                                                                   new DefaultHttpContext(),
                                                                   EstateId,
                                                                   MerchantId,
                                                                   new GenerateMerchantStatementRequest
                                                                   {
                                                                       MerchantStatementDate = Instant
                                                                   },
                                                                   CancellationToken.None));
    }

    public static IEnumerable<object[]> OperatorCases()
    {
        yield return Case(
            "CreateOperator",
            SetupSuccess<OperatorCommands.CreateOperatorCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.RequestDto.Name.ShouldBe("Operator");
            }),
            mediator => OperatorHandlers.CreateOperator(mediator,
                                                        new DefaultHttpContext(),
                                                        EstateId,
                                                        new CreateOperatorRequest
                                                        {
                                                            OperatorId = OperatorId,
                                                            Name = "Operator"
                                                        },
                                                        CancellationToken.None));

        yield return Case(
            "UpdateOperator",
            SetupSuccess<OperatorCommands.UpdateOperatorCommand>(command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.OperatorId.ShouldBe(OperatorId);
            }),
            mediator => OperatorHandlers.UpdateOperator(mediator,
                                                        new DefaultHttpContext(),
                                                        EstateId,
                                                        OperatorId,
                                                        new UpdateOperatorRequest
                                                        {
                                                            Name = "Operator"
                                                        },
                                                        CancellationToken.None));

        yield return Case(
            "GetOperator",
            SetupSuccess<OperatorQueries.GetOperatorQuery, OperatorModel>(new OperatorModel
            {
                OperatorId = OperatorId,
                Name = "Operator"
            }),
            mediator => OperatorHandlers.GetOperator(mediator, new DefaultHttpContext(), EstateId, OperatorId, CancellationToken.None));

        yield return Case(
            "GetOperators",
            SetupSuccess<OperatorQueries.GetOperatorsQuery, List<OperatorModel>>(new List<OperatorModel>
            {
                new()
                {
                    OperatorId = OperatorId,
                    Name = "Operator"
                }
            }),
            mediator => OperatorHandlers.GetOperators(mediator, new DefaultHttpContext(), EstateId, CancellationToken.None));
    }

    public static IEnumerable<object[]> SettlementCases()
    {
        yield return Case(
            "GetPendingSettlement",
            SetupSuccess<SettlementQueries.GetPendingSettlementQuery, PendingSettlementModel>(new PendingSettlementModel
            {
                EstateId = EstateId,
                MerchantId = MerchantId,
                SettlementDate = Instant,
                NumberOfFeesPendingSettlement = 2,
                NumberOfFeesSettled = 1,
                SettlementCompleted = false
            }),
            mediator => SettlementHandlers.GetPendingSettlement(mediator, new DefaultHttpContext(), Instant, EstateId, MerchantId, CancellationToken.None));

        yield return Case(
            "ProcessSettlement",
            SetupSuccess<SettlementCommands.ProcessSettlementCommand, Guid>(SettlementId),
            mediator => SettlementHandlers.ProcessSettlement(mediator, new DefaultHttpContext(), Instant, EstateId, MerchantId, CancellationToken.None));

        yield return Case(
            "GetSettlement",
            SetupSuccess<SettlementQueries.GetSettlementQuery, SettlementModel>(new SettlementModel
            {
                SettlementId = SettlementId,
                SettlementDate = Instant,
                IsCompleted = true,
                NumberOfFeesSettled = 1,
                ValueOfFeesSettled = 12m,
                SettlementFees = new List<SettlementFeeModel>
                {
                    new()
                    {
                        TransactionId = TransactionId,
                        MerchantId = MerchantId,
                        MerchantName = "Merchant",
                        SettlementId = SettlementId,
                        SettlementDate = Instant,
                        CalculatedValue = 1.5m,
                        FeeDescription = "Fee",
                        IsSettled = true,
                        OperatorIdentifier = "OP-1"
                    }
                }
            }),
            mediator => SettlementHandlers.GetSettlement(mediator, new DefaultHttpContext(), EstateId, SettlementId, MerchantId, CancellationToken.None));

        yield return Case(
            "GetSettlements",
            SetupSuccess<SettlementQueries.GetSettlementsQuery, List<SettlementModel>>(new List<SettlementModel>
            {
                new()
                {
                    SettlementId = SettlementId,
                    SettlementDate = Instant,
                    IsCompleted = true,
                    NumberOfFeesSettled = 1,
                    ValueOfFeesSettled = 12m,
                    SettlementFees = new List<SettlementFeeModel>
                    {
                        new()
                        {
                            TransactionId = TransactionId,
                            MerchantId = MerchantId,
                            MerchantName = "Merchant",
                            SettlementDate = Instant,
                            SettlementId = SettlementId,
                            CalculatedValue = 1.5m,
                            FeeDescription = "Fee",
                            IsSettled = true,
                            OperatorIdentifier = "OP-1"
                        }
                    }
                }
            }),
            mediator => SettlementHandlers.GetSettlements(mediator,
                                                           new DefaultHttpContext(),
                                                           EstateId,
                                                           MerchantId,
                                                           "2024-01-01",
                                                           "2024-01-31",
                                                           CancellationToken.None));
    }

    public static IEnumerable<object[]> VoucherCases()
    {
        yield return Case(
            "RedeemVoucher_ExplicitDate",
            SetupSuccess<VoucherCommands.RedeemVoucherCommand, RedeemVoucherModel>(new RedeemVoucherModel
            {
                VoucherCode = VoucherCode,
                RemainingBalance = 10m,
                ExpiryDate = Instant.AddDays(1)
            }),
            mediator => VoucherHandlers.RedeemVoucher(mediator,
                                                      new DefaultHttpContext(),
                                                      new RedeemVoucherRequest
                                                      {
                                                          EstateId = EstateId,
                                                          VoucherCode = VoucherCode,
                                                          RedeemedDateTime = Instant
                                                      },
                                                      CancellationToken.None));

        yield return Case(
            "RedeemVoucher_DefaultDate",
            SetupSuccess<VoucherCommands.RedeemVoucherCommand, RedeemVoucherModel>(new RedeemVoucherModel
            {
                VoucherCode = VoucherCode,
                RemainingBalance = 10m,
                ExpiryDate = Instant.AddDays(1)
            }, command =>
            {
                command.EstateId.ShouldBe(EstateId);
                command.VoucherCode.ShouldBe(VoucherCode);
                command.RedeemedDateTime.ShouldBeGreaterThan(DateTime.Now.AddSeconds(-5));
                command.RedeemedDateTime.ShouldBeLessThan(DateTime.Now.AddSeconds(5));
            }),
            mediator => VoucherHandlers.RedeemVoucher(mediator,
                                                      new DefaultHttpContext(),
                                                      new RedeemVoucherRequest
                                                      {
                                                          EstateId = EstateId,
                                                          VoucherCode = VoucherCode
                                                      },
                                                      CancellationToken.None));

        yield return Case(
            "GetVoucher_ByCode",
            SetupSuccess<VoucherQueries.GetVoucherByVoucherCodeQuery, Voucher>(new Voucher
            {
                VoucherCode = VoucherCode,
                EstateId = EstateId
            }),
            mediator => VoucherHandlers.GetVoucher(mediator, new DefaultHttpContext(), EstateId, VoucherCode, null, CancellationToken.None));

        yield return Case(
            "GetVoucher_ByTransactionId",
            SetupSuccess<VoucherQueries.GetVoucherByTransactionIdQuery, Voucher>(new Voucher
            {
                VoucherCode = VoucherCode,
                EstateId = EstateId,
                TransactionId = VoucherTransactionId
            }),
            mediator => VoucherHandlers.GetVoucher(mediator, new DefaultHttpContext(), EstateId, "", VoucherTransactionId, CancellationToken.None));

        yield return Case(
            "GetVoucher_Invalid",
            mediator => { },
            mediator => VoucherHandlers.GetVoucher(mediator, new DefaultHttpContext(), EstateId, "", null, CancellationToken.None),
            mediator => { });
    }

    private static object[] Case(string name, MediatorSetup setup, Func<IMediator, Task<IResult>> invoke, Action<IMediatorImposter>? verify = null)
        => new object[] { new HandlerCase(name, setup.Configure, invoke, verify ?? setup.Verify) };

    private static object[] Case(string name, Action<IMediatorImposter> setup, Func<IMediator, Task<IResult>> invoke, Action<IMediatorImposter>? verify = null)
        => new object[] { new HandlerCase(name, setup, invoke, verify) };

    private static MediatorSetup SetupSuccess<TRequest, TResponse>(TResponse response, Action<TRequest>? capture = null)
        where TRequest : class, IRequest<Result<TResponse>>
    {
        return new MediatorSetup(mediator =>
        {
            mediator.Send<Result<TResponse>>(Arg<IRequest<Result<TResponse>>>.Any(), Arg<CancellationToken>.Any())
                    .ReturnsAsync(Result.Success(response))
                    .Callback((IRequest<Result<TResponse>> request, CancellationToken _) => { capture?.Invoke((TRequest)request); return Task.CompletedTask; });
        }, mediator => mediator.Send<Result<TResponse>>(Arg<IRequest<Result<TResponse>>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once()));
    }

    private static MediatorSetup SetupSuccess<TRequest>(Action<TRequest>? capture = null)
        where TRequest : class, IRequest<Result>
    {
        return new MediatorSetup(mediator =>
        {
            mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
                    .ReturnsAsync(Result.Success())
                    .Callback((IRequest<Result> request, CancellationToken _) => { capture?.Invoke((TRequest)request); return Task.CompletedTask; });
        }, mediator => mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once()));
    }

    private sealed record MediatorSetup(Action<IMediatorImposter> Configure, Action<IMediatorImposter> Verify);

    public sealed record HandlerCase(string Name,
                                     Action<IMediatorImposter> Setup,
                                     Func<IMediator, Task<IResult>> Invoke,
                                     Action<IMediatorImposter>? Verify = null);
}
