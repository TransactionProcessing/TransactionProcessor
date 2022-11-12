using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.RequestHandlers;
using TransactionProcessor.BusinessLogic.Services;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.RequestHandler
{
    using System.Threading;
    using Requests;
    using Testing;

    public class VoucherManagementRequestHandlerTests
    {
        [Fact]
        public async Task VoucherManagementRequestHandler_IssueVoucherRequest_IsHandled()
        {
            Mock<IVoucherDomainService> voucherDomainService = new Mock<IVoucherDomainService>();
            voucherDomainService.Setup(v => v.IssueVoucher(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<Guid>(),
                                                           It.IsAny<Guid>(), It.IsAny<DateTime>(),
                                                           It.IsAny<Decimal>(), It.IsAny<String>(), It.IsAny<String>(),
                                                           It.IsAny<CancellationToken>())).ReturnsAsync(TestData.IssueVoucherResponse);

            VoucherManagementRequestHandler handler = new VoucherManagementRequestHandler(voucherDomainService.Object);


            IssueVoucherRequest command = TestData.IssueVoucherRequest;
            Should.NotThrow(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });
        }

        [Fact]
        public async Task VoucherManagementRequestHandler_RedeemVoucherRequest_IsHandled()
        {
            Mock<IVoucherDomainService> voucherDomainService = new Mock<IVoucherDomainService>();
            voucherDomainService.Setup(v => v.RedeemVoucher(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<DateTime>(),
                                                           It.IsAny<CancellationToken>())).ReturnsAsync(TestData.RedeemVoucherResponse);

            VoucherManagementRequestHandler handler = new VoucherManagementRequestHandler(voucherDomainService.Object);

            RedeemVoucherRequest command = TestData.RedeemVoucherRequest;
            Should.NotThrow(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });
        }
    }
}
