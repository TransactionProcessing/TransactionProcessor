namespace TransactionProcessor.IntegrationTesting.Helpers;

public static class SubscriptionsHelper
{
    public static List<(String streamName, String groupName, Int32 maxRetries)> GetSubscriptions()
    {
        List<(String streamName, String groupName, Int32 maxRetries)> subscriptions = new(){
                                                                                               ("$ce-EstateAggregate", "Transaction Processor", 2),
                                                                                               ("$ce-EstateAggregate", "Transaction Processor - Ordered", 2),
                                                                                               ("$ce-MerchantAggregate", "Transaction Processor - Ordered", 2),
                                                                                               ("$ce-MerchantDepositListAggregate", "Transaction Processor - Ordered", 2),
                                                                                               ("$ce-SettlementAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-TransactionAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-TransactionAggregate", "Transaction Processor - Ordered", 2),
                                                                                               ("$ce-VoucherAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-VoucherAggregate", "Transaction Processor - Ordered", 2),

                                                                                               ("$ce-CallbackMessageAggregate", "Estate Management", 0),
                                                                                               ("$ce-ContractAggregate", "Estate Management", 0),
                                                                                               ("$ce-FileAggregate", "Estate Management", 0),
                                                                                               ("$ce-FloatAggregate", "Estate Management", 0),
                                                                                               ("$ce-MerchantAggregate", "Estate Management", 0),
                                                                                               ("$ce-MerchantStatementAggregate", "Estate Management", 0),
                                                                                               ("$ce-MerchantStatementAggregate", "Estate Management - Ordered", 0),
                                                                                               ("$ce-ReconciliationAggregate", "Estate Management", 0),
                                                                                               ("$ce-SettlementAggregate", "Estate Management", 0),
                                                                                               ("$ce-TransactionAggregate", "Estate Management", 0),
                                                                                               ("$ce-TransactionAggregate", "Estate Management - Ordered", 0),
                                                                                               ("$ce-VoucherAggregate", "Estate Management", 0),
                                                                                               ("$ce-OperatorAggregate", "Estate Management", 0),

                                                                                           };

        return subscriptions;
    }
}