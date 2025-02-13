@base @shared
Feature: SettlementReporting

Background: 

	Given I create the following api scopes
	| Name                 | DisplayName                       | Description                            |
	| estateManagement     | Estate Managememt REST Scope      | A scope for Estate Managememt REST     |
	| transactionProcessor | Transaction Processor REST  Scope | A scope for Transaction Processor REST |

	Given the following api resources exist
	| Name     | DisplayName            | Secret  | Scopes           | UserClaims                 |
	| estateManagement | Estate Managememt REST | Secret1 | estateManagement | MerchantId, EstateId, role |
	| transactionProcessor | Transaction Processor REST | Secret1 | transactionProcessor |  |

	Given the following clients exist
	| ClientId      | ClientName     | Secret  | Scopes    | GrantTypes  |
	| serviceClient | Service Client | Secret1 | estateManagement,transactionProcessor | client_credentials |

	Given I have a token to access the estate management and transaction processor resources
	| ClientId      | 
	| serviceClient | 

	Given I have created the following estates
	| EstateName    |
	| Test Estate 1 |
	| Test Estate 2 |

	Given I have created the following operators
	| EstateName    | OperatorName | RequireCustomMerchantNumber | RequireCustomTerminalNumber |
	| Test Estate 1 | Safaricom    | True                        | True                        |
	| Test Estate 2 | Safaricom    | True                        | True                        |

	And I have assigned the following operators to the estates
	| EstateName    | OperatorName    | 
	| Test Estate 1 | Safaricom |
	| Test Estate 2 | Safaricom |

	Given I create a contract with the following values
	| EstateName    | OperatorName    | ContractDescription |
	| Test Estate 1 | Safaricom | Safaricom Contract |
	| Test Estate 2 | Safaricom | Safaricom Contract |

	When I create the following Products
	| EstateName   | OperatorName | ContractDescription | ProductName    | DisplayText | Value | ProductType |
	| Test Estate 1 | Safaricom    | Safaricom Contract  | Variable Topup | Custom      |       | MobileTopup |
	| Test Estate 2 | Safaricom    | Safaricom Contract  | Variable Topup | Custom      |       | MobileTopup |

	When I add the following Transaction Fees
	| EstateName   | OperatorName | ContractDescription | ProductName    | CalculationType | FeeDescription      | Value | FeeType  |
	| Test Estate 1 | Safaricom    | Safaricom Contract  | Variable Topup | Percentage      | Merchant Commission | 0.50  | Merchant |
	| Test Estate 2 | Safaricom    | Safaricom Contract  | Variable Topup | Percentage      | Merchant Commission | 0.85  | Merchant |

	Given I create the following merchants
	| MerchantName    | AddressLine1   | Town     | Region      | Country        | ContactName    | EmailAddress                 | EstateName    | SettlementSchedule |
	| Test Merchant 1 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 1 | testcontact1@merchant1.co.uk | Test Estate 1 | Weekly          |
	| Test Merchant 2 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 2 | testcontact2@merchant2.co.uk | Test Estate 1 | Weekly             |
	| Test Merchant 3 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 3 | testcontact3@merchant2.co.uk | Test Estate 2 | Monthly            |

	Given I have assigned the following operator to the merchants
	| OperatorName | MerchantName    | MerchantNumber | TerminalNumber | EstateName    |
	| Safaricom    | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |
	| Safaricom    | Test Merchant 2 | 00000002       | 10000002       | Test Estate 1 |
	| Safaricom    | Test Merchant 3 | 00000003       | 10000003       | Test Estate 2 |

	Given I have assigned the following devices to the merchants
	| DeviceIdentifier | MerchantName    | EstateName    |
	| 123456780        | Test Merchant 1 | Test Estate 1 |
	| 123456781        | Test Merchant 2 | Test Estate 1 |
	| 123456782        | Test Merchant 3 | Test Estate 2 |

	When I add the following contracts to the following merchants
	| EstateName    | MerchantName    | ContractDescription          |
	| Test Estate 1 | Test Merchant 1 | Safaricom Contract |
	| Test Estate 1 | Test Merchant 2 | Safaricom Contract |
	| Test Estate 2 | Test Merchant 3 | Safaricom Contract |

	Given I make the following manual merchant deposits 
	| Reference | Amount   | DateTime | MerchantName    | EstateName    |
	| Deposit1  | 50000.00 | Today    | Test Merchant 1 | Test Estate 1 |
	| Deposit1  | 50000.00 | Today    | Test Merchant 2 | Test Estate 1 |
	| Deposit1  | 50000.00 | Today    | Test Merchant 3 | Test Estate 2 |

	When I perform the following transactions
	| DateTime   | TransactionNumber | TransactionType | TransactionSource | MerchantName    | DeviceIdentifier | EstateName   | OperatorName | TransactionAmount | CustomerAccountNumber | CustomerEmailAddress | ContractDescription | ProductName    |
	| 2022-01-06 | 1                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 100.00            | 123456789             |                      | Safaricom Contract  | Variable Topup |
	| 2022-01-06 | 2                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 5.00              | 123456789             |                      | Safaricom Contract  | Variable Topup |
	| 2022-01-06 | 3                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 25.00             | 123456789             |                      | Safaricom Contract  | Variable Topup |
	| 2022-01-06 | 4                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 150.00            | 123456789             |                      | Safaricom Contract  | Variable Topup |
	| 2022-01-06 | 5                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 3.00              | 123456789             |                      | Safaricom Contract  | Variable Topup |
	| 2022-01-06 | 6                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 40.00             | 123456789             |                      | Safaricom Contract  | Variable Topup |
	| 2022-01-06 | 7                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 60.00             | 123456789             |                      | Safaricom Contract  | Variable Topup |
	| 2022-01-06 | 8                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Safaricom    | 101.00            | 123456789             |                      | Safaricom Contract  | Variable Topup |
																															  
	| 2022-01-06 | 1                 | Sale            | 1                 | Test Merchant 2 | 123456781        | Test Estate 1 | Safaricom    | 100.00            | 123456789             |                      | Safaricom Contract  | Variable Topup |
	| 2022-01-06 | 2                 | Sale            | 1                 | Test Merchant 2 | 123456781        | Test Estate 1 | Safaricom    | 5.00              | 123456789             |                      | Safaricom Contract  | Variable Topup |
	| 2022-01-06 | 3                 | Sale            | 1                 | Test Merchant 2 | 123456781        | Test Estate 1 | Safaricom    | 25.00             | 123456789             |                      | Safaricom Contract  | Variable Topup |
	| 2022-01-06 | 4                 | Sale            | 1                 | Test Merchant 2 | 123456781        | Test Estate 1 | Safaricom    | 15.00             | 123456789             |                      | Safaricom Contract  | Variable Topup |
																															  
	| 2022-01-06 | 1                 | Sale            | 1                 | Test Merchant 3 | 123456782        | Test Estate 2 | Safaricom    | 100.00            | 123456789             |                      | Safaricom Contract  | Variable Topup |

	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage |
	| Test Estate 1 | Test Merchant 1 | 1                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 1 | 2                 | 1008         | DECLINED BY OPERATOR         |
	| Test Estate 1 | Test Merchant 1 | 3                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 1 | 4                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 1 | 5                 | 1008         | DECLINED BY OPERATOR         |
	| Test Estate 1 | Test Merchant 1 | 6                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 1 | 7                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 1 | 8                 | 0000         | SUCCESS         |
				  
	| Test Estate 1 | Test Merchant 2 | 1                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 2 | 2                 | 1008         | DECLINED BY OPERATOR         |
	| Test Estate 1 | Test Merchant 2 | 3                 | 0000         | SUCCESS         |
	| Test Estate 1 | Test Merchant 2 | 4                 | 0000         | SUCCESS         |
				  
	| Test Estate 2 | Test Merchant 3 | 1                 | 0000         | SUCCESS |

	When I get the pending settlements the following information should be returned
	| SettlementDate | EstateName   | MerchantName | NumberOfFees |
	| 2022-01-13     | Test Estate 1 | Test Merchant 1             | 6            |
	| 2022-01-13     | Test Estate 1 | Test Merchant 2             | 3            |

	When I process the settlement for '2022-01-13' on Estate 'Test Estate 1' for Merchant 'Test Merchant 1' then 6 fees are marked as settled and the settlement is completed

	When I process the settlement for '2022-01-13' on Estate 'Test Estate 1' for Merchant 'Test Merchant 2' then 3 fees are marked as settled and the settlement is completed

	When I get the pending settlements the following information should be returned
	| SettlementDate | EstateName   | MerchantName    | NumberOfFees |
	| 2022-02-06     | Test Estate 2 | Test Merchant 3 | 1            |

	When I process the settlement for '2022-02-06' on Estate 'Test Estate 2' for Merchant 'Test Merchant 3' then 1 fees are marked as settled and the settlement is completed

