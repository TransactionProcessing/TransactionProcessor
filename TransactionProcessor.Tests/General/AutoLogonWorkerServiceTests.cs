using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Imposter.Abstractions;
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
        IOperatorProxyImposter proxy = new IOperatorProxyImposter();
        proxy.ProcessLogonMessage(Arg<CancellationToken>.Any())
             .Throws(new RpcException(new Status(StatusCode.Unavailable, "RPC server unavailable")));

        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<Func<String, IOperatorProxy>>(_ => _ => proxy.Instance());
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
        IOperatorProxyImposter proxy = new IOperatorProxyImposter();
        proxy.ProcessLogonMessage(Arg<CancellationToken>.Any())
             .Throws(new InvalidOperationException("boom"));

        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<Func<String, IOperatorProxy>>(_ => _ => proxy.Instance());
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
