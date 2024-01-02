@base @shared
Feature: SaleTransaction

Background: 

	Given I create the following api scopes
	| Name                 | DisplayName                       | Description                            |
	| estateManagement     | Estate Managememt REST Scope      | A scope for Estate Managememt REST     |
	| transactionProcessor | Transaction Processor REST  Scope | A scope for Transaction Processor REST |
	| voucherManagement    | Voucher Management REST  Scope    | A scope for Voucher Management REST    |
	| messagingService     | Scope for Messaging REST          | Scope for Messaging REST               |

	Given the following api resources exist
	| Name         | DisplayName                | Secret  | Scopes               | UserClaims                 |
	| estateManagement     | Estate Managememt REST     | Secret1 | estateManagement     | MerchantId, EstateId, role |
	| transactionProcessor | Transaction Processor REST | Secret1 | transactionProcessor |                            |
	| voucherManagement    | Voucher Management REST    | Secret1 | voucherManagement    |                            |
	| messagingService     | Messaging REST             | Secret  | messagingService     |                            |

	Given the following clients exist
	| ClientId      | ClientName     | Secret  | Scopes    | GrantTypes  |
	| serviceClient | Service Client | Secret1 | estateManagement,transactionProcessor,voucherManagement,messagingService | client_credentials |

	Given I have a token to access the estate management and transaction processor resources
	| ClientId      | 
	| serviceClient | 

	Given I have created the following estates
	| EstateName    |
	| Test Estate 1 |

	Given I have created the following operators
	| EstateName    | OperatorName     | RequireCustomMerchantNumber | RequireCustomTerminalNumber |
	| Test Estate 1 | Safaricom        | True                        | True                        |
	| Test Estate 1 | Voucher          | True                        | True                        |
	| Test Estate 1 | PataPawa PostPay | True                        | True                        |
		
	Given I create a contract with the following values
	| EstateName    | OperatorName     | ContractDescription       |
	| Test Estate 1 | Safaricom        | Safaricom Contract        |
	| Test Estate 1 | Voucher          | Hospital 1 Contract       |
	| Test Estate 1 | PataPawa PostPay | PataPawa PostPay Contract |
	
	When I create the following Products
	| EstateName    | OperatorName     | ContractDescription       | ProductName       | DisplayText     | Value | ProductType |
	| Test Estate 1 | Safaricom        | Safaricom Contract        | Variable Topup    | Custom          |       | MobileTopup |
	| Test Estate 1 | Voucher          | Hospital 1 Contract       | 10 KES            | 10 KES          |       | Voucher     |
	| Test Estate 1 | PataPawa PostPay | PataPawa PostPay Contract | Post Pay Bill Pay | Bill Pay (Post) |       | BillPayment |
	
	When I add the following Transaction Fees
	| EstateName    | OperatorName     | ContractDescription       | ProductName       | CalculationType | FeeDescription      | Value |
	| Test Estate 1 | Safaricom        | Safaricom Contract        | Variable Topup    | Percentage           | Merchant Commission | 0.50  |
	| Test Estate 1 | PataPawa PostPay | PataPawa PostPay Contract | Post Pay Bill Pay | Percentage           | Merchant Commission | 0.50  |
	
	Given I create the following merchants
	| MerchantName    | AddressLine1   | Town     | Region      | Country        | ContactName    | EmailAddress                 | EstateName    |
	| Test Merchant 1 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 1 | testcontact1@merchant1.co.uk | Test Estate 1 |
	| Test Merchant 2 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 2 | testcontact2@merchant2.co.uk | Test Estate 1 |
	| Test Merchant 3 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 3 | testcontact3@merchant3.co.uk | Test Estate 1 |
	| Test Merchant 4 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 4 | testcontact4@merchant4.co.uk | Test Estate 1 |

	Given I have assigned the following  operator to the merchants
	| OperatorName     | MerchantName    | MerchantNumber | TerminalNumber | EstateName    |
	| Safaricom        | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |
	| Voucher          | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |
	| PataPawa PostPay | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |
	| Safaricom        | Test Merchant 2 | 00000002       | 10000002       | Test Estate 1 |
	| Voucher          | Test Merchant 2 | 00000002       | 10000002       | Test Estate 1 |
	| PataPawa PostPay | Test Merchant 2 | 00000002       | 10000002       | Test Estate 1 |
	| Safaricom        | Test Merchant 3 | 00000003       | 10000003       | Test Estate 1 |
	| Voucher          | Test Merchant 3 | 00000003       | 10000003       | Test Estate 1 |
	| PataPawa PostPay | Test Merchant 3 | 00000003       | 10000003       | Test Estate 1 |
	| Safaricom        | Test Merchant 4 | 00000004       | 10000004       | Test Estate 1 |
	| Voucher          | Test Merchant 4 | 00000004       | 10000004       | Test Estate 1 |
	| PataPawa PostPay | Test Merchant 4 | 00000004       | 10000004       | Test Estate 1 |

	Given I have assigned the following devices to the merchants
	| DeviceIdentifier | MerchantName    | EstateName    |
	| 123456780        | Test Merchant 1 | Test Estate 1 |
	| 123456781        | Test Merchant 2 | Test Estate 1 |
	| 123456782        | Test Merchant 3 | Test Estate 1 |
	| 123456783        | Test Merchant 4 | Test Estate 1 |

	Given I make the following manual merchant deposits 
	| Reference | Amount  | DateTime | MerchantName    | EstateName    |
	| Deposit1  | 240.00 | Today    | Test Merchant 1 | Test Estate 1 |
	| Deposit1  | 110.00 | Today    | Test Merchant 2 | Test Estate 1 |
	| Deposit1  | 110.00 | Today    | Test Merchant 3 | Test Estate 1 |
	| Deposit1  | 100.00 | Today    | Test Merchant 4 | Test Estate 1 |

	When I add the following contracts to the following merchants
	| EstateName    | MerchantName    | ContractDescription       |
	| Test Estate 1 | Test Merchant 1 | Safaricom Contract        |
	| Test Estate 1 | Test Merchant 1 | Hospital 1 Contract       |
	| Test Estate 1 | Test Merchant 1 | PataPawa PostPay Contract |
	| Test Estate 1 | Test Merchant 2 | Safaricom Contract        |
	| Test Estate 1 | Test Merchant 2 | Hospital 1 Contract       |
	| Test Estate 1 | Test Merchant 2 | PataPawa PostPay Contract |
	| Test Estate 1 | Test Merchant 3 | Safaricom Contract        |
	| Test Estate 1 | Test Merchant 3 | Hospital 1 Contract       |
	| Test Estate 1 | Test Merchant 3 | PataPawa PostPay Contract |
	| Test Estate 1 | Test Merchant 4 | Safaricom Contract        |
	| Test Estate 1 | Test Merchant 4 | Hospital 1 Contract       |
	| Test Estate 1 | Test Merchant 4 | PataPawa PostPay Contract |

	Given the following bills are available at the PataPawa PostPaid Host
	| AccountNumber | AccountName    | DueDate | Amount |
	| 12345678      | Test Account 1 | Today   | 100.00 |

