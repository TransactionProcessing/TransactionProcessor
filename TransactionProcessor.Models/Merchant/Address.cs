using System;

namespace TransactionProcessor.Models.Merchant
{
    public record Address
    {
        public Guid AddressId { get; }
        public String AddressLine1 { get; }
        public String? AddressLine2 { get; }
        public String? AddressLine3 { get; }
        public String? AddressLine4 { get; }
        public String Town { get; }
        public String? Region { get; }
        public String PostalCode { get; }
        public String Country { get; }

        private Address(Guid addressId,
                        String addressLine1,
                        String? addressLine2,
                        String? addressLine3,
                        String? addressLine4,
                        String town,
                        String? region,
                        String postalCode,
                        String country)
        {
            AddressId = addressId;
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
            AddressLine3 = addressLine3;
            AddressLine4 = addressLine4;
            Town = town;
            Region = region;
            PostalCode = postalCode;
            Country = country;
        }

        public static Address Create(Guid addressId, 
                                     String addressLine1,
                                     String? addressLine2,
                                     String? addressLine3,
                                     String? addressLine4,
                                     String town,
                                     String? region,
                                     String postalCode,
                                     String country)
        {
            if (String.IsNullOrWhiteSpace(addressLine1))
                throw new ArgumentException("AddressLine1 is required");

            if (String.IsNullOrWhiteSpace(town))
                throw new ArgumentException("Town is required");

            if (String.IsNullOrWhiteSpace(postalCode))
                throw new ArgumentException("PostalCode is required");

            if (String.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country is required");

            return new Address(addressId, addressLine1, addressLine2, addressLine3, addressLine4,town, region, postalCode, country);
        }
    }
}