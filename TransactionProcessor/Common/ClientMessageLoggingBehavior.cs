namespace TransactionProcessor.Common;

using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

internal sealed class ClientMessageLoggingBehavior :
    IEndpointBehavior{
    #region Fields

    private readonly String ClientName;

    #endregion

    #region Constructors

    public ClientMessageLoggingBehavior(String clientName){
        this.ClientName = clientName;
    }

    #endregion

    #region Methods

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters){
    }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime){
        clientRuntime.ClientMessageInspectors.Add(new ClientMessageLogger(this.ClientName));
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher){
    }

    public void Validate(ServiceEndpoint endpoint){
    }

    #endregion
}