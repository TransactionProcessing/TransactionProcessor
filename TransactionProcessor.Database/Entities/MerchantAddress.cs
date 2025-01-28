using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("merchantaddress")]
    public class MerchantAddress
    {
        #region Properties

        public Guid AddressId { get; set; }

        public String AddressLine1 { get; set; }

        public String? AddressLine2 { get; set; }

        public String? AddressLine3 { get; set; }

        public String? AddressLine4 { get; set; }

        public String? Country { get; set; }

        public DateTime CreatedDateTime { get; set; }
        
        public Guid MerchantId { get; set; }

        public String? PostalCode { get; set; }

        public String? Region { get; set; }

        public String? Town { get; set; }

        #endregion
    }
}