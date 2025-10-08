using System.Diagnostics.CodeAnalysis;
using Shared.Middleware;

namespace TransactionProcessor.Common;

using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

[ExcludeFromCodeCoverage]
internal sealed class ClientMessageLoggingBehavior :
    IEndpointBehavior{
    #region Fields

    private readonly String ClientName;
    private readonly RequestResponseMiddlewareLoggingConfig Config;

    #endregion

    #region Constructors

    public ClientMessageLoggingBehavior(String clientName, RequestResponseMiddlewareLoggingConfig config)
    {
        this.ClientName = clientName;
        Config = config;
    }

    #endregion

    #region Methods

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters){
        // Not needed for client
    }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime){
        clientRuntime.ClientMessageInspectors.Add(new ClientMessageLogger(this.ClientName, this.Config));
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher){
        // Not needed for client
    }

    public void Validate(ServiceEndpoint endpoint){
        // Not needed
    }

    #endregion
}