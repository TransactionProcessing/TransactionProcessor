using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("merchantdevice")]
    public class MerchantDevice
    {
        #region Properties

        public DateTime CreatedDateTime { get; set; }

        public Guid DeviceId { get; set; }

        public String DeviceIdentifier { get; set; }
        
        public Guid MerchantId { get; set; }
        public Boolean IsEnabled { get; set; }

        #endregion
    }
}