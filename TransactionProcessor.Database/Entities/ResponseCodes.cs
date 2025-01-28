using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    [Table("responsecodes")]
    public class ResponseCodes
    {
        public Int32 ResponseCode { get; set; }

        public String Description { get; set; }
    }
}
