using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionProcessor.Database.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class add_file_import_config_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileFormatHandlers",
                columns: table => new
                {
                    FileFormatHandlerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileFormatHandlers", x => x.FileFormatHandlerId);
                });

            migrationBuilder.CreateTable(
                name: "FileProfileConfigurations",
                columns: table => new
                {
                    FileProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ListeningDirectory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineTerminator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileFormatHandlerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileProfileConfigurations", x => x.FileProfileId);
                });

            migrationBuilder.CreateTable(
                name: "RequestTypes",
                columns: table => new
                {
                    RequestTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestTypes", x => x.RequestTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileFormatHandlers_Name",
                table: "FileFormatHandlers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileProfileConfigurations_Name",
                table: "FileProfileConfigurations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestTypes_Name",
                table: "RequestTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileFormatHandlers");

            migrationBuilder.DropTable(
                name: "FileProfileConfigurations");

            migrationBuilder.DropTable(
                name: "RequestTypes");
        }
    }
}
