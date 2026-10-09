using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kyc.EFInfrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KycVerifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entityId = table.Column<Guid>(type: "uuid", nullable: false),
                    entityType = table.Column<string>(type: "text", nullable: false),
                    identityType = table.Column<string>(type: "text", nullable: false),
                    identityNumber = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    failureReason = table.Column<string>(type: "text", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KycVerifications", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KycVerifications_entityId",
                table: "KycVerifications",
                column: "entityId",
                unique: true,
                filter: "\"status\" = 'InProgress'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KycVerifications");
        }
    }
}
