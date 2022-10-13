namespace TransactionProcessor.IntegrationTests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    public class Contract
    {
        #region Properties

        /// <summary>
        /// Gets or sets the contract identifier.
        /// </summary>
        /// <value>
        /// The contract identifier.
        /// </value>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public String Description { get; set; }

        /// <summary>
        /// Gets or sets the operator identifier.
        /// </summary>
        /// <value>
        /// The operator identifier.
        /// </value>
        public Guid OperatorId { get; set; }

        /// <summary>
        /// Gets or sets the products.
        /// </summary>
        /// <value>
        /// The products.
        /// </value>
        public List<Product> Products { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the product.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="displayText">The display text.</param>
        /// <param name="value">The value.</param>
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

        /// <summary>
        /// Gets the product.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <returns></returns>
        public Product GetProduct(Guid productId)
        {
            if (this.Products.Any() == false) {
                return null;
            }

            return this.Products.SingleOrDefault(p => p.ProductId == productId);
        }

        /// <summary>
        /// Gets the product.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Product GetProduct(String productName)
        {
            if (this.Products.Any() == false)
            {
                return null;
            }

            if (productName == "EmptyProduct")
            {
                return new Product
                {
                           ProductId = Guid.Empty
                };
            }

            if (productName == "InvalidProduct")
            {
                return new Product
                {
                           ProductId = Guid.Parse("934D8164-F36A-448E-B27B-4D671D41D180")
                       };
            }

            return this.Products.SingleOrDefault(p => p.Name == productName);
        }

        #endregion
    }
}