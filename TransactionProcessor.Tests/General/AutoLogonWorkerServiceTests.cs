using Grpc.Core;
using Imposter.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Logger;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.OperatorInterfaces;
using Xunit;
using NullLogger = Shared.Logger.NullLogger;

namespace TransactionProcessor.Tests.General;

public class AutoLogonWorkerServiceTests {
    public AutoLogonWorkerServiceTests() {
        Logger.Initialise(NullLogger.Instance);
    }

    [Fact]
    public async Task OperatorLogonAsync_TransientGrpcUnavailable_ExceptionIsHandled() {
        IOperatorProxyImposter proxy = new();
        proxy.ProcessLogonMessage(Arg<CancellationToken>.Any())
             .Throws(new RpcException(new Status(StatusCode.Unavailable, "RPC server unavailable")));
        AutoLogonWorkerService worker = CreateWorker(proxy.Instance());

        Exception exception = await Record.ExceptionAsync(() => worker.OperatorLogonAsync("operator-1", CancellationToken.None));

        exception.ShouldBeNull();
    }

    [Fact]
    public async Task OperatorLogonAsync_NonTransientException_ExceptionIsHandled() {
        IOperatorProxyImposter proxy = new();
        proxy.ProcessLogonMessage(Arg<CancellationToken>.Any())
             .Throws(new InvalidOperationException("boom"));
        AutoLogonWorkerService worker = CreateWorker(proxy.Instance());

        Exception exception = await Record.ExceptionAsync(() => worker.OperatorLogonAsync("operator-2", CancellationToken.None));

        exception.ShouldBeNull();
    }

    private static AutoLogonWorkerService CreateWorker(IOperatorProxy proxy) =>
        new(new AutoLogonOperatorConfiguration(Array.Empty<String>()), _ => proxy);
}
