using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class MaintenanceTriggersMeeting1811 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "end_date",
                table: "maintenance_work_order_details",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "frequency",
                table: "maintenance_work_order_details",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "frequency_type_id",
                table: "maintenance_work_order_details",
                type: "bigint",
                nullable: false,
                defaultValue: 3L);

            migrationBuilder.AddColumn<int>(
                name: "lead_days",
                table: "maintenance_work_order_details",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<DateOnly>(
                name: "start_date",
                table: "maintenance_work_order_details",
                type: "date",
                nullable: false,
                defaultValueSql: "CURRENT_DATE");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "archived_at",
                table: "maintenance_triggers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "maintenance_triggers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "maintenance_triggers",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            // Meeting note 18/11: a plan has triggers, each with its own schedule and end day.
            migrationBuilder.Sql("""
                -- Each old plan had one trigger: it takes the plan's schedule and its assets' first and last days.
                UPDATE maintenance_work_order_details AS d
                SET frequency = t.frequency,
                    frequency_type_id = t.frequency_type_id,
                    lead_days = t.lead_days,
                    start_date = COALESCE((SELECT min(a.start_time) FROM maintenance_work_order_assets AS a
                                           WHERE a.work_order_detail_id = d.id), CURRENT_DATE),
                    end_date = (SELECT CASE WHEN count(*) = 0 OR bool_or(a.end_time IS NULL) THEN NULL ELSE max(a.end_time) END
                                FROM maintenance_work_order_assets AS a WHERE a.work_order_detail_id = d.id)
                FROM maintenance_triggers AS t
                WHERE t.id = d.trigger_id;

                -- Old plans were named "checklist · building" on screen: keep that as their name.
                UPDATE maintenance_triggers AS t
                SET name = left(x.checklist || ' · ' || x.building, 120)
                FROM (SELECT DISTINCT ON (d.trigger_id) d.trigger_id, c.name AS checklist, b.name AS building
                      FROM maintenance_work_order_details AS d
                      JOIN case_checklists AS c ON c.id = d.checklist_id
                      JOIN maintenance_work_order_assets AS a ON a.work_order_detail_id = d.id
                      JOIN assets AS s ON s.id = a.asset_id
                      JOIN rooms AS r ON r.id = s.room_id
                      JOIN floors AS f ON f.id = r.floor_id
                      JOIN buildings AS b ON b.id = f.building_id
                      ORDER BY d.trigger_id, d.id) AS x
                WHERE x.trigger_id = t.id AND t.name IS NULL;

                -- "Ended" plans are the archived ones now.
                UPDATE maintenance_triggers SET archived_at = updated_at WHERE status_id = 5 AND archived_at IS NULL;
                """);

            migrationBuilder.UpdateData(
                table: "trigger_statuses",
                keyColumn: "id",
                keyValue: 5L,
                column: "name",
                value: "archived");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_work_order_details_frequency_type_id",
                table: "maintenance_work_order_details",
                column: "frequency_type_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_maintenance_work_order_details_end_date",
                table: "maintenance_work_order_details",
                sql: "end_date IS NULL OR end_date >= start_date");

            migrationBuilder.AddCheckConstraint(
                name: "ck_maintenance_work_order_details_frequency",
                table: "maintenance_work_order_details",
                sql: "frequency > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_maintenance_work_order_details_lead_days",
                table: "maintenance_work_order_details",
                sql: "lead_days >= 0");

            migrationBuilder.AddForeignKey(
                name: "fk_maintenance_work_order_details_frequency_types_frequency_ty",
                table: "maintenance_work_order_details",
                column: "frequency_type_id",
                principalTable: "frequency_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_maintenance_work_order_details_frequency_types_frequency_ty",
                table: "maintenance_work_order_details");

            migrationBuilder.DropIndex(
                name: "ix_maintenance_work_order_details_frequency_type_id",
                table: "maintenance_work_order_details");

            migrationBuilder.DropCheckConstraint(
                name: "ck_maintenance_work_order_details_end_date",
                table: "maintenance_work_order_details");

            migrationBuilder.DropCheckConstraint(
                name: "ck_maintenance_work_order_details_frequency",
                table: "maintenance_work_order_details");

            migrationBuilder.DropCheckConstraint(
                name: "ck_maintenance_work_order_details_lead_days",
                table: "maintenance_work_order_details");

            migrationBuilder.DropColumn(
                name: "end_date",
                table: "maintenance_work_order_details");

            migrationBuilder.DropColumn(
                name: "frequency",
                table: "maintenance_work_order_details");

            migrationBuilder.DropColumn(
                name: "frequency_type_id",
                table: "maintenance_work_order_details");

            migrationBuilder.DropColumn(
                name: "lead_days",
                table: "maintenance_work_order_details");

            migrationBuilder.DropColumn(
                name: "start_date",
                table: "maintenance_work_order_details");

            migrationBuilder.DropColumn(
                name: "archived_at",
                table: "maintenance_triggers");

            migrationBuilder.DropColumn(
                name: "description",
                table: "maintenance_triggers");

            migrationBuilder.DropColumn(
                name: "name",
                table: "maintenance_triggers");

            migrationBuilder.UpdateData(
                table: "trigger_statuses",
                keyColumn: "id",
                keyValue: 5L,
                column: "name",
                value: "ended");
        }
    }
}
