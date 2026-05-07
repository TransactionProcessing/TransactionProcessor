using Moq;
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
            Mock<IVoucherDomainService> voucherDomainService = new Mock<IVoucherDomainService>();
            Mock<IVoucherManagementManager> voucherManagementManager = new Mock<IVoucherManagementManager>();
            voucherDomainService.Setup(v => v.IssueVoucher(It.IsAny<VoucherCommands.IssueVoucherCommand>(),
                                                           It.IsAny<CancellationToken>())).ReturnsAsync(TestData.IssueVoucherResponse);

            VoucherManagementRequestHandler handler = new VoucherManagementRequestHandler(voucherDomainService.Object, voucherManagementManager.Object);
            
            var command = TestData.IssueVoucherCommand;
            var result = await handler.Handle(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherManagementRequestHandler_RedeemVoucherRequest_IsHandled()
        {
            Mock<IVoucherDomainService> voucherDomainService = new Mock<IVoucherDomainService>();
            Mock<IVoucherManagementManager> voucherManagementManager = new Mock<IVoucherManagementManager>();
            voucherDomainService.Setup(v => v.RedeemVoucher(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<DateTime>(),
                                                           It.IsAny<CancellationToken>())).ReturnsAsync(TestData.RedeemVoucherResponse);

            VoucherManagementRequestHandler handler = new VoucherManagementRequestHandler(voucherDomainService.Object, voucherManagementManager.Object);

            var command = TestData.RedeemVoucherCommand;
            var result = await handler.Handle(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
    }
}
