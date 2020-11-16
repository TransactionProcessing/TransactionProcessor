var fromCategory = fromCategory || require('../../node_modules/event-store-projection-testing').scope.fromCategory;
var partitionBy = partitionBy !== null ? partitionBy : require('../../node_modules/event-store-projection-testing').scope.partitionBy;
var emit = emit || require('../../node_modules/event-store-projection-testing').scope.emit;

fromCategory('MerchantArchive')
    .foreachStream()
    .when({
        $any: function (s, e) {

            if (e === null || e.data === null || e.data.IsJson === false)
                return;

            eventbus.dispatch(s, e);
        }
    });

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

        if (e.eventType === 'TransactionProcessor.Transaction.DomainEvents.MerchantFeeAddedToTransactionEvent') {
            merchantFeeAddedToTransactionEventHandler(s, e);
            return;
        }
    }
}

function getStreamName(s) {
    return "MerchantBalanceHistory-" + s.merchantId.replace(/-/gi, "");
}

function getEventTypeName() {
    return 'EstateReporting.BusinessLogic.Events.' + getEventType() + ', EstateReporting.BusinessLogic';
}

function getEventType() { return "MerchantBalanceChangedEvent"; }

function generateEventId() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g,
        function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
}

var incrementBalanceFromDeposit = function (s, amount, dateTime) {
    s.balance += amount;
    s.availableBalance += amount;
    s.totalDeposits += amount;
    // protect against events coming in out of order
    if (s.lastDepositDateTime === null || dateTime > s.lastDepositDateTime) {
        s.lastDepositDateTime = dateTime;
    }
};

var addPendingBalanceUpdate = function (s, amount, transactionId, dateTime) {
    s.availableBalance -= amount;
    s.pendingBalanceUpdates.push({
        amount: amount,
        transactionId: transactionId
    });
    // protect against events coming in out of order
    if (s.lastSaleDateTime === null || dateTime > s.lastSaleDateTime) {
        s.lastSaleDateTime = dateTime;
    }
};

var incrementBalanceFromMerchantFee = function (s, amount, dateTime) {
    s.balance += amount;
    s.availableBalance += amount;
    s.totalFees += amount;

    // protect against events coming in out of order
    if (s.lastFeeProcessedDateTime === null || dateTime > s.lastFeeProcessedDateTime) {
        s.lastFeeProcessedDateTime = dateTime;
    }
}

var handlePendingBalanceUpdate = function (s, transactionId, isAuthorised) {
    // lookup the balance update
    //var balanceUpdate = s.pendingBalanceUpdates[transactionId];
    var balanceUpdateIndex = s.pendingBalanceUpdates.findIndex(element => element.transactionId === transactionId);
    var balanceUpdate = s.pendingBalanceUpdates[balanceUpdateIndex];
    if (balanceUpdate !== undefined) {
        if (isAuthorised) {
            s.balance -= balanceUpdate.amount;
            s.totalSales += balanceUpdate.amount;
        }
        else {
            s.availableBalance += balanceUpdate.amount;
            s.totalIncompleteSales += balanceUpdate.amount;
        }

        s.pendingBalanceUpdates.splice(balanceUpdateIndex);
        return balanceUpdate.amount;
    }
};

var merchantCreatedEventHandler = function (s, e) {

    // Setup the state here
    s.estateId = e.data.EstateId;
    s.merchantId = e.data.MerchantId;
    s.merchantName = e.data.MerchantName;
    s.availableBalance = 0;
    s.balance = 0;
    s.lastDepositDateTime = null;
    s.lastSaleDateTime = null;
    s.lastFeeProcessedDateTime = null;
    s.pendingBalanceUpdates = [];
    s.totalDeposits = 0;
    s.totalSales = 0;
    s.totalIncompleteSales = 0;
    s.totalFees = 0;
    emitBalanceChangedEvent(s, 0, e.data.EventCreatedDateTime, "Merchant Created");
};

var emitBalanceChangedEvent = function (s, changeAmount, dateTime, reference) {
    var balanceChangedEvent = {
        $type: getEventTypeName(),
        "merchantId": s.merchantId,
        "estateId": s.estateId,
        "availableBalance": s.availableBalance,
        "balance": s.balance,
        "changeAmount": changeAmount,
        "eventId": generateEventId(),
        "eventTimestamp": dateTime,
        "reference": reference
    }

    // emit an balance changed event here
    emit(getStreamName(s), getEventType(), balanceChangedEvent);
};

var depositMadeEventHandler = function (s, e) {
    incrementBalanceFromDeposit(s, e.data.Amount, e.data.DepositDateTime);

    // emit an balance changed event here
    emitBalanceChangedEvent(s, e.data.Amount, e.data.DepositDateTime, "Merchant Deposit");
};

var transactionHasStartedEventHandler = function (s, e) {

    var amount = e.data.TransactionAmount;
    if (amount === undefined) {
        amount = 0;
    }

    // Add this to a pending balance update list
    addPendingBalanceUpdate(s, amount, e.data.TransactionId, e.data.TransactionDateTime);

    // emit an balance changed event here
    emitBalanceChangedEvent(s, 0, e.data.TransactionDateTime, "Transaction Started");
};

var transactionHasCompletedEventHandler = function (s, e) {
    // Handle the pending balance recorda
    var balanceUpdateValue = handlePendingBalanceUpdate(s, e.data.TransactionId, e.data.IsAuthorised);

    // emit an balance changed event here
    emitBalanceChangedEvent(s, balanceUpdateValue, e.data.EventCreatedDateTime, "Transaction Completed");
};

var merchantFeeAddedToTransactionEventHandler = function (s, e) {
    // increment the balance now
    incrementBalanceFromMerchantFee(s, e.data.CalculatedValue, e.data.EventCreatedDateTime);

    // emit an balance changed event here
    emitBalanceChangedEvent(s, e.data.CalculatedValue, e.data.EventCreatedDateTime, "Transaction Fee Processed");
}



