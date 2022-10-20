

fromCategory('MerchantArchive')
    .foreachStream()
    .when({
        $init: function () {
            return {
                initialised: true,
                availableBalance: 0,
                balance: 0,
                lastDepositDateTime: null,
                lastSaleDateTime: null,
                lastFeeProcessedDateTime: null,
                debug: [],
                totalDeposits: 0,
                totalAuthorisedSales: 0,
                totalDeclinedSales: 0,
                totalFees: 0,
                emittedEvents: 1
            }
        },
        $any: function (s, e) {
            if (e === null || e.data === null || e.data.IsJson === false)
                return;

            eventbus.dispatch(s, e);
        }
    });

var eventbus = {
    dispatch: function (s, e) {

        if (e.eventType === 'MerchantCreatedEvent') {
            merchantCreatedEventHandler(s, e);
            return;
        }

        if (e.eventType === 'ManualDepositMadeEvent') {
            depositMadeEventHandler(s, e);
            return;
        }

        if (e.eventType === 'AutomaticDepositMadeEvent') {
            depositMadeEventHandler(s, e);
            return;
        }

        if (e.eventType === 'TransactionHasStartedEvent') {
            transactionHasStartedEventHandler(s, e);
            return;
        }

        if (e.eventType === 'TransactionHasBeenCompletedEvent') {
            transactionHasCompletedEventHandler(s, e);
            return;
        }

        if (e.eventType === 'MerchantFeeAddedToTransactionEvent') {
            merchantFeeAddedToTransactionEventHandler(s, e);
            return;
        }
    }
}

function addTwoNumbers(number1, number2) {
    return parseFloat((number1 + number2).toFixed(4));
}

function subtractTwoNumbers(number1, number2) {
    return parseFloat((number1 - number2).toFixed(4));
}

var incrementBalanceFromDeposit = function (s, amount, dateTime) {
    s.balance = addTwoNumbers(s.balance, amount);
    s.availableBalance = addTwoNumbers(s.availableBalance, amount);
    s.totalDeposits = addTwoNumbers(s.totalDeposits, amount);

    // protect against events coming in out of order
    if (s.lastDepositDateTime === null || dateTime > s.lastDepositDateTime) {
        s.lastDepositDateTime = dateTime;
    }
};

var incrementBalanceFromMerchantFee = function (s, amount, dateTime) {
    s.balance = addTwoNumbers(s.balance, amount);
    s.availableBalance = addTwoNumbers(s.availableBalance, amount);
    s.totalFees = addTwoNumbers(s.totalFees, amount);

    // protect against events coming in out of order
    if (s.lastFeeProcessedDateTime === null || dateTime > s.lastFeeProcessedDateTime) {
        s.lastFeeProcessedDateTime = dateTime;
    }
};

var decrementAvailableBalanceFromTransactionStarted = function (s, amount, dateTime) {
    s.availableBalance = subtractTwoNumbers(s.availableBalance, amount);

    // protect against events coming in out of order
    if (s.lastSaleDateTime === null || dateTime > s.lastSaleDateTime) {
        s.lastSaleDateTime = dateTime;
    }
};

var decrementBalanceFromAuthorisedTransaction = function (s, amount) {
    s.balance = subtractTwoNumbers(s.balance, amount);
    s.totalAuthorisedSales = addTwoNumbers(s.totalAuthorisedSales, amount);
};

var incrementAvailableBalanceFromDeclinedTransaction = function (s, amount) {
    s.availableBalance = addTwoNumbers(s.availableBalance, amount);
    s.totalDeclinedSales = addTwoNumbers(s.totalDeclinedSales, amount);
};

var merchantCreatedEventHandler = function (s, e) {

    // Setup the state here
    s.estateId = e.data.estateId;
    s.merchantId = e.data.merchantId;
    s.merchantName = e.data.merchantName;
};



var depositMadeEventHandler = function (s, e) {

    // Check if we have got a merchant id already set
    if (s.merchantId === undefined) {
        // We have obviously not got a created event yet but we must process this event,
        // so fill in what we can here
        s.estateId = e.data.estateId;
        s.merchantId = e.data.merchantId;
    }

    incrementBalanceFromDeposit(s, e.data.amount, e.data.depositDateTime);
};

var transactionHasStartedEventHandler = function (s, e) {

    // Check if we have got a merchant id already set
    if (s.merchantId === undefined) {
        // We have obviously not got a created event yet but we must process this event,
        // so fill in what we can here
        e.estateId = e.data.estateId;
        s.merchantId = e.data.merchantId;
    }

    var amount = e.data.transactionAmount;
    if (amount === undefined) {
        amount = 0;
    }
    decrementAvailableBalanceFromTransactionStarted(s, amount, e.data.transactionDateTime);
};

var transactionHasCompletedEventHandler = function (s, e) {

    // Check if we have got a merchant id already set
    if (s.merchantId === undefined) {
        // We have obviously not got a created event yet but we must process this event,
        // so fill in what we can here
        e.estateId = e.data.estateId;
        s.merchantId = e.data.merchantId;
    }

    var amount = e.data.transactionAmount;
    if (amount === undefined) {
        amount = 0;
    }

    var transactionDateTime = new Date(Date.parse(e.data.completedDateTime));
    var completedTime = new Date(transactionDateTime.getFullYear(), transactionDateTime.getMonth(), transactionDateTime.getDate(), transactionDateTime.getHours(), transactionDateTime.getMinutes(), transactionDateTime.getSeconds() + 2);

    if (e.data.isAuthorised) {
        decrementBalanceFromAuthorisedTransaction(s, amount, completedTime);
    }
    else {
    incrementAvailableBalanceFromDeclinedTransaction(s, amount, completedTime);
}
};

var merchantFeeAddedToTransactionEventHandler = function (s, e) {

    // Check if we have got a merchant id already set
    if (s.merchantId === undefined) {
        // We have obviously not got a created event yet but we must process this event,
        // so fill in what we can here
        e.estateId = e.data.estateId;
        s.merchantId = e.data.merchantId;
    }

    // increment the balance now
    incrementBalanceFromMerchantFee(s, e.data.calculatedValue, e.data.feeCalculatedDateTime);
}