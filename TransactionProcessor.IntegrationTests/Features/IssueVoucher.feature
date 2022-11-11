@base @shared
Feature: IssueVoucher

Background: 

	Given I create the following api scopes
	| Name              | DisplayName                    | Description                         |
	| estateManagement  | Estate Managememt REST Scope   | A scope for Estate Managememt REST  |
	| voucherManagement | Voucher Management REST  Scope | A scope for Voucher Management REST |

	Given the following api resources exist
	| ResourceName     | DisplayName            | Secret  | Scopes           | UserClaims                 |
	| estateManagement | Estate Managememt REST | Secret1 | estateManagement | MerchantId, EstateId, role |
	| voucherManagement | Voucher Management REST | Secret1 | voucherManagement |  |

	Given the following clients exist
	| ClientId      | ClientName     | Secret  | AllowedScopes    | AllowedGrantTypes  |
	| serviceClient | Service Client | Secret1 | estateManagement,voucherManagement | client_credentials |

	Given I have a token to access the estate management and transaction processor resources
	| ClientId      | 
	| serviceClient | 

	Given I have created the following estates
	| EstateName    |
	| Test Estate 1 |
	| Test Estate 2 |

	Given I have created the following operators
	| EstateName    | OperatorName    | RequireCustomMerchantNumber | RequireCustomTerminalNumber |
	| Test Estate 1 | Test Operator 1 | True                        | True                        |

@PRTest
Scenario: Issue Vouchers
	When I issue the following vouchers
	| EstateName    | OperatorName    | Value | TransactionId                        | RecipientEmail                 | RecipientMobile |
	| Test Estate 1 | Test Operator 1 | 10.00 | 19f2776a-4230-40d4-8cd2-3649e18732e0 | testrecipient1@recipient.co.uk |                 |
	| Test Estate 1 | Test Operator 1 | 20.00 | 6351e047-8f31-4472-a294-787caa5fb738 |                                | 123456788       |