@settlement
@PRTest
Scenario: Get Settlements - Merchant Filter
	When I get the Estate Settlement Report for Estate 'Test Estate 1' for Merchant 'Test Merchant 1' with the Start Date '2022-01-13' and the End Date '2022-02-06' the following data is returned
	| SettlementDate | NumberOfFeesSettled | ValueOfFeesSettled | IsCompleted |
	| 2022-01-13     | 6                   | 2.39 | True        |

	When I get the Estate Settlement Report for Estate 'Test Estate 1' for Merchant 'Test Merchant 2' with the Start Date '2022-01-13' and the End Date '2022-02-06' the following data is returned
	| SettlementDate | NumberOfFeesSettled | ValueOfFeesSettled | IsCompleted |
	| 2022-01-13     | 3                   | 0.71              | True        |

	When I get the Estate Settlement Report for Estate 'Test Estate 2' for Merchant 'Test Merchant 3' with the Start Date '2022-01-13' and the End Date '2022-02-06' the following data is returned
	| SettlementDate | NumberOfFeesSettled | ValueOfFeesSettled | IsCompleted |
	| 2022-02-06     | 1                   | 0.85               | True        |
	
	When I get the Estate Settlement Report for Estate 'Test Estate 1' for Merchant 'Test Merchant 1' with the Date '2022-01-13' the following fees are settled
	| FeeDescription      | IsSettled | Operator  | CalculatedValue |
	| Merchant Commission | True      | Safaricom | 0.50            |
	| Merchant Commission | True      | Safaricom | 0.13           |
	| Merchant Commission | True      | Safaricom | 0.75            |
	| Merchant Commission | True      | Safaricom | 0.20            |
	| Merchant Commission | True      | Safaricom | 0.30            |
	| Merchant Commission | True      | Safaricom | 0.51            |

	When I get the Estate Settlement Report for Estate 'Test Estate 1' for Merchant 'Test Merchant 2' with the Date '2022-01-13' the following fees are settled
	| FeeDescription      | IsSettled | Operator  | CalculatedValue |
	| Merchant Commission | True      |  Safaricom | 0.50             |
	| Merchant Commission | True      |  Safaricom | 0.13            |
	| Merchant Commission | True      |  Safaricom | 0.08             |
	
	When I get the Estate Settlement Report for Estate 'Test Estate 2' for Merchant 'Test Merchant 3' with the Date '2022-02-06' the following fees are settled
	| FeeDescription      | IsSettled | Operator  | CalculatedValue |
	| Merchant Commission | True      | Safaricom | 0.85            |