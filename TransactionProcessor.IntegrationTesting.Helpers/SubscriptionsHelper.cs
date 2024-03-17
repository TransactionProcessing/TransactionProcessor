namespace TransactionProcessor.IntegrationTesting.Helpers;

public static class SubscriptionsHelper
{
    public static List<(String streamName, String groupName, Int32 maxRetries)> GetSubscriptions()
    {
        List<(String streamName, String groupName, Int32 maxRetries)> subscriptions = new(){
                                                                                               ("$ce-EstateAggregate", "Transaction Processor - Ordered", 2),
                                                                                               ("$ce-MerchantAggregate", "Transaction Processor - Ordered", 2),
                                                                                               ("$ce-MerchantDepositListAggregate", "Transaction Processor - Ordered", 2),
                                                                                               ("$ce-SettlementAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-TransactionAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-TransactionAggregate", "Transaction Processor - Ordered", 2),
                                                                                               ("$ce-VoucherAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-VoucherAggregate", "Transaction Processor - Ordered", 2)
                                                                                           };

        return subscriptions;
    }
}