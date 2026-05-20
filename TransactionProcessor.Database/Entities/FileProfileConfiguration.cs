using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.Database.Entities
{
    public class FileProfileConfiguration
    {
        public Guid FileProfileId { get; set; }
        public String Name { get; set; }
        public String ListeningDirectory { get; set; }
        public Guid RequestTypeId { get; set; }
        public Guid OperatorId { get; set; }
        public String LineTerminator { get; set; }
        public Guid FileFormatHandlerId { get; set; }
    }
}
