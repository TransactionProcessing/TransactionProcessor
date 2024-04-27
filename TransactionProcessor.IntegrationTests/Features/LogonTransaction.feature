@base @shared
Feature: LogonTransaction

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

	Given I have created the following operators
	| EstateName    | OperatorName    | RequireCustomMerchantNumber | RequireCustomTerminalNumber |
	| Test Estate 1 | Test Operator 1 | True                        | True                        |

	And I have assigned the following operators to the estates
	| EstateName    | OperatorName    | 
	| Test Estate 1 | Test Operator 1 |

	Given I create the following merchants
	| MerchantName    | AddressLine1   | Town     | Region      | Country        | ContactName    | EmailAddress                 | EstateName    |
	| Test Merchant 1 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 1 | testcontact1@merchant1.co.uk | Test Estate 1 |
	| Test Merchant 2 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 2 | testcontact2@merchant2.co.uk | Test Estate 1 |
	| Test Merchant 3 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 3 | testcontact3@merchant2.co.uk | Test Estate 1 |
	| Test Merchant 4 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 4 | testcontact4@merchant2.co.uk | Test Estate 1 |
	| Test Merchant 5 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 5 | testcontact5@merchant2.co.uk | Test Estate 1 |
	| Test Merchant 6 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 6 | testcontact6@merchant2.co.uk | Test Estate 1 |
	| Test Merchant 7 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 7 | testcontact7@merchant2.co.uk | Test Estate 1 |

	Given I have assigned the following  operator to the merchants
	| OperatorName    | MerchantName    | MerchantNumber | TerminalNumber | EstateName    |
	| Test Operator 1 | Test Merchant 1 | 00000001       | 10000001       | Test Estate 1 |
	| Test Operator 1 | Test Merchant 2 | 00000001       | 10000001       | Test Estate 1 |
	| Test Operator 1 | Test Merchant 3 | 00000001       | 10000001       | Test Estate 1 |
	| Test Operator 1 | Test Merchant 4 | 00000001       | 10000001       | Test Estate 1 |
	| Test Operator 1 | Test Merchant 5 | 00000001       | 10000001       | Test Estate 1 |
	| Test Operator 1 | Test Merchant 6 | 00000001       | 10000001       | Test Estate 1 |
	| Test Operator 1 | Test Merchant 7 | 00000001       | 10000001       | Test Estate 1 |

@PRTest
Scenario: Logon Transactions

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | MerchantName    | DeviceIdentifier | EstateName    |
	| Today    | 1                 | Logon           | Test Merchant 1 | 123456780  | Test Estate 1 |
	| Today    | 2                 | Logon           | Test Merchant 2 | 123456781  | Test Estate 1 |
	| Today    | 3                 | Logon           | Test Merchant 3 | 123456782  | Test Estate 1 |
	
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage |
	| Test Estate 1 | Test Merchant 1 | 1                 | 0001         | SUCCESS         |
	| Test Estate 1 | Test Merchant 2 | 2                 | 0001         | SUCCESS         |
	| Test Estate 1 | Test Merchant 3 | 3                 | 0001         | SUCCESS         |

	Given I have assigned the following devices to the merchants
	| DeviceIdentifier | MerchantName    | MerchantNumber | EstateName    |
	| 123456783        | Test Merchant 4 | 00000001       | Test Estate 1 |

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | MerchantName    | DeviceIdentifier | EstateName    |
	| Today    | 4                 | Logon           | Test Merchant 4 | 123456783        | Test Estate 1 |
	
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage |
	| Test Estate 1 | Test Merchant 4 | 4                 | 0000         | SUCCESS         |

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | MerchantName    | DeviceIdentifier | EstateName    |
	| Today    | 5                 | Logon           | Test Merchant 1 | 13579135         | Test Estate 1 |

	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage                                                    |
	| Test Estate 1 | Test Merchant 1 | 5                 | 1000         | Device Identifier 13579135 not valid for Merchant Test Merchant 1 |

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | MerchantName    | DeviceIdentifier | EstateName    |
	| Today    | 6                 | Logon           | Test Merchant 1 | 123456785       | InvalidEstate |
	
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage                                                        |
	| InvalidEstate | Test Merchant 1 | 6                 | 1001         | Estate Id [79902550-64df-4491-b0c1-4e78943928a3] is not a valid estate |

	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | MerchantName    | DeviceIdentifier | EstateName    |
	| Today    | 7                 | Logon           | InvalidMerchant | 123456786        | Test Estate 1 |
	
	Then transaction response should contain the following information
	| EstateName    | MerchantName    | TransactionNumber | ResponseCode | ResponseMessage                                                                                       |
	| Test Estate 1 | InvalidMerchant | 7                 | 1002         | Merchant Id [d59320fa-4c3e-4900-a999-483f6a10c69a] is not a valid merchant for estate [Test Estate 1] |