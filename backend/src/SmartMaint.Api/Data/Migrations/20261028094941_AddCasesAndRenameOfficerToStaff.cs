using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCasesAndRenameOfficerToStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_users_role",
                table: "users");

            // Data change between dropping and re-adding the CHECK: the role Officer became Staff (doc "Luồng Chính", 28/10).
            migrationBuilder.Sql("UPDATE users SET role = 'staff' WHERE role = 'officer';");

            migrationBuilder.CreateTable(
                name: "buildings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_buildings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "priority_levels",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sla_hours = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_priority_levels", x => x.id);
                    table.CheckConstraint("ck_priority_levels_code", "code IN ('emergency', 'high', 'normal', 'low')");
                    table.CheckConstraint("ck_priority_levels_sla_hours", "sla_hours > 0");
                });

            migrationBuilder.CreateTable(
                name: "floors",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    building_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_floors", x => x.id);
                    table.ForeignKey(
                        name: "fk_floors_buildings_building_id",
                        column: x => x.building_id,
                        principalTable: "buildings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "manager_building_assignments",
                columns: table => new
                {
                    manager_id = table.Column<long>(type: "bigint", nullable: false),
                    building_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_manager_building_assignments", x => new { x.manager_id, x.building_id });
                    table.ForeignKey(
                        name: "fk_manager_building_assignments_buildings_building_id",
                        column: x => x.building_id,
                        principalTable: "buildings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_manager_building_assignments_users_manager_id",
                        column: x => x.manager_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "staff_building_assignments",
                columns: table => new
                {
                    staff_id = table.Column<long>(type: "bigint", nullable: false),
                    building_id = table.Column<long>(type: "bigint", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staff_building_assignments", x => new { x.staff_id, x.building_id });
                    table.ForeignKey(
                        name: "fk_staff_building_assignments_buildings_building_id",
                        column: x => x.building_id,
                        principalTable: "buildings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_staff_building_assignments_users_staff_id",
                        column: x => x.staff_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "case_categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    group_name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    is_emergency = table.Column<bool>(type: "boolean", nullable: false),
                    default_priority_id = table.Column<long>(type: "bigint", nullable: false),
                    default_cost_bearer = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    photo_required = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_categories", x => x.id);
                    table.CheckConstraint("ck_case_categories_default_cost_bearer", "default_cost_bearer IN ('building', 'customer')");
                    table.ForeignKey(
                        name: "fk_case_categories_priority_levels_default_priority_id",
                        column: x => x.default_priority_id,
                        principalTable: "priority_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    floor_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rooms", x => x.id);
                    table.ForeignKey(
                        name: "fk_rooms_floors_floor_id",
                        column: x => x.floor_id,
                        principalTable: "floors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    room_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assets", x => x.id);
                    table.ForeignKey(
                        name: "fk_assets_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cases",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    requesting_customer_id = table.Column<long>(type: "bigint", nullable: false),
                    room_id = table.Column<long>(type: "bigint", nullable: false),
                    asset_id = table.Column<long>(type: "bigint", nullable: true),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    priority_level_id = table.Column<long>(type: "bigint", nullable: false),
                    cost_bearer = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    staff_id = table.Column<long>(type: "bigint", nullable: true),
                    assigned_technician_id = table.Column<long>(type: "bigint", nullable: true),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    actual_hours = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    outsourced_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    root_cause = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    parts_used = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    hold_reason = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    hold_until = table.Column<DateOnly>(type: "date", nullable: true),
                    rating_stars = table.Column<int>(type: "integer", nullable: true),
                    rating_comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    rated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cases", x => x.id);
                    table.CheckConstraint("ck_cases_actual_hours", "actual_hours IS NULL OR actual_hours > 0");
                    table.CheckConstraint("ck_cases_cost_bearer", "cost_bearer IN ('building', 'customer')");
                    table.CheckConstraint("ck_cases_hold_reason", "hold_reason IN ('waiting_parts', 'waiting_customer', 'waiting_staff_decision', 'waiting_third_party')");
                    table.CheckConstraint("ck_cases_rating_stars", "rating_stars IS NULL OR rating_stars BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_cases_status", "status IN ('pending', 'approved', 'assigned', 'accepted', 'in_progress', 'on_hold', 'completed', 'closed', 'reopened', 'rejected', 'cancelled')");
                    table.ForeignKey(
                        name: "fk_cases_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cases_case_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "case_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cases_priority_levels_priority_level_id",
                        column: x => x.priority_level_id,
                        principalTable: "priority_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cases_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cases_users_assigned_technician_id",
                        column: x => x.assigned_technician_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cases_users_requesting_customer_id",
                        column: x => x.requesting_customer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cases_users_staff_id",
                        column: x => x.staff_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "case_assignments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    case_id = table.Column<long>(type: "bigint", nullable: false),
                    technician_id = table.Column<long>(type: "bigint", nullable: false),
                    assigned_by_id = table.Column<long>(type: "bigint", nullable: false),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    response = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    responded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    decline_reason = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_assignments", x => x.id);
                    table.CheckConstraint("ck_case_assignments_response", "response IN ('waiting', 'accepted', 'declined')");
                    table.ForeignKey(
                        name: "fk_case_assignments_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_case_assignments_users_assigned_by_id",
                        column: x => x.assigned_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_case_assignments_users_technician_id",
                        column: x => x.technician_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "case_images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    case_id = table.Column<long>(type: "bigint", nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_images", x => x.id);
                    table.CheckConstraint("ck_case_images_kind", "kind IN ('report', 'before', 'after')");
                    table.ForeignKey(
                        name: "fk_case_images_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "case_status_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    case_id = table.Column<long>(type: "bigint", nullable: false),
                    from_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    to_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    changed_by_id = table.Column<long>(type: "bigint", nullable: false),
                    reason = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_status_history", x => x.id);
                    table.CheckConstraint("ck_case_status_history_from_status", "from_status IN ('pending', 'approved', 'assigned', 'accepted', 'in_progress', 'on_hold', 'completed', 'closed', 'reopened', 'rejected', 'cancelled')");
                    table.CheckConstraint("ck_case_status_history_to_status", "to_status IN ('pending', 'approved', 'assigned', 'accepted', 'in_progress', 'on_hold', 'completed', 'closed', 'reopened', 'rejected', 'cancelled')");
                    table.ForeignKey(
                        name: "fk_case_status_history_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_case_status_history_users_changed_by_id",
                        column: x => x.changed_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_users_role",
                table: "users",
                sql: "role IN ('customer', 'staff', 'technician', 'manager', 'admin')");

            migrationBuilder.CreateIndex(
                name: "ix_assets_room_id_name",
                table: "assets",
                columns: new[] { "room_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_buildings_name",
                table: "buildings",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_case_assignments_assigned_by_id",
                table: "case_assignments",
                column: "assigned_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_assignments_case_id",
                table: "case_assignments",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_assignments_technician_id",
                table: "case_assignments",
                column: "technician_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_categories_default_priority_id",
                table: "case_categories",
                column: "default_priority_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_categories_group_name_name",
                table: "case_categories",
                columns: new[] { "group_name", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_case_images_case_id",
                table: "case_images",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_status_history_case_id",
                table: "case_status_history",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_status_history_changed_by_id",
                table: "case_status_history",
                column: "changed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_cases_asset_id",
                table: "cases",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_cases_assigned_technician_id",
                table: "cases",
                column: "assigned_technician_id");

            migrationBuilder.CreateIndex(
                name: "ix_cases_category_id",
                table: "cases",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_cases_priority_level_id",
                table: "cases",
                column: "priority_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_cases_requesting_customer_id",
                table: "cases",
                column: "requesting_customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_cases_room_id",
                table: "cases",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "ix_cases_staff_id",
                table: "cases",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "ix_cases_status",
                table: "cases",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_cases_submitted_at",
                table: "cases",
                column: "submitted_at");

            migrationBuilder.CreateIndex(
                name: "ix_floors_building_id_name",
                table: "floors",
                columns: new[] { "building_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_manager_building_assignments_building_id",
                table: "manager_building_assignments",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "ix_priority_levels_code",
                table: "priority_levels",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rooms_floor_id_name",
                table: "rooms",
                columns: new[] { "floor_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_staff_building_assignments_building_id",
                table: "staff_building_assignments",
                column: "building_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_assignments");

            migrationBuilder.DropTable(
                name: "case_images");

            migrationBuilder.DropTable(
                name: "case_status_history");

            migrationBuilder.DropTable(
                name: "manager_building_assignments");

            migrationBuilder.DropTable(
                name: "staff_building_assignments");

            migrationBuilder.DropTable(
                name: "cases");

            migrationBuilder.DropTable(
                name: "assets");

            migrationBuilder.DropTable(
                name: "case_categories");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "priority_levels");

            migrationBuilder.DropTable(
                name: "floors");

            migrationBuilder.DropTable(
                name: "buildings");

            migrationBuilder.DropCheckConstraint(
                name: "ck_users_role",
                table: "users");

            migrationBuilder.Sql("UPDATE users SET role = 'officer' WHERE role = 'staff';");

            migrationBuilder.AddCheckConstraint(
                name: "ck_users_role",
                table: "users",
                sql: "role IN ('customer', 'officer', 'technician', 'manager', 'admin')");
        }
    }
}
