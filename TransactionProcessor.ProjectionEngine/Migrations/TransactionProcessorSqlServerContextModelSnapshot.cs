﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TransactionProcessor.ProjectionEngine.Database;

#nullable disable

namespace TransactionProcessor.ProjectionEngine.Migrations
{
    [DbContext(typeof(TransactionProcessorSqlServerContext))]
    partial class TransactionProcessorSqlServerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("TransactionProcessor.ProjectionEngine.Database.Entities.MerchantBalanceChangedEntry", b =>
                {
                    b.Property<Guid>("AggregateId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("OriginalEventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18,2)");

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

            modelBuilder.Entity("TransactionProcessor.ProjectionEngine.Database.Entities.MerchantBalanceProjectionState", b =>
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

                    b.Property<decimal>("ValueOfFees")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("EstateId", "MerchantId");

                    b.ToTable("MerchantBalanceProjectionState");
                });
#pragma warning restore 612, 618
        }
    }
}
