using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMaint.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AiConversationsOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ai_messages_conversation_id",
                table: "ai_messages");

            migrationBuilder.DropIndex(
                name: "ix_ai_conversations_user_id",
                table: "ai_conversations");

            migrationBuilder.AlterColumn<string>(
                name: "message",
                table: "ai_messages",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "ix_ai_messages_conversation_id",
                table: "ai_messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_ai_conversations_user_id",
                table: "ai_conversations",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ai_messages_conversation_id",
                table: "ai_messages");

            migrationBuilder.DropIndex(
                name: "ix_ai_conversations_user_id",
                table: "ai_conversations");

            migrationBuilder.AlterColumn<string>(
                name: "message",
                table: "ai_messages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.CreateIndex(
                name: "ix_ai_messages_conversation_id",
                table: "ai_messages",
                column: "conversation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ai_conversations_user_id",
                table: "ai_conversations",
                column: "user_id",
                unique: true);
        }
    }
}
