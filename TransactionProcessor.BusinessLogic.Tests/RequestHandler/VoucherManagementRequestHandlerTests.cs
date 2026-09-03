using Imposter.Abstractions;
using Shouldly;
using System;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.RequestHandlers;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.RequestHandler
{
    using Shared.Serialisation;
    using System.Text.Json;
    using System.Threading;
    using Testing;

    public class VoucherManagementRequestHandlerTests
    {
        public VoucherManagementRequestHandlerTests() {
            StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
        }

        [Fact]
        public async Task VoucherManagementRequestHandler_IssueVoucherRequest_IsHandled()
        {
            IVoucherDomainServiceImposter voucherDomainService = new();
            IVoucherManagementManagerImposter voucherManagementManager = new();
            voucherDomainService.IssueVoucher(Arg<VoucherCommands.IssueVoucherCommand>.Any(),
                                                           Arg<CancellationToken>.Any()).ReturnsAsync(TestData.IssueVoucherResponse);

            VoucherManagementRequestHandler handler = new VoucherManagementRequestHandler(voucherDomainService.Instance(), voucherManagementManager.Instance());
            
            var command = TestData.IssueVoucherCommand;
            var result = await handler.Handle(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherManagementRequestHandler_RedeemVoucherRequest_IsHandled()
        {
            IVoucherDomainServiceImposter voucherDomainService = new();
            IVoucherManagementManagerImposter voucherManagementManager = new();
            voucherDomainService.RedeemVoucher(Arg<Guid>.Any(), Arg<String>.Any(), Arg<DateTime>.Any(),
                                                           Arg<CancellationToken>.Any()).ReturnsAsync(TestData.RedeemVoucherResponse);

            VoucherManagementRequestHandler handler = new VoucherManagementRequestHandler(voucherDomainService.Instance(), voucherManagementManager.Instance());

            var command = TestData.RedeemVoucherCommand;
            var result = await handler.Handle(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }
    }
}

