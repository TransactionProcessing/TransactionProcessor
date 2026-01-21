using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities;

[Table("estateoperator")]
public class EstateOperator
{
    #region Properties
        
    public Guid EstateId { get; set; }

    public Guid OperatorId { get; set; }

    public Boolean? IsDeleted { get; set; }

    #endregion
}