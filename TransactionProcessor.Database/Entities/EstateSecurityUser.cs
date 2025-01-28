using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("estatesecurityuser")]
    public class EstateSecurityUser
    {
        #region Properties
        public DateTime CreatedDateTime { get; set; }

        public String EmailAddress { get; set; }

        public Guid EstateId { get; set; }
        
        public Guid SecurityUserId { get; set; }
        
        #endregion
    }
}