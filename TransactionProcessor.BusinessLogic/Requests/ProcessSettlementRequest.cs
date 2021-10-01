using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.Requests
{
    using MediatR;
    using Models;

    public class ProcessSettlementRequest : IRequest<ProcessSettlementResponse>
    {
        #region Constructors

        private ProcessSettlementRequest(DateTime settlementDate, Guid estateId)
        {
            this.EstateId = estateId;
            this.SettlementDate = settlementDate;
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; }

        
        public DateTime SettlementDate { get; }
        
        #endregion

        #region Methods

        /// <summary>
        /// Creates the specified estate identifier.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="transactionType">Type of the transaction.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <returns></returns>
        public static ProcessSettlementRequest Create(DateTime settlementDate,
                                                      Guid estateId)
        {
            return new ProcessSettlementRequest(settlementDate, estateId);
        }

        #endregion
    }
}
