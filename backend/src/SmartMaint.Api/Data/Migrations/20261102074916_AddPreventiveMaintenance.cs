using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPreventiveMaintenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "trigger_work_order_id",
                table: "cases",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "case_checklists",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_checklists", x => x.id);
                    table.ForeignKey(
                        name: "fk_case_checklists_case_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "case_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "frequency_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_frequency_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "task_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trigger_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trigger_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "checklist_tasks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    checklist_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checklist_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_checklist_tasks_case_checklists_checklist_id",
                        column: x => x.checklist_id,
                        principalTable: "case_checklists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "case_tasks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    case_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status_id = table.Column<long>(type: "bigint", nullable: false),
                    repair_case_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_case_tasks_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_case_tasks_cases_repair_case_id",
                        column: x => x.repair_case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_case_tasks_task_statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "task_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_triggers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    frequency = table.Column<int>(type: "integer", nullable: false),
                    frequency_type_id = table.Column<long>(type: "bigint", nullable: false),
                    staff_id = table.Column<long>(type: "bigint", nullable: false),
                    approving_manager_id = table.Column<long>(type: "bigint", nullable: true),
                    status_id = table.Column<long>(type: "bigint", nullable: false),
                    last_paused = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_triggered = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lead_days = table.Column<int>(type: "integer", nullable: false),
                    reject_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_triggers", x => x.id);
                    table.CheckConstraint("ck_maintenance_triggers_frequency", "frequency > 0");
                    table.CheckConstraint("ck_maintenance_triggers_lead_days", "lead_days >= 0");
                    table.ForeignKey(
                        name: "fk_maintenance_triggers_frequency_types_frequency_type_id",
                        column: x => x.frequency_type_id,
                        principalTable: "frequency_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_maintenance_triggers_managers_approving_manager_id",
                        column: x => x.approving_manager_id,
                        principalTable: "managers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_maintenance_triggers_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_maintenance_triggers_trigger_statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "trigger_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "task_images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    task_id = table.Column<long>(type: "bigint", nullable: false),
                    image = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_task_images_case_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "case_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_notes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    task_id = table.Column<long>(type: "bigint", nullable: false),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_notes", x => x.id);
                    table.ForeignKey(
                        name: "fk_task_notes_case_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "case_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_work_order_details",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    trigger_id = table.Column<long>(type: "bigint", nullable: false),
                    checklist_id = table.Column<long>(type: "bigint", nullable: false),
                    estimated_hours = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    priority_level_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_work_order_details", x => x.id);
                    table.CheckConstraint("ck_maintenance_work_order_details_estimated_hours", "estimated_hours IS NULL OR estimated_hours > 0");
                    table.ForeignKey(
                        name: "fk_maintenance_work_order_details_case_checklists_checklist_id",
                        column: x => x.checklist_id,
                        principalTable: "case_checklists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_maintenance_work_order_details_maintenance_triggers_trigger",
                        column: x => x.trigger_id,
                        principalTable: "maintenance_triggers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_maintenance_work_order_details_priority_levels_priority_lev",
                        column: x => x.priority_level_id,
                        principalTable: "priority_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_work_order_assets",
                columns: table => new
                {
                    work_order_detail_id = table.Column<long>(type: "bigint", nullable: false),
                    asset_id = table.Column<long>(type: "bigint", nullable: false),
                    technician_id = table.Column<long>(type: "bigint", nullable: true),
                    start_time = table.Column<DateOnly>(type: "date", nullable: false),
                    end_time = table.Column<DateOnly>(type: "date", nullable: true),
                    last_due_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_work_order_assets", x => new { x.work_order_detail_id, x.asset_id });
                    table.ForeignKey(
                        name: "fk_maintenance_work_order_assets_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_maintenance_work_order_assets_maintenance_work_order_detail",
                        column: x => x.work_order_detail_id,
                        principalTable: "maintenance_work_order_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_maintenance_work_order_assets_technicians_technician_id",
                        column: x => x.technician_id,
                        principalTable: "technicians",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "frequency_types",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1L, "day" },
                    { 2L, "week" },
                    { 3L, "month" },
                    { 4L, "year" }
                });

            migrationBuilder.InsertData(
                table: "task_statuses",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1L, "pending" },
                    { 2L, "pass" },
                    { 3L, "fail" },
                    { 4L, "not_applicable" }
                });

            migrationBuilder.InsertData(
                table: "trigger_statuses",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1L, "pending_approval" },
                    { 2L, "active" },
                    { 3L, "rejected" },
                    { 4L, "paused" },
                    { 5L, "ended" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_cases_trigger_work_order_id",
                table: "cases",
                column: "trigger_work_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_checklists_category_id",
                table: "case_checklists",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_checklists_name",
                table: "case_checklists",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_case_tasks_case_id",
                table: "case_tasks",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_tasks_repair_case_id",
                table: "case_tasks",
                column: "repair_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_tasks_status_id",
                table: "case_tasks",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_checklist_tasks_checklist_id",
                table: "checklist_tasks",
                column: "checklist_id");

            migrationBuilder.CreateIndex(
                name: "ix_frequency_types_name",
                table: "frequency_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_triggers_approving_manager_id",
                table: "maintenance_triggers",
                column: "approving_manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_triggers_frequency_type_id",
                table: "maintenance_triggers",
                column: "frequency_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_triggers_staff_id",
                table: "maintenance_triggers",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_triggers_status_id",
                table: "maintenance_triggers",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_work_order_assets_asset_id",
                table: "maintenance_work_order_assets",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_work_order_assets_technician_id",
                table: "maintenance_work_order_assets",
                column: "technician_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_work_order_details_checklist_id",
                table: "maintenance_work_order_details",
                column: "checklist_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_work_order_details_priority_level_id",
                table: "maintenance_work_order_details",
                column: "priority_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_work_order_details_trigger_id",
                table: "maintenance_work_order_details",
                column: "trigger_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_images_task_id",
                table: "task_images",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_notes_task_id",
                table: "task_notes",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_statuses_name",
                table: "task_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trigger_statuses_name",
                table: "trigger_statuses",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_cases_maintenance_work_order_details_trigger_work_order_id",
                table: "cases",
                column: "trigger_work_order_id",
                principalTable: "maintenance_work_order_details",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cases_maintenance_work_order_details_trigger_work_order_id",
                table: "cases");

            migrationBuilder.DropTable(
                name: "checklist_tasks");

            migrationBuilder.DropTable(
                name: "maintenance_work_order_assets");

            migrationBuilder.DropTable(
                name: "task_images");

            migrationBuilder.DropTable(
                name: "task_notes");

            migrationBuilder.DropTable(
                name: "maintenance_work_order_details");

            migrationBuilder.DropTable(
                name: "case_tasks");

            migrationBuilder.DropTable(
                name: "case_checklists");

            migrationBuilder.DropTable(
                name: "maintenance_triggers");

            migrationBuilder.DropTable(
                name: "task_statuses");

            migrationBuilder.DropTable(
                name: "frequency_types");

            migrationBuilder.DropTable(
                name: "trigger_statuses");

            migrationBuilder.DropIndex(
                name: "ix_cases_trigger_work_order_id",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "trigger_work_order_id",
                table: "cases");
        }
    }
}
