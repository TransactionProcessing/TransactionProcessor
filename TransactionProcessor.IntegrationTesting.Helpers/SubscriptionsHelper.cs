namespace TransactionProcessor.IntegrationTesting.Helpers;

public static class SubscriptionsHelper
{
    public static List<(String streamName, String groupName, Int32 maxRetries)> GetSubscriptions()
    {
        List<(String streamName, String groupName, Int32 maxRetries)> subscriptions = new(){
                                                                                               // Main
                                                                                               ("$ce-CallbackMessageAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-ContractAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-EstateAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-FileAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-FileImportLogAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-FloatAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-MerchantAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-MerchantStatementAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-OperatorAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-ReconciliationAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-SettlementAggregate", "Transaction Processor", 0),
                                                                                               ("$ce-TransactionAggregate", "Transaction Processor", 0),

                                                                                               // Ordered
                                                                                               ("$ce-MerchantAggregate", "Transaction Processor - Ordered", 2),
                                                                                               ("$ce-MerchantStatementAggregate", "Transaction Processor - Ordered", 0),
                                                                                               ("$ce-SettlementAggregate", "Transaction Processor - Ordered", 1),
                                                                                               ("$ce-TransactionAggregate", "Transaction Processor - Ordered", 2),
                                                                                               ("$ce-VoucherAggregate", "Transaction Processor - Ordered", 2),
                                                                                               ("$ce-MerchantDepositListAggregate", "Transaction Processor - Ordered", 2),
                                                                                           };

        return subscriptions;
    }
}