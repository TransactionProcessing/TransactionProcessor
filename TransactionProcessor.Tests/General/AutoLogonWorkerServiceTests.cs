using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.Logger;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.OperatorInterfaces;
using Xunit;

namespace TransactionProcessor.Tests.General;

public class AutoLogonWorkerServiceTests {
    public AutoLogonWorkerServiceTests() {
        Logger.Initialise(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
    }

    [Fact]
    public async Task OperatorLogonAsync_TransientGrpcUnavailable_ExceptionIsHandled() {
        Mock<IOperatorProxy> proxy = new Mock<IOperatorProxy>();
        proxy.Setup(x => x.ProcessLogonMessage(It.IsAny<CancellationToken>()))
             .ThrowsAsync(new RpcException(new Status(StatusCode.Unavailable, "RPC server unavailable")));

        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<Func<String, IOperatorProxy>>(_ => _ => proxy.Object);
        IServiceProvider previousServiceProvider = Startup.ServiceProvider;

        try {
            Startup.ServiceProvider = services.BuildServiceProvider();

            AutoLogonWorkerService worker = new AutoLogonWorkerService();

            Exception exception = await Record.ExceptionAsync(async () =>
                                                              {
                                                                  await worker.OperatorLogonAsync("operator-1", CancellationToken.None);
                                                              });

            exception.ShouldBeNull();
        }
        finally {
            Startup.ServiceProvider = previousServiceProvider;
        }
    }

    [Fact]
    public async Task OperatorLogonAsync_NonTransientException_ExceptionIsHandled() {
        Mock<IOperatorProxy> proxy = new Mock<IOperatorProxy>();
        proxy.Setup(x => x.ProcessLogonMessage(It.IsAny<CancellationToken>()))
             .ThrowsAsync(new InvalidOperationException("boom"));

        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<Func<String, IOperatorProxy>>(_ => _ => proxy.Object);
        IServiceProvider previousServiceProvider = Startup.ServiceProvider;

        try {
            Startup.ServiceProvider = services.BuildServiceProvider();

            AutoLogonWorkerService worker = new AutoLogonWorkerService();

            Exception exception = await Record.ExceptionAsync(async () =>
                                                              {
                                                                  await worker.OperatorLogonAsync("operator-2", CancellationToken.None);
                                                              });

            exception.ShouldBeNull();
        }
        finally {
            Startup.ServiceProvider = previousServiceProvider;
        }
    }
}
