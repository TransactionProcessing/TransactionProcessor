using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.Common
{
    using Google.Protobuf.WellKnownTypes;
    using System.Diagnostics.CodeAnalysis;
    using Type = System.Type;

    public static class Helpers
    {
        public static Guid CalculateSettlementAggregateId(DateTime settlementDate,
                                                    Guid estateId)
        {
            Guid aggregateId = GuidCalculator.Combine(estateId, settlementDate.ToGuid());
            return aggregateId;
        }
    }

    public static class GuidCalculator
    {
        #region Methods

        /// <summary>
        /// Combines the specified GUIDs into a new GUID.
        /// </summary>
        /// <param name="firstGuid">The first unique identifier.</param>
        /// <param name="secondGuid">The second unique identifier.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>Guid.</returns>
        public static Guid Combine(Guid firstGuid,
                                   Guid secondGuid,
                                   Byte offset)
        {
            Byte[] firstAsBytes = firstGuid.ToByteArray();
            Byte[] secondAsBytes = secondGuid.ToByteArray();

            Byte[] newBytes = new Byte[16];

            for (Int32 i = 0; i < 16; i++)
            {
                // Add and truncate any overflow
                newBytes[i] = (Byte)(firstAsBytes[i] + secondAsBytes[i] + offset);
            }

            return new Guid(newBytes);
        }

        /// <summary>
        /// Combines the specified GUIDs into a new GUID.
        /// </summary>
        /// <param name="firstGuid">The first unique identifier.</param>
        /// <param name="secondGuid">The second unique identifier.</param>
        /// <returns>Guid.</returns>
        public static Guid Combine(Guid firstGuid,
                                   Guid secondGuid)
        {
            return GuidCalculator.Combine(firstGuid,
                                          secondGuid,
                                          0);
        }

        /// <summary>
        /// Combines the specified first unique identifier.
        /// </summary>
        /// <param name="firstGuid">The first unique identifier.</param>
        /// <param name="secondGuid">The second unique identifier.</param>
        /// <param name="thirdGuid">The third unique identifier.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>Guid.</returns>
        public static Guid Combine(Guid firstGuid,
                                   Guid secondGuid,
                                   Guid thirdGuid,
                                   Byte offset)
        {
            Byte[] firstAsBytes = firstGuid.ToByteArray();
            Byte[] secondAsBytes = secondGuid.ToByteArray();
            Byte[] thirdAsBytes = thirdGuid.ToByteArray();

            Byte[] newBytes = new Byte[16];

            for (Int32 i = 0; i < 16; i++)
            {
                // Add and truncate any overflow
                newBytes[i] = (Byte)(firstAsBytes[i] + secondAsBytes[i] + thirdAsBytes[i] + offset);
            }

            return new Guid(newBytes);
        }

        /// <summary>
        /// Combines the specified first unique identifier.
        /// </summary>
        /// <param name="firstGuid">The first unique identifier.</param>
        /// <param name="secondGuid">The second unique identifier.</param>
        /// <param name="thirdGuid">The third unique identifier.</param>
        /// <returns>Guid.</returns>
        public static Guid Combine(Guid firstGuid,
                                   Guid secondGuid,
                                   Guid thirdGuid)
        {
            return GuidCalculator.Combine(firstGuid,
                                          secondGuid,
                                          thirdGuid,
                                          0);
        }

        #endregion
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
