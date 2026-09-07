using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactFlow.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFactSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAtUtc",
                table: "Facts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Facts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Facts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Facts");
        }
    }
}
