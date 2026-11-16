using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <summary>
    /// Turns on row level security for every table (16/11, Supabase showed them all as "UNRESTRICTED").
    /// On Supabase the public schema is also served by its Data API, where the anon key is public: without RLS anyone
    /// with that key could read accounts, cases, costs... RLS with no policy denies that API everything.
    /// The SmartMaint API is not affected: it connects as the tables' owner (postgres on Supabase, smartmaint in
    /// Docker), and an owner is not subject to RLS unless FORCE is set (we do not set it).
    /// A migration that creates a new table must enable RLS on it too (CONVENTIONS, database section).
    /// </summary>
    public partial class EnableRowLevelSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE t record;
                BEGIN
                    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname = current_schema() LOOP
                        EXECUTE format('ALTER TABLE %I ENABLE ROW LEVEL SECURITY', t.tablename);
                    END LOOP;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE t record;
                BEGIN
                    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname = current_schema() LOOP
                        EXECUTE format('ALTER TABLE %I DISABLE ROW LEVEL SECURITY', t.tablename);
                    END LOOP;
                END $$;
                """);
        }
    }
}
