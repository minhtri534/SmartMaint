using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRepairOrReplace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_case_images_kind",
                table: "case_images");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "assessed_at",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "assessment_cause",
                table: "cases",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "assessment_condition",
                table: "cases",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "assessment_recommendation",
                table: "cases",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_survey",
                table: "cases",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "replacement_model",
                table: "cases",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "replacement_serial",
                table: "cases",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "replaces_asset_id",
                table: "assets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "expected_life_years",
                table: "asset_types",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "asset_replacement_requests",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    case_id = table.Column<long>(type: "bigint", nullable: false),
                    old_asset_id = table.Column<long>(type: "bigint", nullable: false),
                    new_model = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    new_serial = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    requested_by_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    new_asset_id = table.Column<long>(type: "bigint", nullable: true),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    dismiss_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asset_replacement_requests", x => x.id);
                    table.CheckConstraint("ck_asset_replacement_requests_status", "status IN ('open', 'done', 'dismissed')");
                    table.ForeignKey(
                        name: "fk_asset_replacement_requests_assets_new_asset_id",
                        column: x => x.new_asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asset_replacement_requests_assets_old_asset_id",
                        column: x => x.old_asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asset_replacement_requests_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asset_replacement_requests_staff_requested_by_id",
                        column: x => x.requested_by_id,
                        principalTable: "staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "case_third_parties",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    case_id = table.Column<long>(type: "bigint", nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    job = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    company_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    contact = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    appointment_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    redo_count = table.Column<int>(type: "integer", nullable: false),
                    quote_bearer = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    quote_sent_to = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    quote_sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    quote_reminded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    decision_go = table.Column<bool>(type: "boolean", nullable: true),
                    decided_by = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    evidence_image = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_third_parties", x => x.id);
                    table.CheckConstraint("ck_case_third_parties_job", "job IN ('repair', 'replace')");
                    table.CheckConstraint("ck_case_third_parties_kind", "kind IN ('warranty', 'contract', 'quote')");
                    table.CheckConstraint("ck_case_third_parties_quote_bearer", "quote_bearer IN ('building', 'customer')");
                    table.CheckConstraint("ck_case_third_parties_redo_count", "redo_count >= 0");
                    table.CheckConstraint("ck_case_third_parties_status", "status IN ('quoting', 'awaiting_decision', 'working', 'failed_check', 'declined', 'done', 'cancelled')");
                    table.ForeignKey(
                        name: "fk_case_third_parties_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_cases_assessment_recommendation",
                table: "cases",
                sql: "assessment_recommendation IN ('repairable', 'replace', 'specialist')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_case_images_kind",
                table: "case_images",
                sql: "kind IN ('report', 'before', 'after', 'assessment')");

            migrationBuilder.CreateIndex(
                name: "ix_assets_replaces_asset_id",
                table: "assets",
                column: "replaces_asset_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_asset_types_expected_life_years",
                table: "asset_types",
                sql: "expected_life_years IS NULL OR expected_life_years BETWEEN 1 AND 100");

            migrationBuilder.CreateIndex(
                name: "ix_asset_replacement_requests_case_id",
                table: "asset_replacement_requests",
                column: "case_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asset_replacement_requests_new_asset_id",
                table: "asset_replacement_requests",
                column: "new_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_replacement_requests_old_asset_id",
                table: "asset_replacement_requests",
                column: "old_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_asset_replacement_requests_requested_by_id",
                table: "asset_replacement_requests",
                column: "requested_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_case_third_parties_case_id",
                table: "case_third_parties",
                column: "case_id");

            migrationBuilder.AddForeignKey(
                name: "fk_assets_assets_replaces_asset_id",
                table: "assets",
                column: "replaces_asset_id",
                principalTable: "assets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_assets_assets_replaces_asset_id",
                table: "assets");

            migrationBuilder.DropTable(
                name: "asset_replacement_requests");

            migrationBuilder.DropTable(
                name: "case_third_parties");

            migrationBuilder.DropCheckConstraint(
                name: "ck_cases_assessment_recommendation",
                table: "cases");

            migrationBuilder.DropCheckConstraint(
                name: "ck_case_images_kind",
                table: "case_images");

            migrationBuilder.DropIndex(
                name: "ix_assets_replaces_asset_id",
                table: "assets");

            migrationBuilder.DropCheckConstraint(
                name: "ck_asset_types_expected_life_years",
                table: "asset_types");

            migrationBuilder.DropColumn(
                name: "assessed_at",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "assessment_cause",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "assessment_condition",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "assessment_recommendation",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "is_survey",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "replacement_model",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "replacement_serial",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "replaces_asset_id",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "expected_life_years",
                table: "asset_types");

            migrationBuilder.AddCheckConstraint(
                name: "ck_case_images_kind",
                table: "case_images",
                sql: "kind IN ('report', 'before', 'after')");
        }
    }
}
