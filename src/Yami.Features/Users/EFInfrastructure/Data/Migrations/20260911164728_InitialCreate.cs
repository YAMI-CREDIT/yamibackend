using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.EFInfrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    businessName = table.Column<string>(type: "text", nullable: true),
                    area = table.Column<string>(type: "text", nullable: true),
                    identityType = table.Column<string>(type: "text", nullable: true),
                    identityNumber = table.Column<string>(type: "text", nullable: true),
                    userType = table.Column<string>(type: "text", nullable: true),
                    dateOfBirth = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
