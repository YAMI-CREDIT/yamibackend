using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agreements.EFInfrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Installments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agreementId = table.Column<Guid>(type: "uuid", nullable: false),
                    dueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    amountDue = table.Column<decimal>(type: "numeric", nullable: false),
                    acceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Installments", x => x.id);
                    table.ForeignKey(
                        name: "FK_Installments_Agreements_agreementId",
                        column: x => x.agreementId,
                        principalTable: "Agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Installments_agreementId",
                table: "Installments",
                column: "agreementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Installments");
        }
    }
}
