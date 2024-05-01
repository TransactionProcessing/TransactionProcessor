﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TransactionProcessor.ProjectionEngine.Database.Database;

#nullable disable

namespace TransactionProcessor.ProjectionEngine.Migrations
{
    [DbContext(typeof(TransactionProcessorSqlServerContext))]
    [Migration("20240314122120_storevoucherstate")]
    partial class storevoucherstate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("TransactionProcessor.ProjectionEngine.Database.Database.Entities.Event", b =>
                {
                    b.Property<Guid>("EventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("date");

                    b.HasKey("EventId", "Type");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("EventId", "Type"));

                    b.ToTable("Events");
                });

            modelBuilder.Entity("TransactionProcessor.ProjectionEngine.Database.Database.Entities.MerchantBalanceChangedEntry", b =>
                {
                    b.Property<Guid>("AggregateId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("OriginalEventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CauseOfChangeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("ChangeAmount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("DebitOrCredit")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("EstateId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("MerchantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Reference")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AggregateId", "OriginalEventId");

                    b.ToTable("MerchantBalanceChangedEntry");
                });

            modelBuilder.Entity("TransactionProcessor.ProjectionEngine.Database.Database.Entities.MerchantBalanceProjectionState", b =>
                {
                    b.Property<Guid>("EstateId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("MerchantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("AuthorisedSales")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("AvailableBalance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("CompletedTransactionCount")
                        .HasColumnType("int");

                    b.Property<decimal>("DeclinedSales")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("DepositCount")
                        .HasColumnType("int");

                    b.Property<int>("FeeCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("LastDeposit")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastFee")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastSale")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastWithdrawal")
                        .HasColumnType("datetime2");

                    b.Property<string>("MerchantName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SaleCount")
                        .HasColumnType("int");

                    b.Property<int>("StartedTransactionCount")
                        .HasColumnType("int");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<decimal>("TotalDeposited")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalWithdrawn")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ValueOfFees")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("WithdrawalCount")
                        .HasColumnType("int");

                    b.HasKey("EstateId", "MerchantId");

                    b.ToTable("MerchantBalanceProjectionState");
                });

            modelBuilder.Entity("TransactionProcessor.ProjectionEngine.Database.Database.Entities.VoucherProjectionState", b =>
                {
                    b.Property<Guid>("VoucherId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Barcode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("EstateId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<Guid>("TransactionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("VoucherCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("VoucherId");

                    b.ToTable("VoucherProjectionState");
                });

            modelBuilder.Entity("TransactionProcessor.ProjectionEngine.Database.Database.ViewEntities.MerchantBalanceHistoryViewEntry", b =>
                {
                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ChangeAmount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("DebitOrCredit")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("EntryDateTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("MerchantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("OriginalEventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Reference")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.ToTable((string)null);

                    b.ToView("uvwMerchantBalanceHistory", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}