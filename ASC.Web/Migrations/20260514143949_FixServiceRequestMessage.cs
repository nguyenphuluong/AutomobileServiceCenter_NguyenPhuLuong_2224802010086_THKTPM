using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASC.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixServiceRequestMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ServiceRequestMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ServiceRequestMessages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ServiceRequestMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PartitionKey",
                table: "ServiceRequestMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RowKey",
                table: "ServiceRequestMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ServiceRequestMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ServiceRequestMessages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ServiceRequestMessages");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ServiceRequestMessages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ServiceRequestMessages");

            migrationBuilder.DropColumn(
                name: "PartitionKey",
                table: "ServiceRequestMessages");

            migrationBuilder.DropColumn(
                name: "RowKey",
                table: "ServiceRequestMessages");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ServiceRequestMessages");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ServiceRequestMessages");
        }
    }
}
