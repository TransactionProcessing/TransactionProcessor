using System;

namespace TransactionProcessor.DataTransferObjects.Responses.File{
    public class FileLineDetailsResponse{
        public String FileLineData{ get; set; }
        public Int32 FileLineNumber { get; set; }
        public String Status{ get; set; }
        public TransactionResponse Transaction{ get; set; }
    }
}