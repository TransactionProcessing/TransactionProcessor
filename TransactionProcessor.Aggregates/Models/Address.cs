namespace TransactionProcessor.Aggregates.Models
{
    internal record Address(String AddressLine1, String AddressLine2, String AddressLine3, String AddressLine4, String Town, String Region, String PostalCode, String Country);
}