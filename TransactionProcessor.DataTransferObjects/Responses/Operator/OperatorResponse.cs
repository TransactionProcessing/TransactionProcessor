using System;

namespace TransactionProcessor.DataTransferObjects.Responses.Operator
{
    public class OperatorResponse
    {
        public String Name { get; set; }

        public Guid OperatorId { get; set; }

        public Boolean RequireCustomMerchantNumber { get; set; }

        public Boolean RequireCustomTerminalNumber { get; set; }


    }
}
