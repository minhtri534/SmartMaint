using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AssetTreeChangesMeeting1811 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_asset_change_items_kind",
                table: "asset_change_items");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "rooms",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "floors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "buildings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "building_id",
                table: "asset_change_lists",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "asset_change_items",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "building_id",
                table: "asset_change_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_asset_change_items_building_id",
                table: "asset_change_items",
                column: "building_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_asset_change_items_kind",
                table: "asset_change_items",
                sql: "kind IN ('add_building', 'edit_building', 'delete_building', 'add_floor', 'rename_floor', 'delete_floor', 'add_room', 'rename_room', 'delete_room', 'add_asset', 'edit_asset', 'retire_asset', 'delete_asset')");

            migrationBuilder.AddForeignKey(
                name: "fk_asset_change_items_buildings_building_id",
                table: "asset_change_items",
                column: "building_id",
                principalTable: "buildings",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_asset_change_items_buildings_building_id",
                table: "asset_change_items");

            migrationBuilder.DropIndex(
                name: "ix_asset_change_items_building_id",
                table: "asset_change_items");

            migrationBuilder.DropCheckConstraint(
                name: "ck_asset_change_items_kind",
                table: "asset_change_items");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "rooms");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "floors");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "buildings");

            migrationBuilder.DropColumn(
                name: "address",
                table: "asset_change_items");

            migrationBuilder.DropColumn(
                name: "building_id",
                table: "asset_change_items");

            migrationBuilder.AlterColumn<long>(
                name: "building_id",
                table: "asset_change_lists",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_asset_change_items_kind",
                table: "asset_change_items",
                sql: "kind IN ('add_floor', 'rename_floor', 'delete_floor', 'add_room', 'rename_room', 'delete_room', 'add_asset', 'edit_asset', 'retire_asset', 'delete_asset')");
        }
    }
}
