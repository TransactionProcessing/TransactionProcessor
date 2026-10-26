using System.Net;
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

    public void AfterReceiveReply(ref Message reply, Object correlationState) {

        if (reply.Properties.TryGetValue(
                HttpResponseMessageProperty.Name,
                out object httpResponseObj)) {
            HttpResponseMessageProperty httpResponse = (HttpResponseMessageProperty)httpResponseObj;

            HttpStatusCode statusCode = httpResponse.StatusCode;
            if (statusCode != HttpStatusCode.OK) {
                Logger.LogWarning(
                    "Received failed SOAP reply from {ClientName}:\r\n" + reply,
                    this.ClientName);
                return;
            }

            LogMessage(LogType.Response, "Received SOAP reply from {ClientName}:\r\n" + reply, this.ClientName);
        }
    }


    public Object BeforeSendRequest(ref Message request, IClientChannel channel) {
        LogMessage(LogType.Request, $"Sending SOAP request to {ClientName}:\r\n" + request, this.ClientName);
        return null;
    }


    private void LogMessage(LogType type, String messageTemplate, params Object[]? args)
    {
        if (Config.LogRequests == false && type == LogType.Request)
            return;
        if (Config.LogResponses == false && type == LogType.Response)
            return;

        Action action = Config.LoggingLevel switch
        {
            LogLevel.Warning => () => Logger.LogWarning(messageTemplate, args),
            LogLevel.Information => () => Logger.LogInformation(messageTemplate, args),
            LogLevel.Debug => () => Logger.LogDebug(messageTemplate, args),
            LogLevel.Trace => () => Logger.LogTrace(messageTemplate, args),
            _ => () => Logger.LogInformation(messageTemplate, args)
        };

        action();
    }

    #endregion
}