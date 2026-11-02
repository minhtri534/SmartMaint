using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "accept_reminded_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "acknowledged_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "backup_alerted_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "emergency_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "incident_damage",
                table: "cases",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "incident_follow_up",
                table: "cases",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "incident_lessons",
                table: "cases",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "incident_reported_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "made_safe_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "manager_alerted_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "merged_into_case_id",
                table: "cases",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_cases_merged_into_case_id",
                table: "cases",
                column: "merged_into_case_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_cases_incident_follow_up",
                table: "cases",
                sql: "incident_follow_up IN ('none', 'preventive_plan', 'replacement_review')");

            migrationBuilder.AddForeignKey(
                name: "fk_cases_cases_merged_into_case_id",
                table: "cases",
                column: "merged_into_case_id",
                principalTable: "cases",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // Emergency cases made before F4 existed: their clock starts at submission and they count as acknowledged,
            // so the watch job does not chase old data. Pending ones stay for the staff to approve as in F3.
            migrationBuilder.Sql("""
                UPDATE cases SET emergency_at = submitted_at, acknowledged_at = submitted_at
                WHERE emergency_at IS NULL
                  AND priority_level_id = (SELECT id FROM priority_levels WHERE code = 'emergency')
                  AND status_id <> (SELECT id FROM case_statuses WHERE name = 'pending');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cases_cases_merged_into_case_id",
                table: "cases");

            migrationBuilder.DropIndex(
                name: "ix_cases_merged_into_case_id",
                table: "cases");

            migrationBuilder.DropCheckConstraint(
                name: "ck_cases_incident_follow_up",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "accept_reminded_at",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "acknowledged_at",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "backup_alerted_at",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "emergency_at",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "incident_damage",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "incident_follow_up",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "incident_lessons",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "incident_reported_at",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "made_safe_at",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "manager_alerted_at",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "merged_into_case_id",
                table: "cases");
        }
    }
}
