using Microsoft.Extensions.Logging;
using Shared.Middleware;

namespace TransactionProcessor.Common;

using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Shared.Logger;

[ExcludeFromCodeCoverage]
internal sealed class ClientMessageLogger :
    IClientMessageInspector{
    #region Fields

    private readonly String ClientName;
    private readonly RequestResponseMiddlewareLoggingConfig Config;

    private enum LogType
    {
        Request,
        Response
    }

    #endregion

    #region Constructors

    public ClientMessageLogger(String clientName, RequestResponseMiddlewareLoggingConfig config)
    {
        this.ClientName = clientName;
        Config = config;
    }

    #endregion

    #region Methods

    public void AfterReceiveReply(ref Message reply, Object correlationState) => LogMessage(LogType.Request,$"Received SOAP reply from {this.ClientName}:\r\n{reply}");
    

    public Object BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        LogMessage(LogType.Response, $"Sending SOAP request to {this.ClientName}:\r\n{request}");
        return null;
    }


    private void LogMessage(LogType type, String message)
    {
        if (Config.LogRequests == false && type == LogType.Request)
            return;
        if (Config.LogResponses == false && type == LogType.Response)
            return;

        Action action = Config.LoggingLevel switch
        {
            LogLevel.Warning => () => Logger.LogWarning(message),
            LogLevel.Information => () => Logger.LogInformation(message),
            LogLevel.Debug => () => Logger.LogDebug(message),
            LogLevel.Trace => () => Logger.LogTrace(message),
            _ => () => Logger.LogInformation(message)
        };

        action();
    }

    #endregion
}