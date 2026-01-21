using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Database.Entities.Summary;
using TransactionProcessor.Database.ViewEntities;
using File = TransactionProcessor.Database.Entities.File;

namespace TransactionProcessor.Database.Contexts;

public static class Extensions{
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
                                                   Int32 scale){
        return propertyBuilder.HasColumnType($"decimal({precision},{scale})");
    }

    public static PropertyBuilder IsDateOnly(this PropertyBuilder propertyBuilder){
        return propertyBuilder.HasColumnType("date");
    }

    public static ModelBuilder SetupContract(this ModelBuilder modelBuilder){
        modelBuilder.Entity<Contract>().HasKey(c => new {
                                                            c.EstateId,
                                                            c.OperatorId,
                                                            c.ContractId
                                                        });

        return modelBuilder;
    }

    public static ModelBuilder SetupContractProduct(this ModelBuilder modelBuilder){
        modelBuilder.Entity<ContractProduct>().HasKey(c => new {
                                                                   c.ContractProductReportingId,
                                                               });

        modelBuilder.Entity<ContractProduct>().HasIndex(c => new {
            c.ContractProductId,
            c.ContractId
        }).IsUnique(true);

        return modelBuilder;
    }

    public static ModelBuilder SetupContractProductTransactionFee(this ModelBuilder modelBuilder){
        modelBuilder.Entity<ContractProductTransactionFee>().HasKey(c => new {
                                                                                 c.ContractProductTransactionFeeReportingId,
                                                                             });

        modelBuilder.Entity<ContractProductTransactionFee>().HasIndex(c => new {
            c.ContractProductTransactionFeeId,
            c.ContractProductId
        }).IsUnique(true);

        modelBuilder.Entity<ContractProductTransactionFee>().Property(p => p.Value).DecimalPrecision(18, 4);

        return modelBuilder;
    }

    public static ModelBuilder SetupEstate(this ModelBuilder modelBuilder){
        modelBuilder.Entity<Estate>().HasKey(t => new{
                                                         t.EstateReportingId
                                                     });

        modelBuilder.Entity<Estate>().HasIndex(t => new{
                                                           t.EstateId
                                                       }).IsUnique();

        return modelBuilder;
    }

    public static ModelBuilder SetupEstateOperator(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EstateOperator>().HasKey(t => new {
            t.EstateId,
            t.OperatorId
        });

        return modelBuilder;
    }

    public static ModelBuilder SetupOperator(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Operator>().HasKey(t => new {
                                                          t.OperatorReportingId
                                                      });

        modelBuilder.Entity<Operator>().HasIndex(t => new {
                                                              t.OperatorId
                                                          }).IsUnique();

        return modelBuilder;
    }

    public static ModelBuilder SetupTransactionHistory(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionHistory>().HasNoKey();
        modelBuilder.Entity<TransactionHistory>().HasIndex(s => new
        {
            s.TransactionDate
        }).IsClustered(true);

        modelBuilder.Entity<TransactionHistory>(s => { s.Property(p => p.TransactionDate).IsDateOnly(); });

        modelBuilder.Entity<TransactionHistory>().HasIndex(s => new
        {
            s.TransactionId
        }).IsUnique(true);

        return modelBuilder;
    }

    public static ModelBuilder SetupTodaysTransactions(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodayTransaction>().HasNoKey();
        modelBuilder.Entity<TodayTransaction>().HasIndex(s => new
        {
            s.TransactionDate
        }).IsClustered(true);

        modelBuilder.Entity<TodayTransaction>(s => { s.Property(p => p.TransactionDate).IsDateOnly(); });

        modelBuilder.Entity<TodayTransaction>().HasIndex(s => new
        {
            s.TransactionId
        }).IsUnique(true);

        return modelBuilder;
    }

    public static ModelBuilder SetupSettlementSummary(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SettlementSummary>().HasNoKey();
        modelBuilder.Entity<SettlementSummary>().HasIndex(s => new
        {
            s.SettlementDate
        }).IsClustered(true);

        modelBuilder.Entity<SettlementSummary>(s => { s.Property(p => p.SettlementDate).IsDateOnly(); });

        modelBuilder.Entity<SettlementSummary>().HasIndex(s => new
        {
            s.SettlementDate,
            s.MerchantReportingId,
            s.OperatorReportingId,
            s.ContractProductReportingId,
            s.IsCompleted,
            s.IsSettled
        }).IsUnique(true);

        return modelBuilder;
    }

    public static ModelBuilder SetupEstateSecurityUser(this ModelBuilder modelBuilder){
        modelBuilder.Entity<EstateSecurityUser>().HasKey(t => new{
                                                                     t.SecurityUserId,
                                                                     t.EstateId
                                                                 });
        return modelBuilder;
    }

    public static ModelBuilder SetupMerchant(this ModelBuilder modelBuilder){
        modelBuilder.Entity<Merchant>().HasKey(t => new {
                                                            t.MerchantReportingId
        });

        modelBuilder.Entity<Merchant>().HasIndex(t => new {
                                                              t.EstateId,
                                                              t.MerchantId
                                                          }).IsUnique();

        modelBuilder.Entity<Merchant>(e => { e.Property(p => p.LastSaleDate).IsDateOnly(); });
        
        return modelBuilder;
    }
    
    public static ModelBuilder SetupMerchantAddress(this ModelBuilder modelBuilder){
        modelBuilder.Entity<MerchantAddress>().HasKey(t => new {
                                                                   t.MerchantId,
                                                                   t.AddressId
                                                               });
        return modelBuilder;
    }

    public static ModelBuilder SetupMerchantContact(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MerchantContact>().HasKey(t => new {
                                                                   t.MerchantId,
                                                                   t.ContactId
                                                               });
        return modelBuilder;
    }

    public static ModelBuilder SetupMerchantDevice(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MerchantDevice>().HasKey(t => new {
                                                                  t.MerchantId,
                                                                  t.DeviceId
                                                              });
        return modelBuilder;
    }

    public static ModelBuilder SetupMerchantSecurityUser(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MerchantSecurityUser>().HasKey(t => new {
                                                                        t.MerchantId,
                                                                        t.SecurityUserId
                                                                    });
        return modelBuilder;
    }

    public static ModelBuilder SetupMerchantOperator(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MerchantOperator>().HasKey(t => new {
                                                                    t.MerchantId,
                                                                    t.OperatorId
                                                                });
        return modelBuilder;
    }
    
    public static ModelBuilder SetupResponseCodes(this ModelBuilder modelBuilder){
        modelBuilder.Entity<ResponseCodes>().HasKey(r => new{
                                                                r.ResponseCode
                                                            });
        return modelBuilder;
    }

    public static ModelBuilder SetupSettlement(this ModelBuilder modelBuilder){
        modelBuilder.Entity<Settlement>().HasKey(s => new {
                                                              s.SettlementReportingId
                                                          }).IsClustered(false);

        modelBuilder.Entity<Settlement>().HasIndex(s => new {
                                                                s.EstateId,
                                                                s.SettlementId
                                                            }).IsClustered(false).IsUnique(true);

        modelBuilder.Entity<Settlement>().HasIndex(s => new {
                                                                s.SettlementDate,
                                                            }).IsClustered(true);

        modelBuilder.Entity<Settlement>(e => { e.Property(p => p.SettlementDate).IsDateOnly(); });

        return modelBuilder;
    }

    public static ModelBuilder SetupMerchantSettlementFee(this ModelBuilder modelBuilder){
        

        modelBuilder.Entity<MerchantSettlementFee>().HasKey(s => new{
                                                                        s.SettlementId,
                                                                        s.TransactionId,
                                                                        s.ContractProductTransactionFeeId
                                                                    });

        return modelBuilder;
    }

    public static ModelBuilder SetupTransactionTimings(this ModelBuilder modelBuilder) {
        modelBuilder.Entity<TransactionTimings>().HasKey(t => new {
            t.TransactionId
        }).IsClustered(false);

        return modelBuilder;
    }

    public static ModelBuilder SetupTransaction(this ModelBuilder modelBuilder){
        modelBuilder.Entity<Transaction>().HasKey(t => new {
                                                               t.TransactionReportingId,
                                                           }).IsClustered(false);

        modelBuilder.Entity<Transaction>().HasIndex(t => new {
                                                                 t.TransactionId
                                                             }).IsClustered(false).IsUnique(true);

        modelBuilder.Entity<Transaction>().HasIndex(t => new
        {
            t.TransactionDate
        }).IsClustered(true);

        modelBuilder.Entity<Transaction>(e => { e.Property(p => p.TransactionDate).IsDateOnly(); });

        return modelBuilder;
    }

    public static ModelBuilder SetupTransactionAdditionalRequestData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionAdditionalRequestData>().HasKey(t => new {
                                                                                    t.TransactionId
                                                                                });


        return modelBuilder;
    }

    public static ModelBuilder SetupTransactionAdditionalResponseData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionAdditionalResponseData>().HasKey(t => new {
                                                                                     t.TransactionId
        });


        return modelBuilder;
    }

    public static ModelBuilder SetupVoucher(this ModelBuilder modelBuilder){
        modelBuilder.Entity<VoucherProjectionState>().HasKey(t => new{
                                                          t.VoucherId
                                                      });

        modelBuilder.Entity<VoucherProjectionState>().HasIndex(t => new {
                                                           t.VoucherCode
                                                       });

        modelBuilder.Entity<VoucherProjectionState>().HasIndex(t => new {
                                                             t.TransactionId
        });

        modelBuilder.Entity<VoucherProjectionState>(e => { e.Property(p => p.IssuedDate).IsDateOnly(); });
        modelBuilder.Entity<VoucherProjectionState>(e => { e.Property(p => p.GenerateDate).IsDateOnly(); });
        modelBuilder.Entity<VoucherProjectionState>(e => { e.Property(p => p.ExpiryDate).IsDateOnly(); });

        return modelBuilder;
    }

    public static ModelBuilder SetupReconciliation(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reconciliation>().HasKey(t => new {
                                                               t.TransactionReportingId,
                                                           }).IsClustered(false);

        modelBuilder.Entity<Reconciliation>().HasIndex(t => new {
                                                                    t.TransactionId,
                                                                    t.MerchantId,
                                                                }).IsClustered(false).IsUnique(true);

        modelBuilder.Entity<Reconciliation>().HasIndex(t => new {
                                                                    t.TransactionDate
                                                                }).IsClustered(true);

        modelBuilder.Entity<Reconciliation>(e => { e.Property(p => p.TransactionDate).IsDateOnly(); });

        return modelBuilder;
    }

    public static ModelBuilder SetupStatementHeader(this ModelBuilder modelBuilder){
        modelBuilder.Entity<StatementHeader>().HasKey(s => new {
                                                                   s.MerchantId,
                                                                   s.StatementId
                                                               }).IsClustered(false);

        modelBuilder.Entity<StatementHeader>().HasIndex(s => new {
                                                                     s.StatementGeneratedDate,
                                                                 }).IsClustered();

        modelBuilder.Entity<StatementHeader>(e => { e.Property(p => p.StatementGeneratedDate).IsDateOnly(); });
        modelBuilder.Entity<StatementHeader>(e => { e.Property(p => p.StatementCreatedDate).IsDateOnly(); });

        return modelBuilder;
    }

    public static ModelBuilder SetupStatementLine(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StatementLine>().HasKey(t => new {
                                                                 t.StatementId,
                                                                 t.TransactionId,
                                                                 t.ActivityDateTime,
                                                                 t.ActivityType
                                                             });
        
        modelBuilder.Entity<StatementLine>(e => { e.Property(p => p.ActivityDate).IsDateOnly(); });
        

        return modelBuilder;
    }

    public static ModelBuilder SetupViewEntities(this ModelBuilder modelBuilder){
        modelBuilder.Entity<SettlementView>().HasNoKey().ToView("uvwSettlements");

        return modelBuilder;
    }

    public static ModelBuilder SetupFileImportLog(this ModelBuilder modelBuilder){
        modelBuilder.Entity<FileImportLog>().HasKey(f => new{
                                                                f.EstateId,
                                                                f.FileImportLogReportingId
                                                            });

        modelBuilder.Entity<FileImportLog>().HasIndex(f => new {
                                                                   f.EstateId,
                                                                   f.FileImportLogId
                                                               }).IsUnique();

        modelBuilder.Entity<FileImportLog>(e => { e.Property(p => p.ImportLogDate).IsDateOnly(); });

        return modelBuilder;
    }

    public static ModelBuilder SetupFileImportLogFile(this ModelBuilder modelBuilder){
        modelBuilder.Entity<FileImportLogFile>().HasKey(f => new {
                                                                     f.FileImportLogId,
                                                                     f.FileId,
                                                                 });

        modelBuilder.Entity<FileImportLogFile>(e => { e.Property(p => p.FileUploadedDate).IsDateOnly(); });

        return modelBuilder;
    }

    public static ModelBuilder SetupFile(this ModelBuilder modelBuilder){
        modelBuilder.Entity<File>().HasKey(f => new {
                                                        f.FileReportingId
                                                    });

        modelBuilder.Entity<File>().HasIndex(f => new {
                                                          f.FileId
                                                      }).IsUnique();

        modelBuilder.Entity<File>(e => { e.Property(p => p.FileReceivedDate).IsDateOnly(); });

        return modelBuilder;
    }

    public static ModelBuilder SetupFileLine(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileLine>().HasKey(f => new {
            f.FileId,
            f.LineNumber
        }).IsClustered();

        //modelBuilder.Entity<FileLine>().HasIndex(f => new {
        //    f.TransactionId
        //}).IsUnique(true);
        
        return modelBuilder;
    }

    public static ModelBuilder SetupMerchantContract(this ModelBuilder modelBuilder){
        modelBuilder.Entity<MerchantContract>().HasKey(mc => new{
                                                                    mc.MerchantId,
                                                                    mc.ContractId
                                                                });

        return modelBuilder;
    }

    public static ModelBuilder SetupFloat(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Float>().HasKey(t => new {
                                                           t.FloatId
                                                       }).IsClustered(false);

        modelBuilder.Entity<Float>().HasIndex(t => new {
                                                           t.CreatedDate
                                                       }).IsClustered(true);
        
        modelBuilder.Entity<Float>(e => { e.Property(p => p.CreatedDate).IsDateOnly(); });

        return modelBuilder;
    }

    public static ModelBuilder SetupFloatActivity(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FloatActivity>().HasKey(t => new {
                                                         t.EventId
                                                     }).IsClustered(false);

        modelBuilder.Entity<FloatActivity>().HasIndex(t => new {
                                                                   t.ActivityDate
                                                               }).IsClustered(true);

        modelBuilder.Entity<FloatActivity>(e => { e.Property(p => p.ActivityDate).IsDateOnly(); });

        return modelBuilder;
    }

    #endregion
}