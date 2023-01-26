namespace TransactionProcessor.Common;

using System;
using System.ServiceModel.Description;

public static class ServiceEndpointExtensions{
    #region Methods

    public static void SetTraceLogging(this ServiceEndpoint serviceEndpoint, String clientName){
        if (serviceEndpoint.EndpointBehaviors.Contains(typeof(ClientMessageLoggingBehavior)) == false){
            serviceEndpoint.EndpointBehaviors.Add(new ClientMessageLoggingBehavior(clientName));
        }
    }

    #endregion
}