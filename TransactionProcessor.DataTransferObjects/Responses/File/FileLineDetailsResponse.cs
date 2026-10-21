using System;

namespace TransactionProcessor.DataTransferObjects.Responses.File{
    /// <summary>Data transfer object for FileLineDetailsResponse.</summary>
    /// <example>{"fileLineData":"example","fileLineNumber":1,"status":"example","transaction":{"transactionId":"00000000-0000-0000-0000-000000000057","authCode":"VOUCHER-2026-001","isAuthorised":false,"isCompleted":false,"responseCode":"VOUCHER-2026-001","responseMessage":"Example message","transactionNumber":"EXAMPLE-001"}}</example>
    public class FileLineDetailsResponse{
        /// <summary>The file line data.</summary>
        public String FileLineData{ get; set; }
        /// <summary>The file line number.</summary>
        public Int32 FileLineNumber { get; set; }
        /// <summary>The status.</summary>
        public String Status{ get; set; }
        /// <summary>The transaction.</summary>
        public TransactionResponse Transaction{ get; set; }
    }
}
