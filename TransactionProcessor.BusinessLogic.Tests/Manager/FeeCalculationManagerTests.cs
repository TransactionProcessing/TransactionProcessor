using System;
using System.Collections.Generic;
using System.Text;
using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.BusinessLogic.Tests.Manager
{
    using System.Linq;
    using BusinessLogic.Manager;
    using EventHandling;
    using Models;
    using Shouldly;
    using Xunit;

    public class FeeCalculationManagerTests
    {
        [Fact]
        public void FeeCalculationManager_CalculateFees_SingleFixedFee_ServiceFee_FeesAreCalculated()
        {
            IFeeCalculationManager manager = new FeeCalculationManager();

            List<TransactionFeeToCalculate> feesList = new List<TransactionFeeToCalculate>
                                                       {
                                                           FeeCalculationManagerTestData.FixedServiceFee5
                                                       };

            List<CalculatedFee> calculatedFees = manager.CalculateFees(feesList, FeeCalculationManagerTestData.TransactionAmount100);

            calculatedFees.ShouldHaveSingleItem();
            CalculatedFee calculatedFee = calculatedFees.Single();
            calculatedFee.CalculatedValue.ShouldBe(5.0m);
            calculatedFee.FeeType.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee5.FeeType);
            calculatedFee.FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee5.CalculationType);
            calculatedFee.FeeId.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee5.FeeId);
            calculatedFee.FeeValue.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee5.Value);
        }

        [Fact]
        public void FeeCalculationManager_CalculateFees_SingleFixedFee_ServiceFee_WithCalculationDate_FeesAreCalculated()
        {
            IFeeCalculationManager manager = new FeeCalculationManager();

            List<TransactionFeeToCalculate> feesList = new List<TransactionFeeToCalculate>
            {
                FeeCalculationManagerTestData.FixedServiceFee5
            };

            List<CalculatedFee> calculatedFees = manager.CalculateFees(feesList, FeeCalculationManagerTestData.TransactionAmount100, DateTime.Now);

            calculatedFees.ShouldHaveSingleItem();
            CalculatedFee calculatedFee = calculatedFees.Single();
            calculatedFee.CalculatedValue.ShouldBe(5.0m);
            calculatedFee.FeeType.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee5.FeeType);
            calculatedFee.FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee5.CalculationType);
            calculatedFee.FeeId.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee5.FeeId);
            calculatedFee.FeeValue.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee5.Value);
        }

        [Fact]
        public void FeeCalculationManager_CalculateFees_MultipleFixedFees_ServiceFee_FeesAreCalculated()
        {
            IFeeCalculationManager manager = new FeeCalculationManager();

            List<TransactionFeeToCalculate> feesList = new List<TransactionFeeToCalculate>
                                                       {
                                                           FeeCalculationManagerTestData.FixedServiceFee5,
                                                           FeeCalculationManagerTestData.FixedServiceFee2
                                                       };

            List<CalculatedFee> calculatedFees = manager.CalculateFees(feesList, FeeCalculationManagerTestData.TransactionAmount100);

            calculatedFees.Count.ShouldBe(2);
            calculatedFees[0].CalculatedValue.ShouldBe(5.0m);
            calculatedFees[0].FeeType.ShouldBe(FeeType.ServiceProvider);
            calculatedFees[0].FeeType.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee5.FeeType);
            calculatedFees[0].FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee5.CalculationType);
            calculatedFees[0].FeeId.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee5.FeeId);
            calculatedFees[0].FeeValue.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee5.Value);
            calculatedFees[1].CalculatedValue.ShouldBe(2.0m);
            calculatedFees[1].FeeType.ShouldBe(FeeType.ServiceProvider);
            calculatedFees[1].FeeType.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee2.FeeType);
            calculatedFees[1].FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee2.CalculationType);
            calculatedFees[1].FeeId.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee2.FeeId);
            calculatedFees[1].FeeValue.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee2.Value);
        }

        [Fact]
        public void FeeCalculationManager_CalculateFees_SinglePercentageFee_ServiceFee_FeesAreCalculated()
        {
            IFeeCalculationManager manager = new FeeCalculationManager();

            List<TransactionFeeToCalculate> feesList = new List<TransactionFeeToCalculate>
                                                       {
                                                           FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent
                                                       };

            List<CalculatedFee> calculatedFees = manager.CalculateFees(feesList, FeeCalculationManagerTestData.TransactionAmount100);

            calculatedFees.ShouldHaveSingleItem();
            CalculatedFee calculatedFee = calculatedFees.Single();
            calculatedFee.CalculatedValue.ShouldBe(0.25m);
            calculatedFee.FeeType.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.FeeType);
            calculatedFee.FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.CalculationType);
            calculatedFee.FeeId.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.FeeId);
            calculatedFee.FeeValue.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.Value);
        }

        [Fact]
        public void FeeCalculationManager_CalculateFees_SinglePercentageFee_ServiceFee_WithFixedDate_FeesAreCalculated()
        {
            IFeeCalculationManager manager = new FeeCalculationManager();

            List<TransactionFeeToCalculate> feesList = new List<TransactionFeeToCalculate>
            {
                FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent
            };

            List<CalculatedFee> calculatedFees = manager.CalculateFees(feesList, FeeCalculationManagerTestData.TransactionAmount100, DateTime.Now);

            calculatedFees.ShouldHaveSingleItem();
            CalculatedFee calculatedFee = calculatedFees.Single();
            calculatedFee.CalculatedValue.ShouldBe(0.25m);
            calculatedFee.FeeType.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.FeeType);
            calculatedFee.FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.CalculationType);
            calculatedFee.FeeId.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.FeeId);
            calculatedFee.FeeValue.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.Value);
        }

        [Fact]
        public void FeeCalculationManager_CalculateFees_MultiplePercentageFees_ServiceFee_FeesAreCalculated()
        {
            IFeeCalculationManager manager = new FeeCalculationManager();

            List<TransactionFeeToCalculate> feesList = new List<TransactionFeeToCalculate>
                                                       {
                                                           FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent,
                                                           FeeCalculationManagerTestData.PercentageServiceFeeThreeQuarterPercent
                                                       };

            List<CalculatedFee> calculatedFees = manager.CalculateFees(feesList, FeeCalculationManagerTestData.TransactionAmount100);
            
            calculatedFees[0].CalculatedValue.ShouldBe(0.25m);
            calculatedFees[0].FeeType.ShouldBe(FeeType.ServiceProvider);
            calculatedFees[0].FeeType.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.FeeType);
            calculatedFees[0].FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.CalculationType);
            calculatedFees[0].FeeId.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.FeeId);
            calculatedFees[0].FeeValue.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.Value);
            calculatedFees[1].CalculatedValue.ShouldBe(0.75m);
            calculatedFees[1].FeeType.ShouldBe(FeeType.ServiceProvider);
            calculatedFees[1].FeeType.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeThreeQuarterPercent.FeeType);
            calculatedFees[1].FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeThreeQuarterPercent.CalculationType);
            calculatedFees[1].FeeId.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeThreeQuarterPercent.FeeId);
            calculatedFees[1].FeeValue.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeThreeQuarterPercent.Value);
        }

        [Fact]
        public void FeeCalculationManager_CalculateFees_MultipleFeesFixedAndPercentage_ServiceFee_FeesAreCalculated()
        {
            IFeeCalculationManager manager = new FeeCalculationManager();

            List<TransactionFeeToCalculate> feesList = new List<TransactionFeeToCalculate>
                                                       {
                                                           FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent,
                                                           FeeCalculationManagerTestData.FixedServiceFee2
                                                       };

            List<CalculatedFee> calculatedFees = manager.CalculateFees(feesList, FeeCalculationManagerTestData.TransactionAmount100);

            calculatedFees[0].CalculatedValue.ShouldBe(0.25m);
            calculatedFees[0].FeeType.ShouldBe(FeeType.ServiceProvider);
            calculatedFees[0].FeeType.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.FeeType);
            calculatedFees[0].FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.CalculationType);
            calculatedFees[0].FeeId.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.FeeId);
            calculatedFees[0].FeeValue.ShouldBe(FeeCalculationManagerTestData.PercentageServiceFeeQuarterPercent.Value);
            calculatedFees[1].CalculatedValue.ShouldBe(2.0m);
            calculatedFees[1].FeeType.ShouldBe(FeeType.ServiceProvider);
            calculatedFees[1].FeeType.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee2.FeeType);
            calculatedFees[1].FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee2.CalculationType);
            calculatedFees[1].FeeId.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee2.FeeId);
            calculatedFees[1].FeeValue.ShouldBe(FeeCalculationManagerTestData.FixedServiceFee2.Value);
        }

        [Fact]
        public void FeeCalculationManager_CalculateFees_SingleFixedFee_MerchantFee_FeesAreCalculated()
        {
            IFeeCalculationManager manager = new FeeCalculationManager();

            List<TransactionFeeToCalculate> feesList = new List<TransactionFeeToCalculate>
                                                       {
                                                           FeeCalculationManagerTestData.FixedMerchantFee5
                                                       };

            List<CalculatedFee> calculatedFees = manager.CalculateFees(feesList, FeeCalculationManagerTestData.TransactionAmount100);

            calculatedFees.ShouldHaveSingleItem();
            CalculatedFee calculatedFee = calculatedFees.Single();
            calculatedFee.CalculatedValue.ShouldBe(5.0m);
            calculatedFee.FeeType.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee5.FeeType);
            calculatedFee.FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee5.CalculationType);
            calculatedFee.FeeId.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee5.FeeId);
            calculatedFee.FeeValue.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee5.Value);
        }

        [Fact]
        public void FeeCalculationManager_CalculateFees_MultipleFixedFees_MerchantFee_FeesAreCalculated()
        {
            IFeeCalculationManager manager = new FeeCalculationManager();

            List<TransactionFeeToCalculate> feesList = new List<TransactionFeeToCalculate>
                                                       {
                                                           FeeCalculationManagerTestData.FixedMerchantFee5,
                                                           FeeCalculationManagerTestData.FixedMerchantFee2
                                                       };

            List<CalculatedFee> calculatedFees = manager.CalculateFees(feesList, FeeCalculationManagerTestData.TransactionAmount100);

            calculatedFees.Count.ShouldBe(2);
            calculatedFees[0].CalculatedValue.ShouldBe(5.0m);
            calculatedFees[0].FeeType.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee5.FeeType);
            calculatedFees[0].FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee5.CalculationType);
            calculatedFees[0].FeeId.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee5.FeeId);
            calculatedFees[0].FeeValue.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee5.Value);
            calculatedFees[1].CalculatedValue.ShouldBe(2.0m);
            calculatedFees[1].FeeType.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee2.FeeType);
            calculatedFees[1].FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee2.CalculationType);
            calculatedFees[1].FeeId.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee2.FeeId);
            calculatedFees[1].FeeValue.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee2.Value);
        }

        [Fact]
        public void FeeCalculationManager_CalculateFees_SinglePercentageFee_MerchantFee_FeesAreCalculated()
        {
            IFeeCalculationManager manager = new FeeCalculationManager();

            List<TransactionFeeToCalculate> feesList = new List<TransactionFeeToCalculate>
                                                       {
                                                           FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent
                                                       };

            List<CalculatedFee> calculatedFees = manager.CalculateFees(feesList, FeeCalculationManagerTestData.TransactionAmount100);

            calculatedFees.ShouldHaveSingleItem();
            CalculatedFee calculatedFee = calculatedFees.Single();
            calculatedFee.CalculatedValue.ShouldBe(0.25m);
            calculatedFee.FeeType.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent.FeeType);
            calculatedFee.FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent.CalculationType);
            calculatedFee.FeeId.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent.FeeId);
            calculatedFee.FeeValue.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent.Value);
        }

        [Fact]
        public void FeeCalculationManager_CalculateFees_MultiplePercentageFees_MerchantFee_FeesAreCalculated()
        {
            IFeeCalculationManager manager = new FeeCalculationManager();

            List<TransactionFeeToCalculate> feesList = new List<TransactionFeeToCalculate>
                                                       {
                                                           FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent,
                                                           FeeCalculationManagerTestData.PercentageMerchantFeeThreeQuarterPercent
                                                       };

            List<CalculatedFee> calculatedFees = manager.CalculateFees(feesList, FeeCalculationManagerTestData.TransactionAmount100);

            calculatedFees.Count.ShouldBe(2);
            calculatedFees[0].CalculatedValue.ShouldBe(0.25m);
            calculatedFees[0].FeeType.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent.FeeType);
            calculatedFees[0].FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent.CalculationType);
            calculatedFees[0].FeeId.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent.FeeId);
            calculatedFees[0].FeeValue.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent.Value);
            calculatedFees[1].CalculatedValue.ShouldBe(0.75m);
            calculatedFees[1].FeeType.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeThreeQuarterPercent.FeeType);
            calculatedFees[1].FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeThreeQuarterPercent.CalculationType);
            calculatedFees[1].FeeId.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeThreeQuarterPercent.FeeId);
            calculatedFees[1].FeeValue.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeThreeQuarterPercent.Value);
        }

        [Fact]
        public void FeeCalculationManager_CalculateFees_MultipleFeesFixedAndPercentage_MerchantFee_FeesAreCalculated()
        {
            IFeeCalculationManager manager = new FeeCalculationManager();

            List<TransactionFeeToCalculate> feesList = new List<TransactionFeeToCalculate>
                                                       {
                                                           FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent,
                                                           FeeCalculationManagerTestData.FixedMerchantFee2
                                                       };

            List<CalculatedFee> calculatedFees = manager.CalculateFees(feesList, FeeCalculationManagerTestData.TransactionAmount100);

            calculatedFees.Count.ShouldBe(2);
            calculatedFees[0].CalculatedValue.ShouldBe(0.25m);
            calculatedFees[0].FeeType.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent.FeeType);
            calculatedFees[0].FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent.CalculationType);
            calculatedFees[0].FeeId.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent.FeeId);
            calculatedFees[0].FeeValue.ShouldBe(FeeCalculationManagerTestData.PercentageMerchantFeeQuarterPercent.Value);
            calculatedFees[1].CalculatedValue.ShouldBe(2.0m);
            calculatedFees[1].FeeType.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee2.FeeType);
            calculatedFees[1].FeeCalculationType.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee2.CalculationType);
            calculatedFees[1].FeeId.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee2.FeeId);
            calculatedFees[1].FeeValue.ShouldBe(FeeCalculationManagerTestData.FixedMerchantFee2.Value);
        }
    }

    public static class FeeCalculationManagerTestData
    {
        public static Decimal TransactionAmount100 = 100.00m;

        public static TransactionFeeToCalculate FixedServiceFee5 = new TransactionFeeToCalculate
                                                                   {
                                                                       Value = 5.0m,
                                                                       CalculationType = CalculationType.Fixed,
                                                                       FeeType = FeeType.ServiceProvider,
                                                                       FeeId = Guid.Parse("BB321B3A-CA36-40DD-8E75-DD5C3B0A02E7"),
                                                                   };

        public static TransactionFeeToCalculate FixedMerchantFee5 = new TransactionFeeToCalculate
                                                                   {
                                                                       Value = 5.0m,
                                                                       CalculationType = CalculationType.Fixed,
                                                                       FeeType = FeeType.Merchant,
                                                                       FeeId = Guid.Parse("D24C1645-447F-433B-8CB4-10F03532211A")
                                                                   };

        public static TransactionFeeToCalculate FixedServiceFee2 = new TransactionFeeToCalculate
                                                                   {
                                                                       Value = 2.0m,
                                                                       CalculationType = CalculationType.Fixed,
                                                                       FeeType = FeeType.ServiceProvider,
                                                                       FeeId =Guid.Parse("905F6DF5-0F00-45F4-B139-AA5965BE522A")
                                                                   };

        public static TransactionFeeToCalculate FixedMerchantFee2 = new TransactionFeeToCalculate
                                                                   {
                                                                       Value = 2.0m,
                                                                       CalculationType = CalculationType.Fixed,
                                                                       FeeType = FeeType.Merchant,
                                                                       FeeId = Guid.Parse("12DF5128-8DDA-40A2-B34E-A20353D53EE8")
        };

        public static TransactionFeeToCalculate PercentageServiceFeeQuarterPercent = new TransactionFeeToCalculate
                                                                   {
                                                                       Value = 0.25m,
                                                                       CalculationType = CalculationType.Percentage,
                                                                       FeeType = FeeType.ServiceProvider,
                                                                       FeeId = Guid.Parse("C3CA3208-BBE1-4155-A64D-465DD94A1C9B")
        };

        public static TransactionFeeToCalculate PercentageMerchantFeeQuarterPercent = new TransactionFeeToCalculate
                                                                                     {
                                                                                         Value = 0.25m,
                                                                                         CalculationType = CalculationType.Percentage,
                                                                                         FeeType = FeeType.Merchant,
                                                                                         FeeId = Guid.Parse("DD737732-3B94-480F-A98C-58AF60BCD4AF")
        };

        public static TransactionFeeToCalculate PercentageServiceFeeThreeQuarterPercent = new TransactionFeeToCalculate
                                                                                           {
                                                                                               Value = 0.75m,
                                                                                               CalculationType = CalculationType.Percentage,
                                                                                               FeeType = FeeType.ServiceProvider,
                                                                                               FeeId = Guid.Parse("2CD69A2A-E04A-42E2-B01C-706AEE172B80")
        };

        public static TransactionFeeToCalculate PercentageMerchantFeeThreeQuarterPercent = new TransactionFeeToCalculate
                                                                                     {
                                                                                         Value = 0.75m,
                                                                                         CalculationType = CalculationType.Percentage,
                                                                                         FeeType = FeeType.Merchant,
                                                                                         FeeId = Guid.Parse("25E12CD8-5F1D-4A78-80D9-7CC4B0D64E50")
        };

    }
}
