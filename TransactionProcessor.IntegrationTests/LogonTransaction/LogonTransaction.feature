@base @shared
Feature: LogonTransaction

Background: 
	Given I have created the following estates
	| EstateName    |
	| Test Estate 1 |
	| Test Estate 2 |

	Given I create the following merchants
	| MerchantName    | AddressLine1   | Town     | Region      | Country        | ContactName    | EmailAddress                 | EstateName    |
	| Test Merchant 1 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 1 | testcontact1@merchant1.co.uk | Test Estate 1 |
	| Test Merchant 2 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 2 | testcontact2@merchant2.co.uk | Test Estate 1 |
	| Test Merchant 3 | Address Line 1 | TestTown | Test Region | United Kingdom | Test Contact 3 | testcontact3@merchant2.co.uk | Test Estate 2 |

@PRTest
Scenario: Logon Transaction
	When I perform the following transactions
	| DateTime | TransactionNumber | TransactionType | MerchantName    | IMEINumber |
	| Today    | 1                 | Logon           | Test Merchant 1 | 123456789  |
	| Today    | 2                 | Logon           | Test Merchant 2 | 123456789  |
	| Today    | 3                 | Logon           | Test Merchant 3 | 123456789  |
	
	Then transaction response should contain the following information
	| TransactionNumber | ResponseCode | ResponseMessage |
	| 1                 | 0000         | SUCCESS         |
	| 2                 | 0000         | SUCCESS         |
	| 3                 | 0000         | SUCCESS         |
