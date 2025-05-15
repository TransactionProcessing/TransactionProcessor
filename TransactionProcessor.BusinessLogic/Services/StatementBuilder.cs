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
using TransactionProcessor.Aggregates;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Models.Merchant;
using File = System.IO.File;
using Merchant = TransactionProcessor.Models.Merchant.Merchant;

namespace TransactionProcessor.BusinessLogic.Services
{
    public interface IStatementBuilder
    {
        Task<String> GetStatementHtml(MerchantStatementAggregate statementAggregate,
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

        
        public async Task<String> GetStatementHtml(MerchantStatementAggregate statementAggregate,
                                                   Merchant merchant,
                                                   CancellationToken cancellationToken)
        {
            // TODO: Check statement aggregate status
            // TODO: Check merchant and addresses exist

            IDirectoryInfo path = this.FileSystem.Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            MerchantStatement statementHeader = statementAggregate.GetStatement();

            String mainHtml = await this.FileSystem.File.ReadAllTextAsync($"{path}/Templates/Email/statement.html", cancellationToken);

            var anonymousHeader = new {
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
                
            };
            var statementLines = statementHeader.GetStatementLines();

            var anonymousFooter = new { StatementTotal = statementLines.Sum(sl => sl.statementLine.Amount), TransactionsValue = statementLines.Where(sl => sl.statementLine.LineType == 1).Sum(sl => sl.statementLine.Amount), TransactionFeesValue = statementLines.Where(sl => sl.statementLine.LineType == 2).Sum(sl => sl.statementLine.Amount) };

            // Statement header class first
            PropertyInfo[] statementHeaderProperties = anonymousHeader.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Do the replaces for the transaction
            foreach (PropertyInfo propertyInfo in statementHeaderProperties)
            {
                mainHtml = mainHtml.Replace($"[{propertyInfo.Name}]", propertyInfo.GetValue(anonymousHeader)?.ToString());
            }

            PropertyInfo[] statementFooterProperties = anonymousFooter.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Do the replaces for the transaction
            foreach (PropertyInfo propertyInfo in statementFooterProperties)
            {
                mainHtml = mainHtml.Replace($"[{propertyInfo.Name}]", propertyInfo.GetValue(anonymousFooter)?.ToString());
            }

            StringBuilder lines = new StringBuilder();
            String lineHtml = await this.FileSystem.File.ReadAllTextAsync($"{path}/Templates/Email/statementline.html", cancellationToken);
            foreach ((Int32 lineNumber, MerchantStatementLine statementLine) statementLineContainer in statementHeader.GetStatementLines())
            {
                var anonymousLine = new { StatementLineNumber = statementLineContainer.lineNumber+1, StatementLineDate = statementLineContainer.statementLine.DateTime.ToString("dd/MM/yyyy"), StatementLineDescription = statementLineContainer.statementLine.Description, StatementLineAmount = statementLineContainer.statementLine.Amount };
                PropertyInfo[] statementLineProperties = anonymousLine.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo propertyInfo in statementLineProperties)
                {
                    lineHtml = lineHtml.Replace($"[{propertyInfo.Name}]", propertyInfo.GetValue(anonymousLine)?.ToString());
                }

                lines.Append(lineHtml);
            }

            mainHtml = mainHtml.Replace("[StatementLinesData]", lines.ToString());

            string basePath = Path.GetFullPath($"{AppContext.BaseDirectory}/Templates/Email/");

            var bootstrapmin = await this.FileSystem.File.ReadAllTextAsync($@"{basePath}bootstrap/css/bootstrap.min.css", cancellationToken);
            var fontawesomemin = await this.FileSystem.File.ReadAllTextAsync($@"{basePath}fontawesome/css/fontawesome.min.css", cancellationToken);
            var fontawesomesolid = await this.FileSystem.File.ReadAllTextAsync($@"{basePath}fontawesome/css/solid.css", cancellationToken);

            mainHtml = mainHtml.Replace("{bootstrapcss}", bootstrapmin);
            mainHtml = mainHtml.Replace("{fontawesomemincss}", fontawesomemin);
            mainHtml = mainHtml.Replace("{fontawesomesolidcss}", fontawesomesolid);

            return mainHtml;
        }

        #endregion
    }

