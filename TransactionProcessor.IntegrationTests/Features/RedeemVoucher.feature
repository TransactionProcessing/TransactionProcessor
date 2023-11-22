@base @shared
Feature: RedeemVoucher
	Simple calculator for adding two numbers

Background: 

	Given I create the following api scopes
	| Name              | DisplayName                    | Description                         |
	| estateManagement  | Estate Managememt REST Scope   | A scope for Estate Managememt REST  |
	| voucherManagement | Voucher Management REST  Scope | A scope for Voucher Management REST |

	Given the following api resources exist
	| Name     | DisplayName            | Secret  | Scopes           | UserClaims                 |
	| estateManagement | Estate Managememt REST | Secret1 | estateManagement | MerchantId, EstateId, role |
	| voucherManagement | Voucher Management REST | Secret1 | voucherManagement |  |

	Given the following clients exist
	| ClientId      | ClientName     | Secret  | Scopes    | GrantTypes  |
	| serviceClient | Service Client | Secret1 | estateManagement,voucherManagement | client_credentials |

	Given I have a token to access the estate management and transaction processor resources
	| ClientId      | 
	| serviceClient | 

	Given I have created the following estates
	| EstateName    |
	| Test Estate 1 |
	| Test Estate 2 |

	Given I have created the following operators
	| EstateName    | OperatorName     | RequireCustomMerchantNumber | RequireCustomTerminalNumber |
	| Test Estate 1 | Voucher          | True                        | True                        |

	Given I create a contract with the following values
	| EstateName    | OperatorName     | ContractDescription       |
	| Test Estate 1 | Voucher          | Hospital 1 Contract       |

	When I create the following Products
	| EstateName    | OperatorName | ContractDescription | ProductName | DisplayText | Value | ProductType |
	| Test Estate 1 | Voucher      | Hospital 1 Contract | 10 KES      | 10 KES      |       | Voucher     |

	Given I create the following merchants
	| MerchantName    | AddressLine1   | Town     | Region      | Country        | ContactName    | EmailAddress                 | EstateName    |
	| Test Merchant 1 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 1 | testcontact1@merchant1.co.uk | Test Estate 1 |

	Given I have assigned the following  operator to the merchants
	| OperatorName     | MerchantName    | MerchantNumber | TerminalNumber | EstateName    |
	| Voucher          | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |

	Given I have assigned the following devices to the merchants
	| DeviceIdentifier | MerchantName    | EstateName    |
	| 123456780        | Test Merchant 1 | Test Estate 1 |

	Given I make the following manual merchant deposits 
	| Reference | Amount  | DateTime | MerchantName    | EstateName    |
	| Deposit1  | 20.00 | Today    | Test Merchant 1 | Test Estate 1 |

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | TransactionSource | MerchantName    | DeviceIdentifier | EstateName    | OperatorName     | TransactionAmount | CustomerAccountNumber | CustomerEmailAddress        | ContractDescription       | ProductName       | RecipientEmail       | RecipientMobile | MessageType   | AccountNumber | CustomerName     |
	| Today    | 1                 | Sale            | 1                 | Test Merchant 1 | 123456780        | Test Estate 1 | Voucher          | 10.00             |                       |                             | Hospital 1 Contract       | 10 KES            | test@recipient.co.uk |                 |               |               |                  |
	

@PRTest
Scenario: Redeem Vouchers
	When I redeem the voucher for Estate 'Test Estate 1' and Merchant 'Test Merchant 1' transaction number 1 the voucher balance will be 0