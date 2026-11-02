using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetRetirementAndAuditTargets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "retired_at",
                table: "assets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.InsertData(
                table: "audit_log_target_operations",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 9L, "delete" },
                    { 10L, "retire" },
                    { 11L, "import" }
                });

            migrationBuilder.InsertData(
                table: "audit_log_target_types",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 3L, "building" },
                    { 4L, "floor" },
                    { 5L, "room" },
                    { 6L, "asset" },
                    { 7L, "case_category" },
                    { 8L, "asset_category" },
                    { 9L, "asset_type" },
                    { 10L, "manufacturer" },
                    { 11L, "specialty" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "audit_log_target_operations",
                keyColumn: "id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "audit_log_target_operations",
                keyColumn: "id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "audit_log_target_operations",
                keyColumn: "id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "audit_log_target_types",
                keyColumn: "id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "audit_log_target_types",
                keyColumn: "id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "audit_log_target_types",
                keyColumn: "id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "audit_log_target_types",
                keyColumn: "id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "audit_log_target_types",
                keyColumn: "id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "audit_log_target_types",
                keyColumn: "id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "audit_log_target_types",
                keyColumn: "id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "audit_log_target_types",
                keyColumn: "id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "audit_log_target_types",
                keyColumn: "id",
                keyValue: 11L);

            migrationBuilder.DropColumn(
                name: "retired_at",
                table: "assets");
        }
    }
}
