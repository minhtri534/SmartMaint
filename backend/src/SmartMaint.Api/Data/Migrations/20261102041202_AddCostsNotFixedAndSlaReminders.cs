using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCostsNotFixedAndSlaReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "not_fixed_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "not_fixed_reason",
                table: "cases",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "sla_overdue_notified_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "sla_warned_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "costs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    case_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_costs", x => x.id);
                    table.CheckConstraint("ck_costs_amount", "amount >= 0");
                    table.ForeignKey(
                        name: "fk_costs_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_costs_case_id",
                table: "costs",
                column: "case_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "costs");

            migrationBuilder.DropColumn(
                name: "not_fixed_at",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "not_fixed_reason",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "sla_overdue_notified_at",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "sla_warned_at",
                table: "cases");
        }
    }
}
