namespace TransactionProcessor.Common;

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Shared.Logger;

internal sealed class ClientMessageLogger :
    IClientMessageInspector{
    #region Fields

    private readonly String ClientName;

    #endregion

    #region Constructors

    public ClientMessageLogger(String clientName){
        this.ClientName = clientName;
    }

    #endregion

    #region Methods

    public void AfterReceiveReply(ref Message reply, Object correlationState){
        Logger.LogInformation($"Received SOAP reply from {this.ClientName}:\r\n{reply}");
    }

    public Object BeforeSendRequest(ref Message request, IClientChannel channel){
        Logger.LogInformation($"Sending SOAP request to {this.ClientName}:\r\n{request}");
        return null;
    }

    #endregion
}