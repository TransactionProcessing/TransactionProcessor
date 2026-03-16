using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TransactionProcessor.Database.Contexts;

#nullable disable

namespace TransactionProcessor.Database.Migrations.SqlServer;

public partial class merchant_schedule_read_model : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "merchantschedule",
            columns: table => new
            {
                MerchantScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Year = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_merchantschedule", x => x.MerchantScheduleId);
            });

        migrationBuilder.CreateTable(
            name: "merchantschedulemonth",
            columns: table => new
            {
                MerchantScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Month = table.Column<int>(type: "int", nullable: false),
                ClosedDays = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_merchantschedulemonth", x => new { x.MerchantScheduleId, x.Month });
            });

        migrationBuilder.CreateIndex(
            name: "IX_merchantschedule_MerchantId_Year",
            table: "merchantschedule",
            columns: new[] { "MerchantId", "Year" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "merchantschedulemonth");

        migrationBuilder.DropTable(
            name: "merchantschedule");
    }
}
