using Microsoft.AspNetCore.Html;
using Shared.General;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Models.Merchant;
using File = System.IO.File;
using Merchant = TransactionProcessor.Models.Merchant.Merchant;

namespace TransactionProcessor.BusinessLogic.Services
{
    public interface IStatementBuilder
    {
        Task<Result<String>> GetStatementHtml(MerchantStatementAggregate statementAggregate,
                                      Merchant merchant,
                                      CancellationToken cancellationToken);
    }

    public class StatementBuilder : IStatementBuilder
    {
        #region Fields

        /// <summary>
        /// The file system
        /// </summary>
        private readonly IFileSystem FileSystem;

        #endregion

        #region Constructors

        public StatementBuilder(IFileSystem fileSystem)
        {
            this.FileSystem = fileSystem;
        }

        #endregion

        #region Methods

        
        public async Task<Result<String>> GetStatementHtml(MerchantStatementAggregate statementAggregate,
                                                           Merchant merchant,
                                                           CancellationToken cancellationToken)
        {
            IDirectoryInfo path = this.FileSystem.Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            MerchantStatement statementHeader = statementAggregate.GetStatement();

            // This is only allowed if the statement has been generated but not yet built
            if (statementHeader.IsBuilt)
                return Result.Invalid("Statement has already been built");

            if (statementHeader.IsGenerated == false)
                return Result.Invalid("Statement has not yet been generated");

            String mainHtml = await this.FileSystem.File.ReadAllTextAsync($"{path}/Templates/Email/statement.html", cancellationToken);
            List<(Int32 lineNumber, MerchantStatementLine statementLine)> statementLines = statementHeader.GetStatementLines();
            mainHtml = this.ReplaceTokens(mainHtml, new {
                StatementId = statementAggregate.AggregateId,
                EstateName = "Demo Estate",
                MerchantName = merchant.MerchantName,
                MerchantAddressLine1 = merchant.Addresses.First().AddressLine1,
                MerchantTown = merchant.Addresses.First().Town,
                MerchantRegion = merchant.Addresses.First().Region,
                MerchantCountry = merchant.Addresses.First().Country,
                MerchantPostcode = merchant.Addresses.First().PostalCode,
                MerchantContactNumber = merchant.Contacts.First().ContactPhoneNumber,
                StatementDate = statementHeader.StatementDate,
            });
            mainHtml = this.ReplaceTokens(mainHtml, new {
                StatementTotal = statementLines.Sum(sl => sl.statementLine.Amount),
                TransactionsValue = statementLines.Where(sl => sl.statementLine.LineType == 1).Sum(sl => sl.statementLine.Amount),
                TransactionFeesValue = statementLines.Where(sl => sl.statementLine.LineType == 2).Sum(sl => sl.statementLine.Amount)
            });
            mainHtml = mainHtml.Replace("[StatementLinesData]", await this.BuildStatementLinesHtml(path, statementLines, cancellationToken));

            mainHtml = await this.AddCSSToHtml(mainHtml, "{bootstrapcss}", "bootstrap/css/bootstrap.min.css", cancellationToken);
            mainHtml = await this.AddCSSToHtml(mainHtml, "{fontawesomemincss}", "fontawesome/css/fontawesome.min.css", cancellationToken);
            mainHtml = await this.AddCSSToHtml(mainHtml, "{fontawesomesolidcss}", "fontawesome/css/solid.css", cancellationToken);

            return Result.Success<String>(mainHtml);
        }

        private async Task<String> BuildStatementLinesHtml(IDirectoryInfo path,
                                                           List<(Int32 lineNumber, MerchantStatementLine statementLine)> statementLines,
                                                           CancellationToken cancellationToken)
        {
            StringBuilder lines = new StringBuilder();
            String lineTemplate = await this.FileSystem.File.ReadAllTextAsync($"{path}/Templates/Email/statementline.html", cancellationToken);

            foreach ((Int32 lineNumber, MerchantStatementLine statementLine) statementLineContainer in statementLines) {
                String populatedLine = this.ReplaceTokens(lineTemplate, new {
                    StatementLineNumber = statementLineContainer.lineNumber + 1,
                    StatementLineDate = statementLineContainer.statementLine.DateTime.ToString("dd/MM/yyyy"),
                    StatementLineDescription = statementLineContainer.statementLine.Description,
                    StatementLineAmount = statementLineContainer.statementLine.Amount
                });
                lines.Append(populatedLine);
            }

            return lines.ToString();
        }

        private String ReplaceTokens(String html,
                                     Object tokenSource)
        {
            String result = html;
            PropertyInfo[] properties = tokenSource.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo propertyInfo in properties) {
                result = result.Replace($"[{propertyInfo.Name}]", propertyInfo.GetValue(tokenSource)?.ToString());
            }

            return result;
        }

        private async Task<String> AddCSSToHtml(String html, String tag, String fileName, CancellationToken cancellationToken) {
            String basePath = Path.GetFullPath($"{AppContext.BaseDirectory}/Templates/Email/");

            String css = await this.FileSystem.File.ReadAllTextAsync($@"{basePath}{fileName}", cancellationToken);

            return html.Replace(tag, css);
        }

        #endregion
    }
}
