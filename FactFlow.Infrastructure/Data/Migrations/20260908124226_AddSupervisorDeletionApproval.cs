using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactFlow.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSupervisorDeletionApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Facts",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "FactDeletionRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FactId = table.Column<int>(type: "int", nullable: false),
                    FactContentSnapshot = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TargetFactRowVersion = table.Column<byte[]>(type: "varbinary(8)", maxLength: 8, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequestedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DecidedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DecisionNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactDeletionRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactDeletionRequests_Facts_FactId",
                        column: x => x.FactId,
                        principalTable: "Facts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FactDeletionRequests_FactId_Status",
                table: "FactDeletionRequests",
                columns: new[] { "FactId", "Status" },
                unique: true,
                filter: "[Status] = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_FactDeletionRequests_RequestedBy_RequestedAtUtc",
                table: "FactDeletionRequests",
                columns: new[] { "RequestedBy", "RequestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FactDeletionRequests_Status_RequestedAtUtc",
                table: "FactDeletionRequests",
                columns: new[] { "Status", "RequestedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FactDeletionRequests");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Facts");
        }
    }
}
