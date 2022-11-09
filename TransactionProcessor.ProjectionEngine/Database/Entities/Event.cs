using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.ProjectionEngine.Database.Entities
{
    public class Event
    {
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
        public Guid EventId { get; set; }
        public String Type { get; set; }
    }
}
