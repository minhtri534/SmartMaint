using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <summary>
    /// AssetChangeListsMeeting1811 created asset_change_lists and asset_change_items without row level security, so
    /// Supabase's Data API (public anon key) could read and write them. Same rule as EnableRowLevelSecurity: RLS on, no
    /// policy, so that API gets nothing; the SmartMaint API owns the tables and is not affected.
    /// </summary>
    public partial class EnableRlsOnAssetChangeLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE asset_change_lists ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE asset_change_items ENABLE ROW LEVEL SECURITY;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE asset_change_items DISABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE asset_change_lists DISABLE ROW LEVEL SECURITY;");
        }
    }
}
