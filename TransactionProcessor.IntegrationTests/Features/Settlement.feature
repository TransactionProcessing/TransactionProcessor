@base @shared
Feature: Settlement

Background: 

	Given I create the following api scopes
	| Name                 | DisplayName                       | Description                            |
	| estateManagement     | Estate Managememt REST Scope      | A scope for Estate Managememt REST     |
	| transactionProcessor | Transaction Processor REST  Scope | A scope for Transaction Processor REST |
	| voucherManagement | Voucher Management REST  Scope | A scope for Voucher Management REST |

	Given the following api resources exist
	| Name         | DisplayName                | Secret  | Scopes               | UserClaims                 |
	| estateManagement     | Estate Managememt REST     | Secret1 | estateManagement     | MerchantId, EstateId, role |
	| transactionProcessor | Transaction Processor REST | Secret1 | transactionProcessor |                            |
	| voucherManagement    | Voucher Management REST    | Secret1 | voucherManagement    |                            |

	Given the following clients exist
	| ClientId      | ClientName     | Secret  | Scopes    | GrantTypes  |
	| serviceClient | Service Client | Secret1 | estateManagement,transactionProcessor,voucherManagement | client_credentials |

	Given I have a token to access the estate management and transaction processor resources
	| ClientId      | 
	| serviceClient | 

	Given I have created the following estates
	| EstateName    |
	| Test Estate 1 |

	Given I have created the following operators
	| EstateName    | OperatorName | RequireCustomMerchantNumber | RequireCustomTerminalNumber |
	| Test Estate 1 | Safaricom    | True                        | True                        |
	| Test Estate 1 | Voucher    | True                        | True                        |

	Given I create a contract with the following values
	| EstateName    | OperatorName | ContractDescription |
	| Test Estate 1 | Safaricom    | Safaricom Contract  |
	| Test Estate 1 | Voucher      | Hospital 1 Contract |

	When I create the following Products
	| EstateName    | OperatorName | ContractDescription | ProductName    | DisplayText | Value | ProductType |
	| Test Estate 1 | Safaricom    | Safaricom Contract  | Variable Topup | Custom      |       | MobileTopup |
	| Test Estate 1 | Voucher      | Hospital 1 Contract | 10 KES         | 10 KES      |       | Voucher     |

	When I add the following Transaction Fees
	| EstateName    | OperatorName | ContractDescription | ProductName    | CalculationType | FeeDescription      | Value |
	| Test Estate 1 | Safaricom    | Safaricom Contract  | Variable Topup | Fixed           | Merchant Commission | 2.50  |

Scenario: Get Pending Settlement
	Given I create the following merchants
	| MerchantName    | AddressLine1   | Town     | Region      | Country        | ContactName    | EmailAddress                 | EstateName    | SettlementSchedule |
	| Test Merchant 1 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 1 | testcontact1@merchant1.co.uk | Test Estate 1 | Immediate          |
	| Test Merchant 2 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 2 | testcontact2@merchant2.co.uk | Test Estate 1 | Weekly             |
	| Test Merchant 3 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 3 | testcontact3@merchant2.co.uk | Test Estate 1 | Monthly            |

	Given I have assigned the following  operator to the merchants
	| OperatorName | MerchantName    | MerchantNumber | TerminalNumber | EstateName    |
	| Safaricom    | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |
	| Voucher      | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |
	| Safaricom    | Test Merchant 2 | 00000002       | 10000002       | Test Estate 1 |
	| Voucher      | Test Merchant 2 | 00000002       | 10000002       | Test Estate 1 |
	| Safaricom    | Test Merchant 3 | 00000003       | 10000003       | Test Estate 1 |
	| Voucher      | Test Merchant 3 | 00000003       | 10000003       | Test Estate 1 |

	Given I have assigned the following devices to the merchants
	| DeviceIdentifier | MerchantName    | EstateName    |
	| 123456780        | Test Merchant 1 | Test Estate 1 |
	| 123456781        | Test Merchant 2 | Test Estate 1 |
	| 123456782        | Test Merchant 3 | Test Estate 1 |

	Given I make the following manual merchant deposits 
	| Reference | Amount | DateTime | MerchantName    | EstateName    |
	| Deposit1  | 210.00 | Today    | Test Merchant 1 | Test Estate 1 |
	| Deposit1  | 110.00 | Today    | Test Merchant 2 | Test Estate 1 |
	| Deposit1  | 120.00 | Today    | Test Merchant 3 | Test Estate 1 |

	When I add the following contracts to the following merchants
	| EstateName    | MerchantName    | ContractDescription       |
	| Test Estate 1 | Test Merchant 1 | Safaricom Contract        |
	| Test Estate 1 | Test Merchant 1 | Hospital 1 Contract       |
	| Test Estate 1 | Test Merchant 2 | Safaricom Contract        |
	| Test Estate 1 | Test Merchant 2 | Hospital 1 Contract       |
	| Test Estate 1 | Test Merchant 3 | Safaricom Contract        |
	| Test Estate 1 | Test Merchant 3 | Hospital 1 Contract       |

	When I perform the following transactions
	| DateTime   | TransactionNumber | TransactionType | TransactionSource | MerchantName    | DeviceIdentifier | EstateName    | OperatorName | TransactionAmount | CustomerAccountNumber | CustomerEmailAddress        | ContractDescription | ProductName    | RecipientEmail       | RecipientMobile |
	| 2022-01-06 | 1                 | Sale            | 1                  | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 100.00            | 123456789             |                             | Safaricom Contract  | Variable Topup |                      |                 |
	| 2022-01-06 | 2                 | Sale            | 1                  | Test Merchant 2 | 123456781        | Test Estate 1 | Safaricom    | 100.00            | 123456789             |                             | Safaricom Contract  | Variable Topup |                      |                 |
	| 2022-01-06 | 3                 | Sale            | 2                  | Test Merchant 3 | 123456782        | Test Estate 1 | Safaricom    | 100.00            | 123456789             |                             | Safaricom Contract  | Variable Topup |                      |                 |
	| 2022-01-06 | 4                 | Sale            | 1                  | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 100.00            | 123456789             | testcustomer@customer.co.uk | Safaricom Contract  | Variable Topup |                      |                 |
	| 2022-01-06 | 5                 | Sale            | 1                  | Test Merchant 1 | 123456780        | Test Estate 1 | Voucher      | 10.00             |                       |                             | Hospital 1 Contract | 10 KES         | test@recipient.co.uk |                 |
	| 2022-01-06 | 6                 | Sale            | 1                  | Test Merchant 2 | 123456781        | Test Estate 1 | Voucher      | 10.00             |                       |                             | Hospital 1 Contract | 10 KES         |                      | 123456789       |
	| 2022-01-06 | 7                 | Sale            | 2                  | Test Merchant 3 | 123456782        | Test Estate 1 | Voucher      | 10.00             |                       |                             | Hospital 1 Contract | 10 KES         | test@recipient.co.uk |                 |
	| 2022-01-06 | 8                 | Sale            | 1                  | Test Merchant 3 | 123456782        | Test Estate 1 | Voucher      | 10.00             |                       |                             | Hospital 1 Contract | 10 KES         | test@recipient.co.uk |                 |
	
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage |
	| Test Estate 1 | Test Merchant 1 | 1                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 2 | 2                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 3 | 3                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 1 | 4                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 1 | 5                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 2 | 6                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 3 | 7                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 3 | 8                 | 0000         | SUCCESS         |
		
	When I get the pending settlements the following information should be returned
	| SettlementDate | EstateName    | MerchantName    | NumberOfFees |
	| 2022-01-13     | Test Estate 1 | Test Merchant 2 | 1            |
	| 2022-02-06     | Test Estate 1 | Test Merchant 3 | 1            |

	When I get the completed settlements the following information should be returned
	| SettlementDate | EstateName    | MerchantName    | NumberOfFees |
	| 2022-01-06     | Test Estate 1 | Test Merchant 1 | 2            |

