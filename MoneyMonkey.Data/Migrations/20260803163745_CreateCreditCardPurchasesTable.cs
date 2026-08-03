using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MoneyMonkey.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateCreditCardPurchasesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "credit_card_purchases",
                columns: table => new
                {
                    credit_card_purchase_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    credit_card_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    total_value = table.Column<decimal>(type: "numeric", nullable: false),
                    purchase_date = table.Column<DateOnly>(type: "date", nullable: false),
                    installments_count = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: true),
                    is_subscription = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credit_card_purchases", x => x.credit_card_purchase_id);
                    table.ForeignKey(
                        name: "FK_credit_card_purchases_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "category_id");
                    table.ForeignKey(
                        name: "FK_credit_card_purchases_credit_cards_credit_card_id",
                        column: x => x.credit_card_id,
                        principalTable: "credit_cards",
                        principalColumn: "credit_card_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_credit_card_purchases_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_credit_card_purchases_category_id",
                table: "credit_card_purchases",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_credit_card_purchases_credit_card_id",
                table: "credit_card_purchases",
                column: "credit_card_id");

            migrationBuilder.CreateIndex(
                name: "IX_credit_card_purchases_user_id",
                table: "credit_card_purchases",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credit_card_purchases");
        }
    }
}
