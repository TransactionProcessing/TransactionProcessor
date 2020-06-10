var fromStreams = fromStreams || require('../../node_modules/event-store-projection-testing').scope.fromStreams;
var partitionBy = partitionBy !== null ? partitionBy : require('../../node_modules/event-store-projection-testing').scope.partitionBy;
var emit = emit || require('../../node_modules/event-store-projection-testing').scope.emit;

var incrementBalanceFromDeposit = function(s, merchantId, amount, dateTime) {
    var merchant = s.merchants[merchantId];
    merchant.Balance += amount;
    merchant.AvailableBalance += amount;
        // protect against events coming in out of order
    if (merchant.LastDepositDateTime === null || dateTime > merchant.LastDepositDateTime) {
            merchant.LastDepositDateTime = dateTime;
        }
    }

var addPendingBalanceUpdate = function(s, merchantId, amount, transactionId, dateTime)
{
    var merchant = s.merchants[merchantId];
    merchant.AvailableBalance -= amount;
    merchant.PendingBalanceUpdates[transactionId] = {
        Amount: amount,
        TransactionId: transactionId
    };
    // protect against events coming in out of order
    if (merchant.LastSaleDateTime === null || dateTime > merchant.LastSaleDateTime)
    {
        merchant.LastSaleDateTime = dateTime;
    }
}

var decrementBalanceForSale = function(s, merchantId, transactionId, isAuthorised)
{
    var merchant = s.merchants[merchantId];
    // lookup the balance update
    var balanceUpdate = merchant.PendingBalanceUpdates[transactionId];

    if (balanceUpdate !== undefined)
    {
        if (isAuthorised)
        {
            merchant.Balance -= balanceUpdate.Amount;
        }
        else
        {
            merchant.AvailableBalance += balanceUpdate.Amount;
        }

        delete merchant.PendingBalanceUpdates[transactionId];
    }
}


var eventbus = {
    dispatch: function (s, e) {

        if (e.eventType === 'EstateManagement.Merchant.DomainEvents.MerchantCreatedEvent') {
            merchantCreatedEventHandler(s, e);
            return;
        }

        if (e.eventType === 'EstateManagement.Merchant.DomainEvents.ManualDepositMadeEvent') {
            depositMadeEventHandler(s, e);
            return;
        }

        if (e.eventType === 'TransactionProcessor.Transaction.DomainEvents.TransactionHasStartedEvent') {
            transactionHasStartedEventHandler(s, e);
            return;
        }

        if (e.eventType === 'TransactionProcessor.Transaction.DomainEvents.TransactionHasBeenCompletedEvent') {
            transactionHasCompletedEventHandler(s, e);
            return;
        }
    }
}

var merchantCreatedEventHandler = function (s, e)
{
    var merchantId = e.data.MerchantId;

    if (s.merchants[merchantId] === undefined) {
        s.merchants[merchantId] = {
            MerchantId: e.data.MerchantId,
            MerchantName: e.data.MerchantName,
            AvailableBalance: 0,
            Balance: 0,
            LastDepositDateTime: null,
            LastSaleDateTime: null,
            PendingBalanceUpdates: []
        };
    }
}

var depositMadeEventHandler = function (s, e) {
    var merchantId = e.data.MerchantId;
    var merchant = s.merchants[merchantId];

    incrementBalanceFromDeposit(s, merchantId,e.data.Amount, e.data.DepositDateTime);
}

var transactionHasStartedEventHandler = function(s, e)
{
    // Add this to a pending balance update list
    var merchantId = e.data.MerchantId;
    var merchant = s.merchants[merchantId];

    var amount = e.data.TransactionAmount;
    if (amount === undefined)
    {
        amount = 0;
    }

    addPendingBalanceUpdate(s, merchantId,amount, e.data.TransactionId, e.data.TransactionDateTime);
}

var transactionHasCompletedEventHandler = function(s, e)
{
    // Add this to a pending balance update list
    var merchantId = e.data.MerchantId;
    var merchant = s.merchants[merchantId];

    decrementBalanceForSale(s, merchantId, e.data.TransactionId, e.data.IsAuthorised);
}

fromStreams('$et-EstateManagement.Merchant.DomainEvents.MerchantCreatedEvent',
    '$et-EstateManagement.Merchant.DomainEvents.ManualDepositMadeEvent',
    '$et-TransactionProcessor.Transaction.DomainEvents.TransactionHasStartedEvent',
    '$et-TransactionProcessor.Transaction.DomainEvents.TransactionHasBeenCompletedEvent')
    .partitionBy(function(e)
    {
        return "MerchantBalanceHistory-" + e.data.MerchantId.replace(/-/gi, "");
    })
    .when({
        $init: function (s, e) {
            return {
                merchants: {},
                debug: []
            };
        },

        $any: function (s, e) {

            if (e === null || e.data === null || e.data.IsJson === false)
                return;

            eventbus.dispatch(s, e);
        }
    });