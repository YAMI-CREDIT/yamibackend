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
                    creditorName = table.Column<string>(type: "text", nullable: false),
                    debtorId = table.Column<Guid>(type: "uuid", nullable: false),
                    debtorName = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    amountTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    amountPaid = table.Column<decimal>(type: "numeric", nullable: true),
                    dueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
