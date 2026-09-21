using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Storage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordPolicyToGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxLoginAttempts",
                table: "groups",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinPasswordLength",
                table: "groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PasswordExpiryDays",
                table: "groups",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireDigit",
                table: "groups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireLowercase",
                table: "groups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireSpecialChar",
                table: "groups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireUppercase",
                table: "groups",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxLoginAttempts",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "MinPasswordLength",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "PasswordExpiryDays",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "RequireDigit",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "RequireLowercase",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "RequireSpecialChar",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "RequireUppercase",
                table: "groups");
        }
    }
}
