using TransactionProcessor.DataTransferObjects.Responses.Contract;

namespace TransactionProcessor.IntegrationTesting.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class EstateDetails
    {
        private EstateDetails(Guid estateId, String estateName, String estateReference)
        {
            this.EstateId = estateId;
            this.EstateName = estateName;
            this.EstateReference = estateReference;
            this.Merchants = new List<TransactionProcessor.DataTransferObjects.Responses.Merchant.MerchantResponse>();
            this.Operators=new Dictionary<String, Guid>();
            this.AssignedOperators = new List<Guid>();
            this.MerchantUsers = new Dictionary<String, Dictionary<String, String>>();
            this.Contracts = new List<Contract>();
            this.TransactionResponses = new Dictionary<(Guid merchantId, String transactionNumber), String>();
        }

        private Dictionary<(Guid merchantId, String transactionNumber), String> TransactionResponses { get; }

        public Guid GetMerchantId(String merchantName)
        {
            if (merchantName == "InvalidMerchant")
            {
                return Guid.Parse("D59320FA-4C3E-4900-A999-483F6A10C69A");
            }

            return this.Merchants.Single(m => m.MerchantName == merchantName).MerchantId;
        }

        public void AddTransactionResponse(Guid merchantId,
                                           String transactionNumber,
                                           String transactionResponse)
        {
            this.TransactionResponses.Add((merchantId, transactionNumber), transactionResponse);
        }

        public String EstateUser { get; private set; }
        public String EstatePassword { get; private set; }

        public String AccessToken { get; private set; }

        public static EstateDetails Create(Guid estateId,
                                           String estateName,
                                           String estateReference)
        {
            return new EstateDetails(estateId,estateName, estateReference);
        }

        public String GetTransactionResponse(Guid merchantId,
                                             String transactionNumber)
        {
            KeyValuePair<(Guid merchantId, String transactionNumber), String> transactionResponse =
                this.TransactionResponses.SingleOrDefault(t => t.Key.merchantId == merchantId && t.Key.transactionNumber == transactionNumber);

            return transactionResponse.Value;
        }

        public Dictionary<String,Guid> GetOperators() {
            return this.Operators;
        }

        public void AddOperator(Guid operatorId,
                                String operatorName)
        {
            this.Operators.Add(operatorName,operatorId);
        }

        public void UpdateOperator(Guid operatorId,
                                String newOperatorName)
        {
            KeyValuePair<String, Guid> originalOperator = this.Operators.SingleOrDefault(o => o.Value == operatorId);

            this.Operators.Remove(originalOperator.Key);
            this.Operators.Add(newOperatorName, operatorId);
        }

        public void AddAssignedOperator(Guid operatorId)
        {
            this.AssignedOperators.Add(operatorId);
        }

        public void AddContract(Guid contractId,
                                String contractName,
                                Guid operatorId)
        {
            this.Contracts.Add(new Contract
                               {
                                   ContractId = contractId,
                                   Description = contractName,
                                   OperatorId = operatorId,
                               });
        }
        
        public void AddMerchant(TransactionProcessor.DataTransferObjects.Responses.Merchant.MerchantResponse merchant)
        {
            // make sure the merchant does not already exist
            if (this.Merchants.Any(m => m.MerchantId == merchant.MerchantId))
            {
                return;
            }

            this.Merchants.Add(merchant);
        }

        public Contract GetContract(String contractName)
        {
            return this.Contracts.Single(c => c.Description == contractName);
        }
        public Contract GetContract(Guid contractId)
        {
            return this.Contracts.Single(c => c.ContractId == contractId);
        }

        public TransactionProcessor.DataTransferObjects.Responses.Merchant.MerchantResponse GetMerchant(String merchantName)
        {
            return this.Merchants.SingleOrDefault(m => m.MerchantName == merchantName);
        }

        public List<TransactionProcessor.DataTransferObjects.Responses.Merchant.MerchantResponse> GetMerchants()
        {
            return this.Merchants;
        }

        public Guid GetOperatorId(String operatorName)
        {
            return this.Operators.Single(o => o.Key == operatorName).Value;
        }

        public void SetEstateUser(String userName,
                                  String password)
        {
            this.EstateUser = userName;
            this.EstatePassword = password;
        }
        
        public void AddMerchantUser(String merchantName,
                                    String userName,
                                    String password)
        {
            if (this.MerchantUsers.ContainsKey(merchantName))
            {
                Dictionary<String, String> merchantUsersList = this.MerchantUsers[merchantName];
                if (merchantUsersList.ContainsKey(userName) == false)
                {
                    merchantUsersList.Add(userName,password);
                }
            }
            else
            {
                Dictionary<String,String> merchantUsersList = new Dictionary<String, String>();
                merchantUsersList.Add(userName,password);
                this.MerchantUsers.Add(merchantName,merchantUsersList);
            }
        }

        public void AddMerchantUserToken(String merchantName,
                                    String userName,
                                    String token)
        {
            if (this.MerchantUsersTokens.ContainsKey(merchantName))
            {
                Dictionary<String, String> merchantUsersList = this.MerchantUsersTokens[merchantName];
                if (merchantUsersList.ContainsKey(userName) == false)
                {
                    merchantUsersList.Add(userName, token);
                }
            }
            else
            {
                Dictionary<String, String> merchantUsersList = new Dictionary<String, String>();
                merchantUsersList.Add(userName, token);
                this.MerchantUsersTokens.Add(merchantName, merchantUsersList);
            }
        }

        public void SetEstateUserToken(String accessToken)
        {
            this.AccessToken = accessToken;
        }
        
        public Guid EstateId { get; private set; }
        public String EstateName { get; private set; }
        public String EstateReference { get; private set; }
        private Dictionary<String, Guid> Operators;

        private List<Guid> AssignedOperators;

        private List<TransactionProcessor.DataTransferObjects.Responses.Merchant.MerchantResponse> Merchants;
        
        private Dictionary<String, Dictionary<String,String>> MerchantUsers;
        private Dictionary<String, Dictionary<String, String>> MerchantUsersTokens;

        private List<Contract> Contracts;
    }

    public class Contract
    {
        public Guid ContractId { get; set; }

        public Guid OperatorId { get; set; }

        public String Description { get; set; }

        public List<Product> Products { get; set; }

        public void AddProduct(Guid productId,
                               String name,
                               String displayText,
                               Decimal? value = null)
        {
            Product product = new Product
                              {
                                  ProductId = productId,
                                  DisplayText = displayText,
                                  Name = name,
                                  Value = value
                              };

            if (this.Products == null)
            {
                this.Products = new List<Product>();
            }
            this.Products.Add(product);
        }

        public Product GetProduct(Guid productId)
        {
            return this.Products.SingleOrDefault(p => p.ProductId == productId);
        }

        public Product GetProduct(String name)
        {
            return this.Products.SingleOrDefault(p => p.Name == name);
        }
    }

    public class Product
    {
        public Guid ProductId { get; set; }

        public String Name { get; set; }
        public String DisplayText { get; set; }

        public Decimal? Value { get; set; }

        public List<TransactionFee> TransactionFees { get; set; }

        public void AddTransactionFee(Guid transactionFeeId,
                                      CalculationType calculationType,
                                      FeeType feeType,
                                      String description,
                                      Decimal value)
        {
            TransactionFee transactionFee = new TransactionFee
                              {
                                  TransactionFeeId = transactionFeeId,
                                  CalculationType = calculationType,
                                  FeeType = feeType,
                                  Description = description,
                                  Value = value
                              };

            if (this.TransactionFees == null)
            {
                this.TransactionFees = new List<TransactionFee>();
            }
            this.TransactionFees.Add(transactionFee);
        }

        public TransactionFee GetTransactionFee(Guid transactionFeeId)
        {
            return this.TransactionFees.SingleOrDefault(t => t.TransactionFeeId== transactionFeeId);
        }

        public TransactionFee GetTransactionFee(String description)
        {
            return this.TransactionFees.SingleOrDefault(t => t.Description == description);
        }
    }

    public class TransactionFee
    {
        public Guid TransactionFeeId { get; set; }

        public CalculationType CalculationType { get; set; }
        public FeeType FeeType { get; set; }

        public String Description { get; set; }

        public Decimal  Value { get; set; }
    }

    public class CreateNewUserRequest
    {
        public String EmailAddress { get; set; }
        
        public String Password { get; set; }

        public String GivenName { get; set; }

        public String MiddleName { get; set; }

        public String FamilyName { get; set; }

        public Int32 UserType {get; set; }
        public Guid? EstateId { get; set; }
        public Guid? MerchantId { get; set; }
        public String MerchantName { get; set; }
    }
}
