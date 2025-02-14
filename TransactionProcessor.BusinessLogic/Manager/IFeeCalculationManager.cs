namespace TransactionProcessor.BusinessLogic.Manager
{
    using System;
    using System.Collections.Generic;
    using Models;

    /// <summary>
    /// 
    /// </summary>
    public interface IFeeCalculationManager
    {
        #region Methods

        /// <summary>
        /// Calculates the fees.
        /// </summary>
        /// <param name="feeList">The fee list.</param>
        /// <param name="transactionAmount">The transaction amount.</param>
        /// <param name="calculationDateTime">The calculation date time.</param>
        /// <returns></returns>
        List<CalculatedFee> CalculateFees(List<TransactionFeeToCalculate> feeList,
                                          Decimal transactionAmount,
                                          DateTime calculationDateTime = new DateTime());

        #endregion
    }
}