@PRTest
Scenario: Process Settlement
	Given I create the following merchants
	| MerchantName    | AddressLine1   | Town     | Region      | Country        | ContactName    | EmailAddress                 | EstateName    | SettlementSchedule |
	| Test Merchant 1 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 1 | testcontact1@merchant1.co.uk | Test Estate 1 | Immediate             |
	| Test Merchant 2 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 2 | testcontact2@merchant2.co.uk | Test Estate 1 | Weekly             |

	Given I have assigned the following  operator to the merchants
	| OperatorName | MerchantName    | MerchantNumber | TerminalNumber | EstateName    |
	| Safaricom    | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |
	| Voucher      | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |
	| Safaricom    | Test Merchant 2 | 00000002       | 10000002       | Test Estate 1 |
	| Voucher      | Test Merchant 2 | 00000002       | 10000002       | Test Estate 1 |

	Given I have assigned the following devices to the merchants
	| DeviceIdentifier | MerchantName    | EstateName    |
	| 123456780        | Test Merchant 1 | Test Estate 1 |
	| 123456781        | Test Merchant 2 | Test Estate 1 |

	Given I make the following manual merchant deposits 
	| Reference | Amount | DateTime | MerchantName    | EstateName    |
	| Deposit1  | 210.00 | Today    | Test Merchant 1 | Test Estate 1 |
	| Deposit1  | 110.00 | Today    | Test Merchant 2 | Test Estate 1 |

	When I perform the following transactions
	| DateTime   | TransactionNumber | TransactionType | TransactionSource | MerchantName    | DeviceIdentifier | EstateName    | OperatorName | TransactionAmount | CustomerAccountNumber | CustomerEmailAddress        | ContractDescription | ProductName    | RecipientEmail       | RecipientMobile |
	| 2022-01-06 | 1                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 100.00            | 123456789             |                             | Safaricom Contract  | Variable Topup |                      |                 |
	| 2022-01-06 | 2                 | Sale            | 1                 | Test Merchant 2 | 123456781        | Test Estate 1 | Safaricom    | 100.00            | 123456789             |                             | Safaricom Contract  | Variable Topup |                      |                 |
	| 2022-01-06 | 4                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 100.00            | 123456789             | testcustomer@customer.co.uk | Safaricom Contract  | Variable Topup |                      |                 |
	| 2022-01-06 | 5                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Voucher      | 10.00             |                       |                             | Hospital 1 Contract | 10 KES         | test@recipient.co.uk |                 |
	| 2022-01-06 | 6                 | Sale            | 1                 | Test Merchant 2 | 123456781        | Test Estate 1 | Voucher      | 10.00             |                       |                             | Hospital 1 Contract | 10 KES         |                      | 123456789       |
	
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage |
	| Test Estate 1 | Test Merchant 1 | 1                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 2 | 2                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 1 | 4                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 1 | 5                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 2 | 6                 | 0000         | SUCCESS         |

	When I get the pending settlements the following information should be returned
	| SettlementDate | EstateName    | MerchantName    | NumberOfFees |
	| 2022-01-13     | Test Estate 1 | Test Merchant 2 | 1            |

	When I process the settlement for '2022-01-13' on Estate 'Test Estate 1' for Merchant 'Test Merchant 2' then 1 fees are marked as settled and the settlement is completed