    /*
    public interface IPDFGenerator
    {
        #region Methods

        Task<String> CreatePDF(String htmlString,
                               CancellationToken cancellationToken);

        #endregion
    }

    [ExcludeFromCodeCoverage]
    public class PDFGenerator : IPDFGenerator
    {
        #region Fields

        

        #endregion

        #region Constructors

        public PDFGenerator()
        {
            
        }


        #endregion

        #region Methods

        public async Task<String> CreatePDF(String htmlString,
                                            CancellationToken cancellationToken) {
            //Environment.SetEnvironmentVariable("QT_WEBENGINE_DISABLE_GPU", "1");
            //Environment.SetEnvironmentVariable("QT_OPENGL", "software");

            string basePath = Path.GetFullPath($"{AppContext.BaseDirectory}/Templates/Email/");

            var bootstrapmin = File.ReadAllText($@"{basePath}bootstrap/css/bootstrap.min.css");
            var fontawesomemin = File.ReadAllText($@"{basePath}fontawesome/css/fontawesome.min.css");
            var fontawesomesolid = File.ReadAllText($@"{basePath}fontawesome/css/solid.css");

            htmlString = htmlString.Replace("{bootstrapcss}", bootstrapmin);
            htmlString = htmlString.Replace("{fontawesomemincss}", fontawesomemin);
            htmlString = htmlString.Replace("{fontawesomesolidcss}", fontawesomesolid);

            //string originalTag = "<link rel=\"stylesheet\" href=\"bootstrap/css/bootstrap.min.css\">";
            //string updatedTag = ConvertLinkHrefToFileUri(originalTag, basePath);
            //htmlString = htmlString.Replace(originalTag, updatedTag);

            //originalTag = "<link rel=\"stylesheet\" href=\"fontawesome/css/fontawesome.min.css\">";
            //updatedTag = ConvertLinkHrefToFileUri(originalTag, basePath);
            //htmlString = htmlString.Replace(originalTag, updatedTag);

            //originalTag = "<link href=\"fontawesome/css/solid.css\" rel=\"stylesheet\" />";
            //updatedTag = ConvertLinkHrefToFileUri(originalTag, basePath);
            //htmlString = htmlString.Replace(originalTag, updatedTag);
            //var fontSet = new FontSet();

            //// Example: Add Windows system fonts (adjust for your OS if needed)
            //var windowsFontDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
            //fontSet.AddDirectory(windowsFontDir);
            //converter.ConverterSettings = new BlinkConverterSettings();
            var settings = new BlinkConverterSettings
            {
                EnableJavaScript = false,
                BlinkPath = "C:\\Temp\\SyncfusionBlink\\binaries"
            };
            settings.CommandLineArguments.Add("--enable-logging");
            settings.CommandLineArguments.Add("--headless");
            settings.CommandLineArguments.Add("--disable-gpu");
            //settings.CommandLineArguments.Add("--disable-software-rasterizer");
            settings.CommandLineArguments.Add("--disable-gpu-compositing");
            settings.CommandLineArguments.Add("--disable-accelerated-2d-canvas");
            settings.CommandLineArguments.Add("--no-sandbox");
            //settings.CommandLineArguments.Add("--disable-component-update");
            //settings.CommandLineArguments.Add("--disable-background-networking");
            //settings.CommandLineArguments.Add("--no-first-run");
            settings.CommandLineArguments.Add("--no-default-browser-check");
            settings.CommandLineArguments.Add("--single-process");
            settings.CommandLineArguments.Add("--no-zygote");
            settings.CommandLineArguments.Add("--disable-network-service");

            settings.TempPath = @"C:\Temp\SyncfusionBlink"; // Make sure this path exists and is writable
            var converter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink)
            { 
                ConverterSettings = settings
            };
            //converter.ConverterSettings.EnableDebugging = true;
            //converter.ConverterSettings.LogFilePath = @"C:\Temp\pdfconversion.log";

            //Convert URL to PDF


            //PdfDocument document = htmlConverter.Convert(htmlString, "/");
            //var pdf = await Task.Run(() => converter.Convert(htmlString, ""));
            //var pdf = converter.ConvertPartialHtml("<html><body><h1>Hello World</h1></body></html>", "", "");

            var pdf = await Task.Run(() => converter.Convert("<html><body><h1>Hello World</h1></body></html>", ""));

            //Saving the PDF to the MemoryStream
            MemoryStream stream = new MemoryStream();

            pdf.Save(stream);
            pdf.Close();

            String base64 = Convert.ToBase64String(stream.ToArray());

            return base64;
        }


        internal static string ConvertLinkHrefToFileUri(string linkTag, string basePath)
        {
            if (string.IsNullOrWhiteSpace(linkTag) || string.IsNullOrWhiteSpace(basePath))
                return linkTag;

            var hrefMatch = Regex.Match(linkTag, @"href\s*=\s*[""'](?<href>[^""']+)[""']", RegexOptions.IgnoreCase);
            if (!hrefMatch.Success)
                return linkTag;

            string relativeHref = hrefMatch.Groups["href"].Value;
            string fullPath = Path.Combine(basePath, relativeHref.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!File.Exists(fullPath))
                return linkTag; // Don't update if the file doesn't exist

            string fileUri = new Uri(fullPath).AbsoluteUri;
            string updatedTag = Regex.Replace(linkTag, @"href\s*=\s*[""'][^""']+[""']",
                $"href=\"{fileUri}\"", RegexOptions.IgnoreCase);

            return updatedTag;
        }

        #endregion
    }*/
}
