using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Tenant_AddDbDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "db_name",
                table: "tenants",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "db_password",
                table: "tenants",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "db_port",
                table: "tenants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "db_server",
                table: "tenants",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "db_user",
                table: "tenants",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "db_name",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "db_password",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "db_port",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "db_server",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "db_user",
                table: "tenants");
        }
    }
}
