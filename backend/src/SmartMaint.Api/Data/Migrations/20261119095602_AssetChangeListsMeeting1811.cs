using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AssetChangeListsMeeting1811 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asset_change_lists",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    building_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    staff_id = table.Column<long>(type: "bigint", nullable: false),
                    reviewer_id = table.Column<long>(type: "bigint", nullable: true),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reject_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_change_lists", x => x.id);
                    table.CheckConstraint("ck_asset_change_lists_status", "status IN ('draft', 'pending', 'approved', 'rejected')");
                    table.ForeignKey(
                        name: "fk_asset_change_lists_buildings_building_id",
                        column: x => x.building_id,
                        principalTable: "buildings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asset_change_lists_managers_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "managers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asset_change_lists_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asset_change_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    list_id = table.Column<long>(type: "bigint", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    key = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    parent_key = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    floor_id = table.Column<long>(type: "bigint", nullable: true),
                    room_id = table.Column<long>(type: "bigint", nullable: true),
                    asset_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    type_id = table.Column<long>(type: "bigint", nullable: true),
                    manufacturer_id = table.Column<long>(type: "bigint", nullable: true),
                    has_copyright = table.Column<bool>(type: "boolean", nullable: true),
                    warranty_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    target_label = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_change_items", x => x.id);
                    table.CheckConstraint("ck_asset_change_items_kind", "kind IN ('add_floor', 'rename_floor', 'delete_floor', 'add_room', 'rename_room', 'delete_room', 'add_asset', 'edit_asset', 'retire_asset', 'delete_asset')");
                    table.ForeignKey(
                        name: "fk_asset_change_items_asset_change_lists_list_id",
                        column: x => x.list_id,
                        principalTable: "asset_change_lists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asset_change_items_asset_types_type_id",
                        column: x => x.type_id,
                        principalTable: "asset_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asset_change_items_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_asset_change_items_floors_floor_id",
                        column: x => x.floor_id,
                        principalTable: "floors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_asset_change_items_manufacturers_manufacturer_id",
                        column: x => x.manufacturer_id,
                        principalTable: "manufacturers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asset_change_items_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asset_change_items_asset_id",
                table: "asset_change_items",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_change_items_floor_id",
                table: "asset_change_items",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_change_items_list_id_key",
                table: "asset_change_items",
                columns: new[] { "list_id", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asset_change_items_manufacturer_id",
                table: "asset_change_items",
                column: "manufacturer_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_change_items_room_id",
                table: "asset_change_items",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_change_items_type_id",
                table: "asset_change_items",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_change_lists_building_id_status",
                table: "asset_change_lists",
                columns: new[] { "building_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_change_lists_reviewer_id",
                table: "asset_change_lists",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_change_lists_staff_id",
                table: "asset_change_lists",
                column: "staff_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asset_change_items");

            migrationBuilder.DropTable(
                name: "asset_change_lists");
        }
    }
}
