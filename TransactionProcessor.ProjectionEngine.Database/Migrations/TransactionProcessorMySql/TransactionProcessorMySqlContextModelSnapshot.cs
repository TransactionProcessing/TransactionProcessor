﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TransactionProcessor.ProjectionEngine.Database;

#nullable disable

namespace TransactionProcessor.ProjectionEngine.Migrations.TransactionProcessorMySql
{
    using Database.Database;

    [DbContext(typeof(TransactionProcessorMySqlContext))]
    partial class TransactionProcessorMySqlContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("TransactionProcessor.ProjectionEngine.Database.Entities.Event", b =>
                {
                    b.Property<Guid>("EventId")
                        .HasColumnType("char(36)");

                    b.Property<string>("Type")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("date");

                    b.HasKey("EventId", "Type")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("Events");
                });

            modelBuilder.Entity("TransactionProcessor.ProjectionEngine.Database.Entities.MerchantBalanceChangedEntry", b =>
                {
                    b.Property<Guid>("AggregateId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("OriginalEventId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("CauseOfChangeId")
                        .HasColumnType("char(36)");

                    b.Property<decimal>("ChangeAmount")
                        .HasColumnType("decimal(65,30)");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("DebitOrCredit")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<Guid>("EstateId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("MerchantId")
                        .HasColumnType("char(36)");

                    b.Property<string>("Reference")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("AggregateId", "OriginalEventId");

                    b.ToTable("MerchantBalanceChangedEntry");
                });

            modelBuilder.Entity("TransactionProcessor.ProjectionEngine.Database.Entities.MerchantBalanceProjectionState", b =>
                {
                    b.Property<Guid>("EstateId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("MerchantId")
                        .HasColumnType("char(36)");

                    b.Property<decimal>("AuthorisedSales")
                        .HasColumnType("decimal(65,30)");

                    b.Property<decimal>("AvailableBalance")
                        .HasColumnType("decimal(65,30)");

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(65,30)");

                    b.Property<int>("CompletedTransactionCount")
                        .HasColumnType("int");

                    b.Property<decimal>("DeclinedSales")
                        .HasColumnType("decimal(65,30)");

                    b.Property<int>("DepositCount")
                        .HasColumnType("int");

                    b.Property<int>("FeeCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("LastDeposit")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastFee")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastSale")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastWithdrawal")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("MerchantName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("SaleCount")
                        .HasColumnType("int");

                    b.Property<int>("StartedTransactionCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp(6)");

                    b.Property<decimal>("TotalDeposited")
                        .HasColumnType("decimal(65,30)");

                    b.Property<decimal>("TotalWithdrawn")
                        .HasColumnType("decimal(65,30)");

                    b.Property<decimal>("ValueOfFees")
                        .HasColumnType("decimal(65,30)");

                    b.Property<int>("WithdrawalCount")
                        .HasColumnType("int");

                    b.HasKey("EstateId", "MerchantId");

                    b.ToTable("MerchantBalanceProjectionState");
                });

            modelBuilder.Entity("TransactionProcessor.ProjectionEngine.Database.ViewEntities.MerchantBalanceHistoryViewEntry", b =>
                {
                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(65,30)");

                    b.Property<decimal>("ChangeAmount")
                        .HasColumnType("decimal(65,30)");

                    b.Property<string>("DebitOrCredit")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("EntryDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("MerchantId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("OriginalEventId")
                        .HasColumnType("char(36)");

                    b.Property<string>("Reference")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.ToTable((string)null);

                    b.ToView("uvwMerchantBalanceHistory", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}