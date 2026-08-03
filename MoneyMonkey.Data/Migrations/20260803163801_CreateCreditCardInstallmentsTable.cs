using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MoneyMonkey.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateCreditCardInstallmentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "credit_card_installments",
                columns: table => new
                {
                    credit_card_installment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    credit_card_purchase_id = table.Column<int>(type: "integer", nullable: false),
                    installment_number = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<decimal>(type: "numeric", nullable: false),
                    invoice_month = table.Column<int>(type: "integer", nullable: false),
                    invoice_year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credit_card_installments", x => x.credit_card_installment_id);
                    table.ForeignKey(
                        name: "FK_credit_card_installments_credit_card_purchases_credit_card_~",
                        column: x => x.credit_card_purchase_id,
                        principalTable: "credit_card_purchases",
                        principalColumn: "credit_card_purchase_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_credit_card_installments_credit_card_purchase_id",
                table: "credit_card_installments",
                column: "credit_card_purchase_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credit_card_installments");
        }
    }
}
