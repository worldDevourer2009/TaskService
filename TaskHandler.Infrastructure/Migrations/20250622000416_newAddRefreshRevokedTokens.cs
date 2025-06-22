using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskHandler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class newAddRefreshRevokedTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "revoked_tokens");

            migrationBuilder.CreateTable(
                name: "revoked_refresh_tokens",
                columns: table => new
                {
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_revoked_refresh_tokens", x => x.token_hash);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "revoked_refresh_tokens");

            migrationBuilder.CreateTable(
                name: "revoked_tokens",
                columns: table => new
                {
                    token_id = table.Column<string>(type: "text", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_revoked_tokens", x => x.token_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_revoked_tokens_token",
                table: "revoked_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_revoked_tokens_user_id",
                table: "revoked_tokens",
                column: "user_id");
        }
    }
}
