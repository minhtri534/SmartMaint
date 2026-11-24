using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CaseReassignClosureMeeting1811 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "reassigned_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            // Cases already closed by a reassign (another case continues them) are marked too, so they are not reopened.
            migrationBuilder.Sql("""
                UPDATE cases AS c
                SET reassigned_at = c.closed_at
                FROM cases AS n
                WHERE n.previous_case_id = c.id AND c.status_id = 8 AND c.reassigned_at IS NULL;
                """);

            migrationBuilder.InsertData(
                table: "task_statuses",
                columns: new[] { "id", "name" },
                values: new object[] { 5L, "moved" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "task_statuses",
                keyColumn: "id",
                keyValue: 5L);

            migrationBuilder.DropColumn(
                name: "reassigned_at",
                table: "cases");
        }
    }
}
