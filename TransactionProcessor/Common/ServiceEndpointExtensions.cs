using Shared.Middleware;

namespace TransactionProcessor.Common;

using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel.Description;

[ExcludeFromCodeCoverage]
public static class ServiceEndpointExtensions{
    #region Methods

    public static void SetTraceLogging(this ServiceEndpoint serviceEndpoint, String clientName, RequestResponseMiddlewareLoggingConfig config){
        if (serviceEndpoint.EndpointBehaviors.Contains(typeof(ClientMessageLoggingBehavior)) == false){
            serviceEndpoint.EndpointBehaviors.Add(new ClientMessageLoggingBehavior(clientName, config));
        }
    }

    #endregion
}