namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class TransactionValidationException : Exception
    {
        public TransactionResponseCode ResponseCode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionValidationException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="responseCode">The response code.</param>
        public TransactionValidationException(String message, TransactionResponseCode responseCode) : this(message, responseCode, null)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionValidationException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public TransactionValidationException(String message, TransactionResponseCode responseCode, Exception innerException) : base(message, innerException)
        {
            this.ResponseCode = responseCode;
        }
    }
}