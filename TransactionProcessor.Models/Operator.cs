using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Models
{
    [ExcludeFromCodeCoverage]
    public class EstateOperator
    {
        #region Properties
        
        /// <summary>
        /// Gets the operator identifier.
        /// </summary>
        /// <value>
        /// The operator identifier.
        /// </value>
        public Guid OperatorId { get; set; }
        public Boolean IsDeleted { get; set; }
        public String Name{ get; set; }

        #endregion
    }
}
