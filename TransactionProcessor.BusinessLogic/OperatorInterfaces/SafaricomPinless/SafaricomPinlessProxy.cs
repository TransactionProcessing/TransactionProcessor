namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.SafaricomPinless
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Serialization;
    using EstateManagement.DataTransferObjects.Responses;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.BusinessLogic.OperatorInterfaces.IOperatorProxy" />
    public class SafaricomPinlessProxy : IOperatorProxy
    {
        #region Fields

        private readonly HttpClient HttpClient;

        /// <summary>
        /// The safaricom configuration
        /// </summary>
        private readonly SafaricomConfiguration SafaricomConfiguration;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SafaricomPinlessProxy" /> class.
        /// </summary>
        /// <param name="safaricomConfiguration">The safaricom configuration.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public SafaricomPinlessProxy(SafaricomConfiguration safaricomConfiguration,
                                     HttpClient httpClient)
        {
            this.SafaricomConfiguration = safaricomConfiguration;
            this.HttpClient = httpClient;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes the sale message.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="merchant">The merchant.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="additionalTransactionMetadata">The additional transaction metadata.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Amount is a required field for this transaction type
        /// or
        /// CustomerAccountNumber is a required field for this transaction type
        /// or
        /// Error sending request [{requestUrl}] to Safaricom.  Status Code [{responseMessage.StatusCode}]</exception>
        public async Task<OperatorResponse> ProcessSaleMessage(String accessToken,
                                                               Guid transactionId,
                                                               String operatorIdentifier,
                                                               MerchantResponse merchant,
                                                               DateTime transactionDateTime,
                                                               String transactionReference,
                                                               Dictionary<String, String> additionalTransactionMetadata,
                                                               CancellationToken cancellationToken)
        {
            // Extract the required fields
            String transactionAmount = this.ExtractFieldFromMetadata("Amount", additionalTransactionMetadata);
            String customerMsisdn = this.ExtractFieldFromMetadata("CustomerAccountNumber", additionalTransactionMetadata);

            if (String.IsNullOrEmpty(transactionAmount))
            {
                throw new Exception("Amount is a required field for this transaction type");
            }

            if (String.IsNullOrEmpty(customerMsisdn))
            {
                throw new Exception("CustomerAccountNumber is a required field for this transaction type");
            }

            // Multiply amount before sending
            // Covert the transaction amount to Decimal and remove decimal places
            if (Decimal.TryParse(transactionAmount, out Decimal amountAsDecimal) == false)
            {
                throw new Exception("Transaction Amount is not a valid decimal value");
            }

            Decimal operatorTransactionAmount = amountAsDecimal * 100;
            
            String requestUrl = this.BuildRequest(transactionDateTime, transactionReference, customerMsisdn, operatorTransactionAmount);

            // Concatenate the request message
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(requestUrl));

            // Send the request to Safaricom
            HttpResponseMessage responseMessage = await this.HttpClient.SendAsync(requestMessage, cancellationToken);

            // Check the send was successful
            if (responseMessage.IsSuccessStatusCode == false)
            {
                throw new Exception($"Error sending request [{requestUrl}] to Safaricom.  Status Code [{responseMessage.StatusCode}]");
            }

            // Get the response
            String responseContent = await responseMessage.Content.ReadAsStringAsync();

            return this.CreateFrom(responseContent);
        }

        [ExcludeFromCodeCoverage]
        private String ExtractFieldFromMetadata(String fieldName,
                                              Dictionary<String, String> additionalTransactionMetadata)
        {
            // Create a case insensitive version of the dictionary
            Dictionary<String, String> caseInsensitiveDictionary = new Dictionary<String, String>(StringComparer.InvariantCultureIgnoreCase);
            foreach (KeyValuePair<String, String> keyValuePair in additionalTransactionMetadata)
            {
                caseInsensitiveDictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }

            if (caseInsensitiveDictionary.ContainsKey(fieldName))
            {
                return caseInsensitiveDictionary[fieldName];
            }
            else
            {
                return String.Empty;
            }

        }

        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="externalReference">The external reference.</param>
        /// <param name="customerMsisdn">The customer msisdn.</param>
        /// <param name="transactionAmount">The transaction amount.</param>
        /// <returns></returns>
        private String BuildRequest(DateTime transactionDateTime,
                                    String externalReference,
                                    String customerMsisdn,
                                    Decimal transactionAmount)
        {
            String requestUrl = $"{this.SafaricomConfiguration.Url}?VENDOR={this.SafaricomConfiguration.LoginId}&REQTYPE=EXRCTRFREQ&DATA=";

            StringBuilder xmlData = new StringBuilder();
            
            // Now build up the XML part of the message
            xmlData.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xmlData.Append("<ns0:COMMAND xmlns:ns0=\"http://safaricom.co.ke/Pinless/keyaccounts/\">");
            xmlData.Append("<ns0:TYPE>EXRCTRFREQ</ns0:TYPE>");
            xmlData.Append($"<ns0:DATE>{transactionDateTime:dd-MMM-yyyy}</ns0:DATE>");
            xmlData.Append("<ns0:EXTNWCODE>SA</ns0:EXTNWCODE>");
            xmlData.Append($"<ns0:MSISDN>{this.SafaricomConfiguration.MSISDN}</ns0:MSISDN>");
            xmlData.Append($"<ns0:PIN>{this.SafaricomConfiguration.Pin}</ns0:PIN>");
            xmlData.Append($"<ns0:LOGINID>{this.SafaricomConfiguration.LoginId}</ns0:LOGINID>");
            xmlData.Append($"<ns0:PASSWORD>{this.SafaricomConfiguration.Password}</ns0:PASSWORD>");
            xmlData.Append($"<ns0:EXTCODE>{this.SafaricomConfiguration.ExtCode}</ns0:EXTCODE>");
            xmlData.Append($"<ns0:EXTREFNUM>{externalReference}</ns0:EXTREFNUM>");
            xmlData.Append($"<ns0:MSISDN2>{customerMsisdn}</ns0:MSISDN2>");
            xmlData.Append($"<ns0:AMOUNT>{transactionAmount:G0}</ns0:AMOUNT>");
            xmlData.Append("<ns0:LANGUAGE1>0</ns0:LANGUAGE1>");
            xmlData.Append("<ns0:LANGUAGE2>0</ns0:LANGUAGE2>");
            xmlData.Append("<ns0:SELECTOR>0</ns0:SELECTOR>");
            xmlData.Append("</ns0:COMMAND>");

            return $"{requestUrl}{xmlData}";
        }

        /// <summary>
        /// Creates from.
        /// </summary>
        /// <param name="responseContent">Content of the response.</param>
        /// <returns></returns>
        private OperatorResponse CreateFrom(String responseContent)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(responseContent);
            XmlSerializer xs = new XmlSerializer(typeof(SafaricomResponse));
            SafaricomResponse cl = (SafaricomResponse)xs.Deserialize(new StringReader(doc.OuterXml));

            return new OperatorResponse
                   {
                       AuthorisationCode = "ABCD1234",
                       ResponseCode = cl.TransactionStatus.ToString(),
                       ResponseMessage = cl.Message,
                       IsSuccessful = cl.TransactionStatus == 200

                   };
        }

        #endregion
    }
}