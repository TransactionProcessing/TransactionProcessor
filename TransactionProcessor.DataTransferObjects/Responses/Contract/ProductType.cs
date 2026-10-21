namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    /// <summary>Product categories that can be assigned to a contract.</summary>
    /// <example>0</example>
    public enum ProductType
    {
        /// <summary>No product category has been selected.</summary>
        NotSet = 0,

        /// <summary>Mobile top-up product.</summary>
        MobileTopup,

        /// <summary>Voucher product.</summary>
        Voucher,

        /// <summary>Bill payment product.</summary>
        BillPayment,

        /// <summary>Agency banking product.</summary>
        AgencyBanking
    }
}
