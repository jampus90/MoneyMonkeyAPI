using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MoneyMonkey.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateCreditCardsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "credit_cards",
                columns: table => new
                {
                    credit_card_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    brand = table.Column<string>(type: "text", nullable: false),
                    last_four_digits = table.Column<string>(type: "text", nullable: false),
                    closing_day = table.Column<int>(type: "integer", nullable: false),
                    due_day = table.Column<int>(type: "integer", nullable: false),
                    credit_limit = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credit_cards", x => x.credit_card_id);
                    table.ForeignKey(
                        name: "FK_credit_cards_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_credit_cards_user_id",
                table: "credit_cards",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credit_cards");
        }
    }
}
