@base @shared
Feature: Contract

Background: 
	Given the following security roles exist
	| Role Name |
	| Estate   |

	Given I create the following api scopes
	| Name             | DisplayName                  | Description                        |
	| estateManagement | Estate Managememt REST Scope | A scope for Estate Managememt REST |

	Given the following api resources exist
	| Name     | DisplayName            | Secret  | Scopes           | UserClaims                 |
	| estateManagement | Estate Managememt REST | Secret1 | estateManagement | merchantId, estateId, role |

	Given the following clients exist
	| ClientId      | ClientName     | Secret  | Scopes    | GrantTypes  |
	| serviceClient | Service Client | Secret1 | estateManagement | client_credentials |
	| estateClient  | Estate Client  | Secret1 | estateManagement | password           |

	Given I have a token to access the estate management resource
	| ClientId      | 
	| serviceClient |

	Given I have created the following estates
	| EstateName    |
	| Test Estate 1 |
	| Test Estate 2 |
	
	Given I have created the following operators
	| EstateName    | OperatorName    | RequireCustomMerchantNumber | RequireCustomTerminalNumber |
	| Test Estate 1 | Test Operator 1 | True                        | True                        |
	| Test Estate 2 | Test Operator 1 | True                        | True                        |

	And I have assigned the following operators to the estates
	| EstateName    | OperatorName    | 
	| Test Estate 1 | Test Operator 1 |
	| Test Estate 2 | Test Operator 1 |

	Given I have created the following security users
	| EmailAddress                  | Password | GivenName  | FamilyName | EstateName    |
	| estateuser1@testestate1.co.uk | 123456   | TestEstate | User1      | Test Estate 1 |
	| estateuser1@testestate2.co.uk | 123456   | TestEstate | User1      | Test Estate 2 |

Scenario: Get Merchant Contracts

	Given I create the following merchants
	| MerchantName    | AddressLine1   | Town     | Region      | Country        | ContactName    | EmailAddress                 | EstateName    |
	| Test Merchant 1 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 1 | testcontact1@merchant1.co.uk | Test Estate 1 |
	| Test Merchant 2 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 1 | testcontact1@merchant2.co.uk | Test Estate 2 |
	
	Given I create a contract with the following values
	| EstateName    | OperatorName    | ContractDescription |
	| Test Estate 1 | Test Operator 1 | Operator 1 Contract Estate 1 |
	| Test Estate 2 | Test Operator 1 | Operator 1 Contract Estate 2 |

	When I create the following Products
	| EstateName    | OperatorName    | ContractDescription          | ProductName      | DisplayText | Value  | ProductType |
	| Test Estate 1 | Test Operator 1 | Operator 1 Contract Estate 1 | 100 KES Topup    | 100 KES     | 100.00 | MobileTopup |
	| Test Estate 1 | Test Operator 1 | Operator 1 Contract Estate 1 | Variable Topup 1 | Custom      |        | MobileTopup |
	| Test Estate 2 | Test Operator 1 | Operator 1 Contract Estate 2 | 200 KES Topup    | 100 KES     | 100.00 | MobileTopup |
	| Test Estate 2 | Test Operator 1 | Operator 1 Contract Estate 2 | Variable Topup 2 | Custom      |        | MobileTopup |

	When I add the following Transaction Fees
	| EstateName    | OperatorName    | ContractDescription          | ProductName      | CalculationType | FeeDescription      | Value | FeeType  |
	| Test Estate 1 | Test Operator 1 | Operator 1 Contract Estate 1 | 100 KES Topup    | Fixed           | Merchant Commission | 1.00  | Merchant |
	| Test Estate 1 | Test Operator 1 | Operator 1 Contract Estate 1 | 100 KES Topup    | Percentage      | Merchant Commission | 0.015 | Merchant |
	| Test Estate 1 | Test Operator 1 | Operator 1 Contract Estate 1 | Variable Topup 1 | Fixed           | Merchant Commission | 1.50  | Merchant |
	| Test Estate 2 | Test Operator 1 | Operator 1 Contract Estate 2 | 200 KES Topup    | Percentage      | Merchant Commission | 0.25  | Merchant |
	| Test Estate 2 | Test Operator 1 | Operator 1 Contract Estate 2 | Variable Topup 2 | Percentage      | Merchant Commission | 2.25  | Merchant |

	When I add the following contracts to the following merchants
	| EstateName    | MerchantName    | ContractDescription          |
	| Test Estate 1 | Test Merchant 1 | Operator 1 Contract Estate 1 |
	| Test Estate 2 | Test Merchant 2 | Operator 1 Contract Estate 2 |

	Then I get the Contracts for 'Test Estate 1' the following contract details are returned
	| ContractDescription          | ProductName      |
	| Operator 1 Contract Estate 1 | 100 KES Topup    |
	| Operator 1 Contract Estate 1 | Variable Topup 1 |

	Then I get the Contracts for 'Test Estate 2' the following contract details are returned
	| ContractDescription          | ProductName      |
	| Operator 1 Contract Estate 2 | 200 KES Topup    |
	| Operator 1 Contract Estate 2 | Variable Topup 2 |

	Then I get the Merchant Contracts for 'Test Merchant 1' for 'Test Estate 1' the following contract details are returned
	| ContractDescription | ProductName    |
	| Operator 1 Contract Estate 1 | 100 KES Topup  |
	| Operator 1 Contract Estate 1 | Variable Topup 1|

	Then I get the Merchant Contracts for 'Test Merchant 2' for 'Test Estate 2' the following contract details are returned
	| ContractDescription          | ProductName      |
	| Operator 1 Contract Estate 2 | 200 KES Topup    |
	| Operator 1 Contract Estate 2 | Variable Topup 2 |

	Then I get the Transaction Fees for '100 KES Topup' on the 'Operator 1 Contract Estate 1' contract for 'Test Estate 1' the following fees are returned
	| CalculationType | FeeDescription      | Value | FeeType  |
	| Fixed           | Merchant Commission | 1.00  | Merchant |
	| Percentage      | Merchant Commission | 0.015 | Merchant |

	Then I get the Transaction Fees for 'Variable Topup 1' on the 'Operator 1 Contract Estate 1' contract for 'Test Estate 1' the following fees are returned
	| CalculationType | FeeDescription      | Value | FeeType  |
	| Fixed           | Merchant Commission | 1.50  | Merchant |
													  
	Then I get the Transaction Fees for '200 KES Topup' on the 'Operator 1 Contract Estate 2' contract for 'Test Estate 2' the following fees are returned
	| CalculationType | FeeDescription      | Value | FeeType  |
	| Percentage      | Merchant Commission | 0.25  | Merchant |

	Then I get the Transaction Fees for 'Variable Topup 2' on the 'Operator 1 Contract Estate 2' contract for 'Test Estate 2' the following fees are returned
	| CalculationType | FeeDescription      | Value | FeeType  |
	| Percentage      | Merchant Commission | 2.25  | Merchant |


Scenario: Prevent Duplicate Contracts
	
	Given I create a contract with the following values
	| EstateName    | OperatorName    | ContractDescription |
	| Test Estate 1 | Test Operator 1 | Operator 1 Contract |

	When I create another contract with the same values it should be rejected
	| EstateName    | OperatorName    | ContractDescription |
	| Test Estate 1 | Test Operator 1 | Operator 1 Contract |
	