@PRTest
Scenario: Sale Transactions

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | TransactionSource | MerchantName    | DeviceIdentifier | EstateName    | OperatorName     | TransactionAmount | CustomerAccountNumber | CustomerEmailAddress        | ContractDescription       | ProductName       | RecipientEmail       | RecipientMobile | MessageType   | AccountNumber | CustomerName     |
	| Today    | 1                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom        | 110.00            | 123456789             | testcustomer@customer.co.uk | Safaricom Contract        | Variable Topup    |                      |                 |               |               |                  |
	| Today    | 2                 | Sale            | 1                 | Test Merchant 2 | 123456781        | Test Estate 1 | Safaricom        | 100.00            | 123456789             |                             | Safaricom Contract        | Variable Topup    |                      |                 |               |               |                  |
	| Today    | 3                 | Sale            | 2                 | Test Merchant 3 | 123456782        | Test Estate 1 | Safaricom        | 100.00            | 123456789             |                             | Safaricom Contract        | Variable Topup    |                      |                 |               |               |                  |
	| Today    | 4                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom        | 90.00             | 123456789             | testcustomer@customer.co.uk | Safaricom Contract        | Variable Topup    |                      |                 |               |               |                  |
	| Today    | 5                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Voucher          | 10.00             |                       |                             | Hospital 1 Contract       | 10 KES            | test@recipient.co.uk |                 |               |               |                  |
	| Today    | 6                 | Sale            | 1                 | Test Merchant 2 | 123456781        | Test Estate 1 | Voucher          | 10.00             |                       |                             | Hospital 1 Contract       | 10 KES            |                      | 123456789       |               |               |                  |
	| Today    | 7                 | Sale            | 2                 | Test Merchant 3 | 123456782        | Test Estate 1 | Voucher          | 10.00             |                       |                             | Hospital 1 Contract       | 10 KES            | test@recipient.co.uk |                 |               |               |                  |
	| Today    | 8                 | Sale            | 2                 | Test Merchant 1 | 123456780        | Test Estate 1 | PataPawa PostPay | 0.00              |                       |                             | PataPawa PostPay Contract | Post Pay Bill Pay | test@recipient.co.uk |                 | VerifyAccount | 12345678      |                  |
	| Today    | 9                 | Sale            | 2                 | Test Merchant 1 | 123456780        | Test Estate 1 | PataPawa PostPay | 20.00             |                       |                             | PataPawa PostPay Contract | Post Pay Bill Pay | test@recipient.co.uk | 123456789       | ProcessBill   | 12345678      | Mr Test Customer |
		
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage |
	| Test Estate 1 | Test Merchant 1 | 1                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 2 | 2                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 3 | 3                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 1 | 4                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 1 | 5                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 2 | 6                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 3 | 7                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 1 | 8                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 1 | 9                 | 0000         | SUCCESS         |
	
	Then the following entries appear in the merchants balance history for estate 'Test Estate 1' and merchant 'Test Merchant 1'
	| DateTime | Reference                 | EntryType | In     | Out    | ChangeAmount | Balance |
	| Today    | Merchant Deposit          | C         | 240.00 | 0.00   | 240.00       | 230.00  |
	| Today    | Transaction Completed     | D         | 0.00   | 110.00 | 110.00       | 130.00  |
	| Today    | Transaction Completed     | D         | 0.00   | 90.00  | 90.00        | 30.00   |
	| Today    | Transaction Completed     | D         | 0.00   | 10.00  | 10.00        | 20.00   |
	| Today    | Transaction Completed     | D         | 0.00   | 20.00  | 20.00        | 20.00   |
	| Today    | Transaction Fee Processed | C         | 0.00   | 0.55   | 0.55         | 20.00   |
	| Today    | Transaction Fee Processed | C         | 0.00   | 0.45   | 0.45         | 20.00   |
	| Today    | Transaction Fee Processed | C         | 0.00   | 0.01   | 0.10         | 20.00   |
	| Today    | Opening Balance           | C         | 0.00   | 0.00   | 0.00         | 20.00   |

	Then the following entries appear in the merchants balance history for estate 'Test Estate 1' and merchant 'Test Merchant 2'
	| DateTime | Reference                 | EntryType | In     | Out    | ChangeAmount | Balance |
	| Today    | Merchant Deposit          | C         | 110.00 | 0.00   | 110.00       | 230.00  |
	| Today    | Transaction Completed     | D         | 0.00   | 100.00 | 100.00       | 130.00  |
	| Today    | Transaction Completed     | D         | 0.00   | 10.00  | 10.00        | 30.00   |
	| Today    | Transaction Fee Processed | C         | 0.00   | 0.50   | 0.50         | 20.00   |
	| Today    | Opening Balance           | C         | 0.00   | 0.00   | 0.00         | 20.00   |

	Then the following entries appear in the merchants balance history for estate 'Test Estate 1' and merchant 'Test Merchant 3'
	| DateTime | Reference                 | EntryType | In     | Out    | ChangeAmount | Balance |
	| Today    | Merchant Deposit          | C         | 110.00 | 0.00   | 110.00       | 230.00  |
	| Today    | Transaction Completed     | D         | 0.00   | 100.00 | 100.00       | 130.00  |
	| Today    | Transaction Completed     | D         | 0.00   | 10.00  | 10.00        | 30.00   |
	| Today    | Transaction Fee Processed | C         | 0.00   | 0.85   | 0.50         | 20.00   |
	| Today    | Opening Balance           | C         | 0.00   | 0.00   | 0.00         | 20.00   |

	When I request the receipt is resent
	| EstateName    | MerchantName    | TransactionNumber | 
	| Test Estate 1 | Test Merchant 1 | 1                 | 

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | TransactionSource | MerchantName    | DeviceIdentifier | EstateName    | OperatorName | TransactionAmount | CustomerAccountNumber | CustomerEmailAddress        | ContractDescription | ProductName    |
	| Today    | 10                | Sale            | 1                 | Test Merchant 1 | 123456781        | Test Estate 1 | Safaricom    | 100.00            | 123456789             | testcustomer@customer.co.uk | Safaricom Contract  | Variable Topup |
	
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage                                                    |
	| Test Estate 1 | Test Merchant 1 | 10                 | 1000         | Device Identifier 123456781 not valid for Merchant Test Merchant 1 |

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | TransactionSource | MerchantName    | DeviceIdentifier | EstateName    | OperatorName | TransactionAmount | CustomerAccountNumber | CustomerEmailAddress        | ContractDescription | ProductName    |
	| Today    | 11                 | Sale            | 1                 | Test Merchant 1 | 123456780        | InvalidEstate | Safaricom    | 100.00            | 123456789             | testcustomer@customer.co.uk | Safaricom Contract  | Variable Topup |
	
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage                                                        |
	| InvalidEstate | Test Merchant 1 | 11                | 1001         | Estate Id [79902550-64df-4491-b0c1-4e78943928a3] is not a valid estate |

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | TransactionSource | MerchantName    | DeviceIdentifier | EstateName    | OperatorName | TransactionAmount | CustomerAccountNumber | CustomerEmailAddress        | ContractDescription | ProductName    |
	| Today    | 12                 | Sale            | 1                 | InvalidMerchant | 123456780        | Test Estate 1 | Safaricom    | 100.00            | 123456789             | testcustomer@customer.co.uk | Safaricom Contract  | Variable Topup |
	
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage                                                                                       |
	| Test Estate 1 | InvalidMerchant | 12                 | 1002         | Merchant Id [d59320fa-4c3e-4900-a999-483f6a10c69a] is not a valid merchant for estate [Test Estate 1] |

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | TransactionSource | MerchantName    | DeviceIdentifier | EstateName    | OperatorName | TransactionAmount | CustomerAccountNumber | CustomerEmailAddress        | ContractDescription | ProductName    |
	| Today    | 13                | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 100.00            | 123456789             | testcustomer@customer.co.uk | EmptyContract       | Variable Topup |
	
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage                                                                       |
	| Test Estate 1 | Test Merchant 1 | 13                 | 1012         | Contract Id [00000000-0000-0000-0000-000000000000] must be set for a sale transaction |

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | TransactionSource | MerchantName    | DeviceIdentifier | EstateName    | OperatorName | TransactionAmount | CustomerAccountNumber | CustomerEmailAddress        | ContractDescription | ProductName    |
	| Today    | 14                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 100.00            | 123456789             | testcustomer@customer.co.uk | InvalidContract  | Variable Topup |
	
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage                                                                             |
	| Test Estate 1 | Test Merchant 1 | 14                 | 1015         | Contract Id [934d8164-f36a-448e-b27b-4d671d41d180] not valid for Merchant [Test Merchant 1] |

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | TransactionSource | MerchantName    | DeviceIdentifier | EstateName    | OperatorName | TransactionAmount | CustomerAccountNumber | CustomerEmailAddress        | ContractDescription | ProductName    |
	| Today    | 15                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 100.00            | 123456789             | testcustomer@customer.co.uk | Safaricom Contract  | EmptyProduct |
	
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage                                                                      |
	| Test Estate 1 | Test Merchant 1 | 15                 | 1013         | Product Id [00000000-0000-0000-0000-000000000000] must be set for a sale transaction |

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | TransactionSource | MerchantName    | DeviceIdentifier | EstateName    | OperatorName | TransactionAmount | CustomerAccountNumber | CustomerEmailAddress        | ContractDescription | ProductName    |
	| Today    | 16                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 100.00            | 123456789             | testcustomer@customer.co.uk | Safaricom Contract  | InvalidProduct |
	
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage                                                                      |
	| Test Estate 1 | Test Merchant 1 | 16                 | 1016         | Product Id [934d8164-f36a-448e-b27b-4d671d41d180] not valid for Merchant [Test Merchant 1] |

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | TransactionSource | MerchantName    | DeviceIdentifier | EstateName    | OperatorName | TransactionAmount | CustomerAccountNumber | CustomerEmailAddress        | ContractDescription | ProductName    |
	| Today    | 17                 | Sale            | 1                  | Test Merchant 4 | 123456783        | Test Estate 1 | Safaricom    | 300.00            | 123456789             | testcustomer@customer.co.uk | Safaricom Contract  | Variable Topup |
		
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage                                                                                                    |
	| Test Estate 1 | Test Merchant 4 | 17                 | 1009         | Merchant [Test Merchant 4] does not have enough credit available [100.00] to perform transaction amount [300.00] |

	