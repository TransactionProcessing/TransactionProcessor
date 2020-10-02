using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.BusinessLogic.Services
{
    using System.IO.Abstractions;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    public interface ITransactionReceiptBuilder
    {
        Task<String> GetEmailReceiptMessage(Transaction transaction, CancellationToken cancellationToken);
    }

    public class TransactionReceiptBuilder : ITransactionReceiptBuilder
    {
        private readonly IFileSystem FileSystem;

        public TransactionReceiptBuilder(IFileSystem fileSystem)
        {
            this.FileSystem = fileSystem;
        }

        public async Task<String> GetEmailReceiptMessage(Transaction transaction,
                                                    CancellationToken cancellationToken)
        {
            var fileData = await this.FileSystem.File.ReadAllTextAsync($"\\Receipts\\Email\\{transaction.OperatorIdentifier}\\TransactionAuthorised.html", cancellationToken);

            // TODO: We will do substitutions here

            return fileData;
        }
    }

    
}
