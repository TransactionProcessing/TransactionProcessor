using System;
using System.Collections.Generic;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace TransactionProcessor.DataTransferObjects.Responses.File{
    /// <summary>Data transfer object for FileDetailsResponse.</summary>
    /// <example>{"fileId":"00000000-0000-0000-0000-000000000048","fileReceivedDate":"2026-01-15T10:30:00Z","fileReceivedDateTime":"2026-01-15T10:30:00Z","fileLineDetails":[{"fileLineData":"example","fileLineNumber":1,"status":"example","transaction":{"transactionId":"00000000-0000-0000-0000-000000000057","authCode":"VOUCHER-2026-001","isAuthorised":false,"isCompleted":false,"responseCode":"VOUCHER-2026-001","responseMessage":"Example message","transactionNumber":"EXAMPLE-001"}}],"merchant":{"addresses":[{"addressId":"00000000-0000-0000-0000-000000000119","addressLine1":"123 Example Street","addressLine2":"123 Example Street","addressLine3":"123 Example Street","addressLine4":"123 Example Street","country":"US","postalCode":"10001","region":"Example City","town":"Example City"}],"contacts":[{"contactEmailAddress":"customer@example.com","contactId":"00000000-0000-0000-0000-000000000125","contactName":"Example Name","contactPhoneNumber":"+15550100"}],"estateId":"00000000-0000-0000-0000-000000000023","estateReportingId":1,"merchantId":"00000000-0000-0000-0000-000000000227","merchantReportingId":1,"merchantName":"Example Name","merchantReference":"REF-2026-001","nextStatementDate":"2026-01-15T10:30:00Z","operators":[{"name":"Example Name","operatorId":"00000000-0000-0000-0000-000000000253","merchantNumber":"EXAMPLE-001","terminalNumber":"EXAMPLE-001","isDeleted":false}],"settlementSchedule":0,"contracts":[{"contractId":"00000000-0000-0000-0000-000000000239","isDeleted":false,"contractProducts":[]}],"schedules":[{"year":1,"months":[{"month":1,"closedDays":[]}]}]}}</example>
    public class FileDetailsResponse{
        /// <summary>The file id.</summary>
        public Guid FileId{ get; set; }
        /// <summary>The file received date.</summary>

        public DateTime FileReceivedDate{ get; set; }
        /// <summary>The file received date time.</summary>

        public DateTime FileReceivedDateTime{ get; set; }
        /// <summary>The file line details.</summary>

        public List<FileLineDetailsResponse> FileLineDetails{ get; set; }
        /// <summary>The merchant.</summary>

        public MerchantResponse Merchant{ get; set; }
    }
}
