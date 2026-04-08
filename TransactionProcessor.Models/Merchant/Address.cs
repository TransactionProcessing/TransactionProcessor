using System;

namespace TransactionProcessor.Models.Merchant
{
    public record AddressLines(String Line1, String? Line2, String? Line3, String? Line4);

    public record Address
    {
        public Guid AddressId { get; init; }
        public String AddressLine1 { get; init; }
        public String? AddressLine2 { get; init; }
        public String? AddressLine3 { get; init; }
        public String? AddressLine4 { get; init; }
        public String Town { get; init; }
        public String? Region { get; init; }
        public String PostalCode { get; init; }
        public String Country { get; init; }

        private Address() { }

        public static Address Create(Guid addressId,
                                     AddressLines lines,
                                     String town,
                                     String? region,
                                     String postalCode,
                                     String country)
        {
            if (lines == null)
                throw new ArgumentNullException(nameof(lines));

            if (String.IsNullOrWhiteSpace(lines.Line1))
                throw new ArgumentException("AddressLine1 is required");

            if (String.IsNullOrWhiteSpace(town))
                throw new ArgumentException("Town is required");

            if (String.IsNullOrWhiteSpace(postalCode))
                throw new ArgumentException("PostalCode is required");

            if (String.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country is required");

            return new Address
            {
                AddressId = addressId,
                AddressLine1 = lines.Line1,
                AddressLine2 = lines.Line2,
                AddressLine3 = lines.Line3,
                AddressLine4 = lines.Line4,
                Town = town,
                Region = region,
                PostalCode = postalCode,
                Country = country
            };
        }
    }
}