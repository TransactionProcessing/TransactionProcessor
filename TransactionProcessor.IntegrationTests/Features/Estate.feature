@base @shared
Feature: Estate

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

Scenario: Get Estate
	Given I have created the following estates
	| EstateName    |
	| Test Estate 1 |
	And I have created the following operators
	| EstateName    | OperatorName    | RequireCustomMerchantNumber | RequireCustomTerminalNumber |
	| Test Estate 1 | Test Operator 1 | True                        | True                        |
	| Test Estate 1 | Test Operator 2 | True                        | True                        |
	And I have assigned the following operators to the estates
	| EstateName    | OperatorName    | 
	| Test Estate 1 | Test Operator 1 |
	| Test Estate 1 | Test Operator 2 |
	And I have created the following security users
	| EmailAddress                  | Password | GivenName  | FamilyName | EstateName    |
	| estateuser1@testestate1.co.uk | 123456   | TestEstate | User1      | Test Estate 1 |
	| estateuser2@testestate1.co.uk | 123456   | TestEstate | User2      | Test Estate 1 |
	When I get the estate "Test Estate 1" the estate details are returned as follows
	| EstateName    |
	| Test Estate 1 |
	When I get the estate "Test Estate 1" the estate operator details are returned as follows
	| OperatorName    |
	| Test Operator 1 |
	| Test Operator 2 |
	When I get the estate "Test Estate 1" the estate security user details are returned as follows
	| EmailAddress                  |
	| estateuser1@testestate1.co.uk |
	| estateuser2@testestate1.co.uk |
	When I get the estate "Test Estate 2" an error is returned
	Given I am logged in as "estateuser1@testestate1.co.uk" with password "123456" for Estate "Test Estate 1" with client "estateClient"
	When I get the estate "Test Estate 1" the estate details are returned as follows
	| EstateName    |
	| Test Estate 1 |
	When I get the estate "Test Estate 1" the estate operator details are returned as follows
	| OperatorName    |
	| Test Operator 1 |
	| Test Operator 2 |
	When I get the estate "Test Estate 1" the estate security user details are returned as follows
	| EmailAddress                  |
	| estateuser1@testestate1.co.uk |
	| estateuser2@testestate1.co.uk |
	When I get the estate "Test Estate 2" an error is returned

Scenario: Update Estate
	Given I have created the following estates
	| EstateName    |
	| Test Estate 1 |
	And I have created the following operators
	| EstateName    | OperatorName    | RequireCustomMerchantNumber | RequireCustomTerminalNumber |
	| Test Estate 1 | Test Operator 1 | True                        | True                        |
	| Test Estate 1 | Test Operator 2 | True                        | True                        |
	And I have assigned the following operators to the estates
	| EstateName    | OperatorName    | 
	| Test Estate 1 | Test Operator 1 |
	| Test Estate 1 | Test Operator 2 |
	And I have created the following security users
	| EmailAddress                  | Password | GivenName  | FamilyName | EstateName    |
	| estateuser1@testestate1.co.uk | 123456   | TestEstate | User1      | Test Estate 1 |
	| estateuser2@testestate1.co.uk | 123456   | TestEstate | User2      | Test Estate 1 |
	When I remove the operator 'Test Operator 1' from estate 'Test Estate 1' the operator is removed