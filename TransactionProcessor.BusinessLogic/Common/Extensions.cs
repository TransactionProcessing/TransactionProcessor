using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.Common
{
    using System.Diagnostics.CodeAnalysis;

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
        public static T ExtractFieldFromMetadata<T>(this Dictionary<String, String> additionalTransactionMetadata, String fieldName)
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
                return (T)Convert.ChangeType(fieldData, typeof(T));
            }
            else
            {
                return default(T);
            }
        }
    }
}
