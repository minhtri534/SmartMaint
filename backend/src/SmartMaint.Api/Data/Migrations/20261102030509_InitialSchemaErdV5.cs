using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchemaErdV5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asset_categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    category_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_categories", x => x.id);
                });

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
                name: "case_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "manufacturers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_manufacturers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "priority_levels",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
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
                name: "roles",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "specialties",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_specialties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asset_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_asset_types_asset_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "asset_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "floors",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    building_id = table.Column<long>(type: "bigint", nullable: false),
                    floor_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
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
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false),
                    failed_login_count = table.Column<int>(type: "integer", nullable: false),
                    locked_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                    table.CheckConstraint("ck_accounts_failed_login_count", "failed_login_count >= 0");
                    table.CheckConstraint("ck_accounts_status", "status IN ('active', 'deactivated')");
                    table.ForeignKey(
                        name: "fk_accounts_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
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
                    room_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_common_area = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "admins",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admins", x => x.id);
                    table.ForeignKey(
                        name: "fk_admins_accounts_id",
                        column: x => x.id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "managers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_managers", x => x.id);
                    table.ForeignKey(
                        name: "fk_managers_accounts_id",
                        column: x => x.id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff", x => x.id);
                    table.ForeignKey(
                        name: "fk_staff_accounts_id",
                        column: x => x.id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "technicians",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    hourly_rate = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technicians", x => x.id);
                    table.CheckConstraint("ck_technicians_hourly_rate", "hourly_rate IS NULL OR hourly_rate >= 0");
                    table.ForeignKey(
                        name: "fk_technicians_accounts_id",
                        column: x => x.id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    room_id = table.Column<long>(type: "bigint", nullable: false),
                    type_id = table.Column<long>(type: "bigint", nullable: false),
                    manufacturer_id = table.Column<long>(type: "bigint", nullable: true),
                    has_copyright = table.Column<bool>(type: "boolean", nullable: false),
                    warranty_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assets", x => x.id);
                    table.ForeignKey(
                        name: "fk_assets_asset_types_type_id",
                        column: x => x.type_id,
                        principalTable: "asset_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_manufacturers_manufacturer_id",
                        column: x => x.manufacturer_id,
                        principalTable: "manufacturers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assets_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    room_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                    table.ForeignKey(
                        name: "fk_customers_accounts_id",
                        column: x => x.id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_customers_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
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
                        name: "fk_manager_building_assignments_managers_manager_id",
                        column: x => x.manager_id,
                        principalTable: "managers",
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
                        name: "fk_staff_building_assignments_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "technician_specialties",
                columns: table => new
                {
                    technician_id = table.Column<long>(type: "bigint", nullable: false),
                    specialty_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_technician_specialties", x => new { x.technician_id, x.specialty_id });
                    table.ForeignKey(
                        name: "fk_technician_specialties_specialties_specialty_id",
                        column: x => x.specialty_id,
                        principalTable: "specialties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_technician_specialties_technicians_technician_id",
                        column: x => x.technician_id,
                        principalTable: "technicians",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cases",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    requesting_customer_id = table.Column<long>(type: "bigint", nullable: true),
                    room_id = table.Column<long>(type: "bigint", nullable: false),
                    asset_id = table.Column<long>(type: "bigint", nullable: true),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status_id = table.Column<long>(type: "bigint", nullable: false),
                    priority_level_id = table.Column<long>(type: "bigint", nullable: false),
                    cost_bearer = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    staff_id = table.Column<long>(type: "bigint", nullable: true),
                    assigned_technician_id = table.Column<long>(type: "bigint", nullable: true),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    estimated_completion_hours = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    actual_completion_hours = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
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
                    table.CheckConstraint("ck_cases_actual_completion_hours", "actual_completion_hours IS NULL OR actual_completion_hours > 0");
                    table.CheckConstraint("ck_cases_cost_bearer", "cost_bearer IN ('building', 'customer')");
                    table.CheckConstraint("ck_cases_estimated_completion_hours", "estimated_completion_hours IS NULL OR estimated_completion_hours > 0");
                    table.CheckConstraint("ck_cases_hold_reason", "hold_reason IN ('waiting_parts', 'waiting_customer', 'waiting_staff_decision', 'waiting_third_party')");
                    table.CheckConstraint("ck_cases_rating_stars", "rating_stars IS NULL OR rating_stars BETWEEN 1 AND 5");
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
                        name: "fk_cases_case_statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "case_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cases_customers_requesting_customer_id",
                        column: x => x.requesting_customer_id,
                        principalTable: "customers",
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
                        name: "fk_cases_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cases_technicians_assigned_technician_id",
                        column: x => x.assigned_technician_id,
                        principalTable: "technicians",
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
                    image = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    new_status_id = table.Column<long>(type: "bigint", nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    changed_by_id = table.Column<long>(type: "bigint", nullable: false),
                    reason = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_status_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_case_status_history_accounts_changed_by_id",
                        column: x => x.changed_by_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_case_status_history_case_statuses_new_status_id",
                        column: x => x.new_status_id,
                        principalTable: "case_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_case_status_history_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "technician_rejections",
                columns: table => new
                {
                    technician_id = table.Column<long>(type: "bigint", nullable: false),
                    case_id = table.Column<long>(type: "bigint", nullable: false),
                    rejected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_technician_rejections", x => new { x.case_id, x.technician_id, x.rejected_at });
                    table.ForeignKey(
                        name: "fk_technician_rejections_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_technician_rejections_technicians_technician_id",
                        column: x => x.technician_id,
                        principalTable: "technicians",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "case_statuses",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1L, "pending" },
                    { 2L, "approved" },
                    { 3L, "assigned" },
                    { 4L, "accepted" },
                    { 5L, "in_progress" },
                    { 6L, "on_hold" },
                    { 7L, "completed" },
                    { 8L, "closed" },
                    { 9L, "reopened" },
                    { 10L, "rejected" },
                    { 11L, "cancelled" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1L, "customer" },
                    { 2L, "staff" },
                    { 3L, "technician" },
                    { 4L, "manager" },
                    { 5L, "admin" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_accounts_email",
                table: "accounts",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_role_id",
                table: "accounts",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_accounts_username",
                table: "accounts",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asset_categories_code",
                table: "asset_categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asset_types_category_id",
                table: "asset_types",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_types_code",
                table: "asset_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assets_manufacturer_id",
                table: "assets",
                column: "manufacturer_id");

            migrationBuilder.CreateIndex(
                name: "ix_assets_room_id_name",
                table: "assets",
                columns: new[] { "room_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assets_type_id",
                table: "assets",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "ix_buildings_name",
                table: "buildings",
                column: "name",
                unique: true);

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
                name: "ix_case_status_history_new_status_id",
                table: "case_status_history",
                column: "new_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_statuses_name",
                table: "case_statuses",
                column: "name",
                unique: true);

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
                name: "ix_cases_status_id",
                table: "cases",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_cases_submitted_at",
                table: "cases",
                column: "submitted_at");

            migrationBuilder.CreateIndex(
                name: "ix_customers_room_id",
                table: "customers",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "ix_floors_building_id_floor_number",
                table: "floors",
                columns: new[] { "building_id", "floor_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_manager_building_assignments_building_id",
                table: "manager_building_assignments",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "ix_manufacturers_name",
                table: "manufacturers",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_priority_levels_code",
                table: "priority_levels",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rooms_floor_id_room_number",
                table: "rooms",
                columns: new[] { "floor_id", "room_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_specialties_name",
                table: "specialties",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_staff_building_assignments_building_id",
                table: "staff_building_assignments",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "ix_technician_rejections_technician_id",
                table: "technician_rejections",
                column: "technician_id");

            migrationBuilder.CreateIndex(
                name: "ix_technician_specialties_specialty_id",
                table: "technician_specialties",
                column: "specialty_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "case_images");

            migrationBuilder.DropTable(
                name: "case_status_history");

            migrationBuilder.DropTable(
                name: "manager_building_assignments");

            migrationBuilder.DropTable(
                name: "staff_building_assignments");

            migrationBuilder.DropTable(
                name: "technician_rejections");

            migrationBuilder.DropTable(
                name: "technician_specialties");

            migrationBuilder.DropTable(
                name: "managers");

            migrationBuilder.DropTable(
                name: "cases");

            migrationBuilder.DropTable(
                name: "specialties");

            migrationBuilder.DropTable(
                name: "assets");

            migrationBuilder.DropTable(
                name: "case_categories");

            migrationBuilder.DropTable(
                name: "case_statuses");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "staff");

            migrationBuilder.DropTable(
                name: "technicians");

            migrationBuilder.DropTable(
                name: "asset_types");

            migrationBuilder.DropTable(
                name: "manufacturers");

            migrationBuilder.DropTable(
                name: "priority_levels");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "asset_categories");

            migrationBuilder.DropTable(
                name: "floors");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "buildings");
        }
    }
}
