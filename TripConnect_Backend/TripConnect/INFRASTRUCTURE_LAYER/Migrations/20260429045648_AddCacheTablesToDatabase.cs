using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INFRASTRUCTURE_LAYER.Migrations
{
    /// <inheritdoc />
    public partial class AddCacheTablesToDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CachePolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PolicyType = table.Column<int>(type: "int", nullable: false),
                    AbsoluteExpirationSeconds = table.Column<int>(type: "int", nullable: true),
                    SlidingExpirationSeconds = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachePolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CacheEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CacheKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CachedData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CachedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AccessCount = table.Column<long>(type: "bigint", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CachePolicyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CacheEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CacheEntries_CachePolicies_CachePolicyId",
                        column: x => x.CachePolicyId,
                        principalTable: "CachePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CacheEntries_CacheKey",
                table: "CacheEntries",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CacheEntries_CachePolicyId",
                table: "CacheEntries",
                column: "CachePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_CacheEntries_DataType",
                table: "CacheEntries",
                column: "DataType");

            migrationBuilder.CreateIndex(
                name: "IX_CacheEntries_ExpiresAt",
                table: "CacheEntries",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_CachePolicies_PolicyName",
                table: "CachePolicies",
                column: "PolicyName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CacheEntries");

            migrationBuilder.DropTable(
                name: "CachePolicies");
        }
    }
}
