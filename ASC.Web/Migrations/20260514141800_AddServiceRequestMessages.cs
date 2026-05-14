using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASC.Web.Migrations
{
    public partial class AddServiceRequestMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceRequestMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    ServiceRequestId = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    UserName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    Message = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ServiceRequestMessages",
                        x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRequestMessages");
        }
    }
}