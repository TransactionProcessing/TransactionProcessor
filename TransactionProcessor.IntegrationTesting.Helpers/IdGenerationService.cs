using TransactionProcessing.Shared.Serialisation;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace TransactionProcessor.IntegrationTesting.Helpers
{
    [ExcludeFromCodeCoverage]
    public class IdGenerationService
    {
        internal delegate Guid GenerateUniqueIdFromObject(Object payload);

        internal delegate Guid GenerateUniqueIdFromString(String payload);

        private static readonly GenerateUniqueIdFromObject GenerateUniqueId =
            data => IdGenerationService.GenerateGuidFromString(StringSerialiser.Serialise(data));

        private static readonly GenerateUniqueIdFromString GenerateGuidFromString = uniqueKey => {
                                                                                        using SHA256 sha256Hash = SHA256.Create();
                                                                                        //Generate hash from the key
                                                                                        Byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(uniqueKey));

                                                                                        Byte[] j = bytes.Skip(Math.Max(0, bytes.Count() - 16)).ToArray(); //Take last 16

                                                                                        //Create our Guid.
                                                                                        return new Guid(j);
                                                                                    };

        public static Guid GenerateEventId(Object o) => IdGenerationService.GenerateUniqueId(o);

        public static Guid GenerateFloatActivityAggregateId(Guid estateId, Guid floatId, DateTimeOffset dateTime) =>
            IdGenerationService.GenerateUniqueId(new
            {
                estateId,
                floatId,
                dateTime
            });

        public static Guid GenerateMerchantStatementForDateAggregateId(Guid estateId, Guid merchantId, DateTimeOffset nextStatementDateTime, DateTimeOffset activityDateTime) =>
            IdGenerationService.GenerateUniqueId(new
            {
                estateId,
                merchantId,
                nextStatementDate = nextStatementDateTime.Date,
                activityDate = activityDateTime.Date
            });

        public static Guid GenerateMerchantStatementAggregateId(Guid estateId, Guid merchantId, DateTimeOffset nextStatementDateTime) =>
            IdGenerationService.GenerateUniqueId(new
            {
                estateId,
                merchantId,
                nextStatementDate = nextStatementDateTime.Date
            });

        public static Guid GenerateMerchantScheduleAggregateId(Guid estateId, Guid merchantId, Int32 year) =>
            IdGenerationService.GenerateUniqueId(new
            {
                estateId,
                merchantId,
                year
            });

        public static Guid GenerateContractId(Guid estateId, Guid operatorId, String contractName) {
            String normalisedContractName = contractName.ToLowerInvariant().Replace(" ", String.Empty);

            return IdGenerationService.GenerateUniqueId(new { estateId, operatorId, normalisedContractName });
        }

        public static Guid GenerateOperatorId(Guid estateId, String operatorName) {
            String normalisedOperatorName = operatorName.ToLowerInvariant().Replace(" ", String.Empty);

            return IdGenerationService.GenerateUniqueId(new { estateId, normalisedOperatorName });
        }

        public static Guid GenerateSettlementId(Guid estateId, Guid merchantId, DateTimeOffset nextSettlementDate) {
            return IdGenerationService.GenerateUniqueId(new { estateId, merchantId, timeInMilliseconds = nextSettlementDate.ToUnixTimeSeconds() });
        }

        public static Guid GenerateEstateId(String estateName) {
            String normalisedEstateName = estateName.ToLowerInvariant().Replace(" ", String.Empty);

            return IdGenerationService.GenerateUniqueId(new { normalisedEstateName });
        }

        public static Guid GenerateMerchantId(Guid estateId, String merchantName) {
            String normalisedMerchantName = merchantName.ToLowerInvariant().Replace(" ", String.Empty);

            return IdGenerationService.GenerateUniqueId(new { estateId, normalisedMerchantName });
        }

        public static Guid GenerateVoucherId(Guid estateId, Guid operatorId, Guid transactionId) {
            return IdGenerationService.GenerateUniqueId(new { estateId, operatorId, transactionId });
        }

        public static Guid GenerateReconciliationId(Guid estateId, Guid merchantId, String deviceIdentifier, DateTimeOffset transactionDateTime) {
            return IdGenerationService.GenerateUniqueId(new { estateId, merchantId, deviceIdentifier, transactionDateTime });
        }

        public static Guid GenerateTransactionId(Guid estateId, Guid merchantId, String deviceIdentifier, DateTimeOffset transactionDateTime, String transactionNumber) {
            return IdGenerationService.GenerateUniqueId(new { estateId, merchantId, deviceIdentifier, transactionDateTime, transactionNumber });
        }
    }
}
