using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class IntakeRulesMeeting1811 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "archived_at",
                table: "specialties",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "room_id",
                table: "customers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "archived_at",
                table: "case_categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "asset_type_id",
                table: "case_categories",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_case_categories_asset_type_id",
                table: "case_categories",
                column: "asset_type_id");

            migrationBuilder.AddForeignKey(
                name: "fk_case_categories_asset_types_asset_type_id",
                table: "case_categories",
                column: "asset_type_id",
                principalTable: "asset_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // The seeded faults that clearly belong to one asset type; the others apply to every type.
            migrationBuilder.Sql("""
                UPDATE case_categories c
                SET asset_type_id = t.id
                FROM (VALUES
                    ('Air conditioning', 'Not cooling',              'AC-SPLIT'),
                    ('Air conditioning', 'Water dripping from unit', 'AC-SPLIT'),
                    ('Elevator',         'Person stuck in elevator', 'ELEVATOR'),
                    ('Elevator',         'Elevator out of service',  'ELEVATOR'),
                    ('Electrical',       'Light not working',        'LIGHT'),
                    ('Electrical',       'Breaker keeps tripping',   'DB')
                ) AS m(grp, name, code)
                JOIN asset_types t ON t.code = m.code
                WHERE c.group_name = m.grp AND c.name = m.name AND c.asset_type_id IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_case_categories_asset_types_asset_type_id",
                table: "case_categories");

            migrationBuilder.DropIndex(
                name: "ix_case_categories_asset_type_id",
                table: "case_categories");

            migrationBuilder.DropColumn(
                name: "archived_at",
                table: "specialties");

            migrationBuilder.DropColumn(
                name: "archived_at",
                table: "case_categories");

            migrationBuilder.DropColumn(
                name: "asset_type_id",
                table: "case_categories");

            migrationBuilder.AlterColumn<long>(
                name: "room_id",
                table: "customers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
