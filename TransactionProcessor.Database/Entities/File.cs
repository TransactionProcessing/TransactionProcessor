using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("file")]
    public class File
    {
        public Guid EstateId { get; set; }

        public Guid FileId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 FileReportingId{ get; set; }

        public Guid FileImportLogId { get; set; }

        public string FileLocation { get; set; }

        public Guid FileProfileId { get; set; }

        public DateTime FileReceivedDateTime { get; set; }

        public DateTime FileReceivedDate { get; set; }

        public Guid MerchantId { get; set; }

        public Guid UserId { get; set; }

        public Boolean IsCompleted { get; set; }
    }
}