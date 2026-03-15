using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionProcessor.Database.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class merchant_opening_hours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "merchantopeninghours",
                columns: table => new
                {
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MondayOpening = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MondayClosing = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TuesdayOpening = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TuesdayClosing = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WednesdayOpening = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WednesdayClosing = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThursdayOpening = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThursdayClosing = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FridayOpening = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FridayClosing = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaturdayOpening = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaturdayClosing = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SundayOpening = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SundayClosing = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchantopeninghours", x => x.MerchantId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "merchantopeninghours");
        }
    }
}
