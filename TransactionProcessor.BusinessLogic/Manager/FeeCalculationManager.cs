using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.BusinessLogic.Manager
{
    using System;
    using System.Collections.Generic;
    using Models;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.BusinessLogic.Manager.IFeeCalculationManager" />
    public class FeeCalculationManager : IFeeCalculationManager
    {
        #region Methods

        /// <summary>
        /// Calculates the fees.
        /// </summary>
        /// <param name="feeList">The fee list.</param>
        /// <param name="transactionAmount">The transaction amount.</param>
        /// <returns></returns>
        public List<CalculatedFee> CalculateFees(List<TransactionFeeToCalculate> feeList,
                                                 Decimal transactionAmount,
                                                 DateTime calculationDateTime = new DateTime())
        {
            List<CalculatedFee> calculatedFees = new List<CalculatedFee>();

            foreach (TransactionFeeToCalculate transactionFeeToCalculate in feeList)
            {
                if (transactionFeeToCalculate.CalculationType == CalculationType.Percentage)
                {
                    // percentage fee
                    Decimal feeValue = (transactionFeeToCalculate.Value / 100) * transactionAmount;
                    calculatedFees.Add(new CalculatedFee
                                       {
                                           CalculatedValue = feeValue,
                                           FeeType = transactionFeeToCalculate.FeeType,
                                           FeeCalculationType = transactionFeeToCalculate.CalculationType,
                                           FeeId = transactionFeeToCalculate.FeeId,
                                           FeeValue = transactionFeeToCalculate.Value,
                                           FeeCalculatedDateTime = calculationDateTime == DateTime.MinValue ? DateTime.Now : calculationDateTime
                                       });
                }
                else if (transactionFeeToCalculate.CalculationType == CalculationType.Fixed)
                {
                    // fixed value fee
                    calculatedFees.Add(new CalculatedFee
                                       {
                                           CalculatedValue = transactionFeeToCalculate.Value,
                                           FeeType = transactionFeeToCalculate.FeeType,
                                           FeeCalculationType = transactionFeeToCalculate.CalculationType,
                                           FeeId = transactionFeeToCalculate.FeeId,
                                           FeeValue = transactionFeeToCalculate.Value,
                                           FeeCalculatedDateTime = calculationDateTime == DateTime.MinValue ? DateTime.Now : calculationDateTime
                    });
                }
            }

            return calculatedFees;
        }

        #endregion
    }
}