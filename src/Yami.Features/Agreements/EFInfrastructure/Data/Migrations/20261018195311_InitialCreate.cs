using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agreements.EFInfrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agreements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    creditorId = table.Column<Guid>(type: "uuid", nullable: false),
                    creditorName = table.Column<string>(type: "text", nullable: true),
                    debtorId = table.Column<Guid>(type: "uuid", nullable: false),
                    debtorName = table.Column<string>(type: "text", nullable: true),
                    creditorConsent = table.Column<bool>(type: "boolean", nullable: false),
                    debtorConsent = table.Column<bool>(type: "boolean", nullable: false),
                    purpose = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    amountTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    amountPaid = table.Column<decimal>(type: "numeric", nullable: true),
                    currency = table.Column<string>(type: "text", nullable: false),
                    dueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdBy = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    acceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    settledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    repaymentPlan = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agreements", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agreements");
        }
    }
}
