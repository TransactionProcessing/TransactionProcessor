using System;

namespace TransactionProcessor.Models.Operator {
    public record Operator() {
        public Guid OperatorId { get; set; }
        public String Name { get; set; }
        public Boolean RequireCustomMerchantNumber { get; set; }
        public Boolean RequireCustomTerminalNumber { get; set; }
    }
}
