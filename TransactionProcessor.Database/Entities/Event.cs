namespace TransactionProcessor.ProjectionEngine.Database.Database.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Event
    {
        [Column(TypeName = "date")]
        public DateOnly Date { get; set; }
        public Guid EventId { get; set; }
        public String Type { get; set; }
    }
}
