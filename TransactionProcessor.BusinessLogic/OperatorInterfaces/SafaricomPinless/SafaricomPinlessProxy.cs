namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.SafaricomPinless
{
    using System;
    using System.Collections.Generic;
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
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="merchant">The merchant.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="additionalTransactionMetadata">The additional transaction metadata.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<OperatorResponse> ProcessSaleMessage(Guid transactionId,
                                                               MerchantResponse merchant,
                                                               DateTime transactionDateTime,
                                                               Dictionary<String, String> additionalTransactionMetadata,
                                                               CancellationToken cancellationToken)
        {
            String requestUrl = this.BuildRequest(transactionDateTime, "123456789", "123456789", "1000");

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
                                    String transactionAmount)
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
            xmlData.Append($"<ns0:AMOUNT>{transactionAmount}</ns0:AMOUNT>");
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