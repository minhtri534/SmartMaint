using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class StaffWorkOrderMeeting0711 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_cases_cost_bearer",
                table: "cases");

            migrationBuilder.DropCheckConstraint(
                name: "ck_case_categories_default_cost_bearer",
                table: "case_categories");

            // Meeting 07/11: "Assigned" (3) is gone; a case with a technician who has not accepted yet is Approved (2).
            migrationBuilder.Sql("UPDATE cases SET status_id = 2 WHERE status_id = 3;");
            migrationBuilder.Sql("UPDATE case_status_history SET new_status_id = 2 WHERE new_status_id = 3;");

            migrationBuilder.DeleteData(
                table: "case_statuses",
                keyColumn: "id",
                keyValue: 3L);

            migrationBuilder.DropColumn(
                name: "cost_bearer",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "default_cost_bearer",
                table: "case_categories");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "planned_end_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "planned_start_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "previous_case_id",
                table: "cases",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "done_at",
                table: "case_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "done_by",
                table: "case_tasks",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "case_followers",
                columns: table => new
                {
                    case_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_followers", x => new { x.case_id, x.customer_id });
                    table.ForeignKey(
                        name: "fk_case_followers_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_case_followers_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cases_previous_case_id",
                table: "cases",
                column: "previous_case_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_cases_planned_window",
                table: "cases",
                sql: "planned_start_at IS NULL OR planned_end_at IS NULL OR planned_end_at > planned_start_at");

            migrationBuilder.AddCheckConstraint(
                name: "ck_case_tasks_done_by",
                table: "case_tasks",
                sql: "done_by IN ('technician', 'third_party')");

            migrationBuilder.CreateIndex(
                name: "ix_case_followers_customer_id",
                table: "case_followers",
                column: "customer_id");

            migrationBuilder.AddForeignKey(
                name: "fk_cases_cases_previous_case_id",
                table: "cases",
                column: "previous_case_id",
                principalTable: "cases",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // Meeting 07/11: every case has tasks. An approved incident without any gets one named after the case;
            // a finished one counts it as done by its technician.
            migrationBuilder.Sql("""
                INSERT INTO case_tasks (case_id, name, status_id, done_by, done_at)
                SELECT c.id, left(c.title, 200),
                       CASE WHEN c.status_id IN (7, 8) THEN 2 ELSE 1 END,
                       CASE WHEN c.status_id IN (7, 8) AND c.assigned_technician_id IS NOT NULL THEN 'technician' END,
                       CASE WHEN c.status_id IN (7, 8) THEN c.completed_at END
                FROM cases c
                WHERE c.status_id NOT IN (1, 10, 11)
                  AND NOT EXISTS (SELECT 1 FROM case_tasks t WHERE t.case_id = c.id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cases_cases_previous_case_id",
                table: "cases");

            migrationBuilder.DropTable(
                name: "case_followers");

            migrationBuilder.DropIndex(
                name: "ix_cases_previous_case_id",
                table: "cases");

            migrationBuilder.DropCheckConstraint(
                name: "ck_cases_planned_window",
                table: "cases");

            migrationBuilder.DropCheckConstraint(
                name: "ck_case_tasks_done_by",
                table: "case_tasks");

            migrationBuilder.DropColumn(
                name: "planned_end_at",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "planned_start_at",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "previous_case_id",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "done_at",
                table: "case_tasks");

            migrationBuilder.DropColumn(
                name: "done_by",
                table: "case_tasks");

            migrationBuilder.AddColumn<string>(
                name: "cost_bearer",
                table: "cases",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "default_cost_bearer",
                table: "case_categories",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "case_statuses",
                columns: new[] { "id", "name" },
                values: new object[] { 3L, "assigned" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_cases_cost_bearer",
                table: "cases",
                sql: "cost_bearer IN ('building', 'customer')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_case_categories_default_cost_bearer",
                table: "case_categories",
                sql: "default_cost_bearer IN ('building', 'customer')");
        }
    }
}
