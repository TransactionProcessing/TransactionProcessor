namespace TransactionProcessor.ProjectionEngine.Common
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public static class Extensions
    {
        #region Methods

        /// <summary>
        /// Decimals the precision.
        /// </summary>
        /// <param name="propertyBuilder">The property builder.</param>
        /// <param name="precision">The precision.</param>
        /// <param name="scale">The scale.</param>
        /// <returns></returns>
        public static PropertyBuilder DecimalPrecision(this PropertyBuilder propertyBuilder,
                                                       Int32 precision,
        Int32 scale)
        {
            return propertyBuilder.HasColumnType($"decimal({precision},{scale})");
        }

        

        #endregion
    }
}