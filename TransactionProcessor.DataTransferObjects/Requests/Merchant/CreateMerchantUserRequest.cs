using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    [ExcludeFromCodeCoverage]
    public class CreateMerchantUserRequest
    {
        public String EmailAddress { get; set; }

        public String Password { get; set; }
        
        public String GivenName { get; set; }

        public String MiddleName { get; set; }

        public String FamilyName { get; set; }
    }
}
