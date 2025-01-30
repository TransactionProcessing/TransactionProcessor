using System;

namespace TransactionProcessor.Models.Estate
{
    public class Operator() {
        public Guid OperatorId { get; set; }
        public String Name { get; set; }    
        public Boolean IsDeleted { get; set; }
    }
}