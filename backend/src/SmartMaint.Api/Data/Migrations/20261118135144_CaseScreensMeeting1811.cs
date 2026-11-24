using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CaseScreensMeeting1811 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "added_by_id",
                table: "costs",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_costs_added_by_id",
                table: "costs",
                column: "added_by_id");

            migrationBuilder.AddForeignKey(
                name: "fk_costs_accounts_added_by_id",
                table: "costs",
                column: "added_by_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            // Requests already rejected as duplicates: from now on no figure counts them.
            migrationBuilder.Sql("""
                UPDATE cases AS c
                SET merged_into_case_id = target.id
                FROM case_status_history AS h
                JOIN cases AS target ON target.id = substring(h.reason from '\(see CS-0*([0-9]+)\)$')::bigint
                WHERE h.case_id = c.id
                  AND c.status_id = 10
                  AND c.merged_into_case_id IS NULL
                  AND h.reason LIKE 'A case for this problem already exists:%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_costs_accounts_added_by_id",
                table: "costs");

            migrationBuilder.DropIndex(
                name: "ix_costs_added_by_id",
                table: "costs");

            migrationBuilder.DropColumn(
                name: "added_by_id",
                table: "costs");
        }
    }
}
