using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiry",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenHash",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiry",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RefreshTokenHash",
                table: "AspNetUsers");
        }
    }
}
