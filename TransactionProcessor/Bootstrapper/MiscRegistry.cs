﻿using Microsoft.Extensions.Logging;

namespace TransactionProcessor.Bootstrapper
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions;
    using BusinessLogic.Common;
    using BusinessLogic.Manager;
    using BusinessLogic.Services;
    using Lamar;
    using Microsoft.Extensions.DependencyInjection;
    using Shared.General;
    using Shared.Middleware;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    [ExcludeFromCodeCoverage]
    public class MiscRegistry : ServiceRegistry {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MiscRegistry"/> class.
        /// </summary>
        public MiscRegistry() {
            this.AddSingleton<ITransactionReceiptBuilder, TransactionReceiptBuilder>();
            this.AddSingleton<IFileSystem, FileSystem>();
            this.AddSingleton<IFeeCalculationManager, FeeCalculationManager>();
            this.AddSingleton<IVoucherManagementManager, VoucherManagementManager>();
            this.AddSingleton<ITransactionProcessorManager, TransactionProcessorManager>();
            this.AddSingleton<IMemoryCacheWrapper, MemoryCacheWrapper>();
            this.AddSingleton<IStatementBuilder, StatementBuilder>();

            bool logRequests = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogRequests", true);
            bool logResponses = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogResponses", true);
            LogLevel middlewareLogLevel = ConfigurationReader.GetValueOrDefault<LogLevel>("MiddlewareLogging", "MiddlewareLogLevel", LogLevel.Warning);

            RequestResponseMiddlewareLoggingConfig config = new RequestResponseMiddlewareLoggingConfig(middlewareLogLevel, logRequests, logResponses);

            this.AddSingleton(config);
        }

        #endregion
    }
}