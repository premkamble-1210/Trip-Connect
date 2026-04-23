using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INFRASTRUCTURE_LAYER.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneOtpFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneOtpCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PhoneOtpExpiry",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhoneOtpRequestCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneOtpCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneOtpExpiry",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneOtpRequestCount",
                table: "Users");
        }
    }
}
