﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Results;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Common
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.General;
    using Shared.Logger;
    using Type = System.Type;

    [ExcludeFromCodeCoverage]
    public static class Helpers
    {
        public static Guid CalculateSettlementAggregateId(DateTime settlementDate,
                                                    Guid merchantId, 
                                                    Guid estateId)
        {
            Guid aggregateId = GuidCalculator.Combine(estateId, merchantId, settlementDate.ToGuid());
            return aggregateId;
        }

        public static async Task<Result<TokenResponse>> GetToken(TokenResponse currentToken, ISecurityServiceClient securityServiceClient, CancellationToken cancellationToken)
        {
            // Get a token to talk to the estate service
            String clientId = ConfigurationReader.GetValue("AppSettings", "ClientId");
            String clientSecret = ConfigurationReader.GetValue("AppSettings", "ClientSecret");
            Logger.LogDebug($"Client Id is {clientId}");
            Logger.LogDebug($"Client Secret is {clientSecret}");

            if (currentToken == null)
            {
                Result<TokenResponse> tokenResult= await securityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
                if (tokenResult.IsFailed)
                    return ResultHelpers.CreateFailure(tokenResult);
                TokenResponse token =tokenResult.Data;
                Logger.LogDebug($"Token is {token.AccessToken}");
                return Result.Success(token);
            }

            if (currentToken.Expires.UtcDateTime.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(2))
            {
                Logger.LogDebug($"Token is about to expire at {currentToken.Expires.DateTime:O}");
                Result<TokenResponse> tokenResult = await securityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
                if (tokenResult.IsFailed)
                    return ResultHelpers.CreateFailure(tokenResult);
                TokenResponse token = tokenResult.Data;
                Logger.LogDebug($"Token is {token.AccessToken}");
                return Result.Success(token);
            }

            return Result.Success(currentToken);
        }
    }
    
    public static class Extensions
    {
        /// <summary>
        /// Extracts the field from metadata.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="additionalTransactionMetadata">The additional transaction metadata.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public static T ExtractFieldFromMetadata<T>(this Dictionary<String, String> additionalTransactionMetadata,
                                                    String fieldName)
        {
            // Create a case insensitive version of the dictionary
            Dictionary<String, String> caseInsensitiveDictionary = new Dictionary<String, String>(StringComparer.InvariantCultureIgnoreCase);
            foreach (KeyValuePair<String, String> keyValuePair in additionalTransactionMetadata)
            {
                caseInsensitiveDictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }

            if (caseInsensitiveDictionary.ContainsKey(fieldName))
            {
                String fieldData = caseInsensitiveDictionary[fieldName];
                Type t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                Object safeValue = (fieldData == null) ? null : Convert.ChangeType(fieldData, t);
                return (T)safeValue;
            }

            return default(T);
        }

        public static Guid ToGuid(this DateTime dt)
        {
            var bytes = BitConverter.GetBytes(dt.Ticks);

            Array.Resize(ref bytes, 16);

            return new Guid(bytes);
        }

        public static DateTime ToDateTime(this Guid guid)
        {
            var bytes = guid.ToByteArray();

            Array.Resize(ref bytes, 8);

            return new DateTime(BitConverter.ToInt64(bytes));
        }
    }
}
