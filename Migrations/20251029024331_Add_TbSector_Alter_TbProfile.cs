using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace freela_match_api.Migrations
{
    /// <inheritdoc />
    public partial class Add_TbSector_Alter_TbProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SectorId",
                table: "Profiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Profiles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Sector",
                columns: table => new
                {
                    SectorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sector", x => x.SectorId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_SectorId",
                table: "Profiles",
                column: "SectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_Sector_SectorId",
                table: "Profiles",
                column: "SectorId",
                principalTable: "Sector",
                principalColumn: "SectorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_Sector_SectorId",
                table: "Profiles");

            migrationBuilder.DropTable(
                name: "Sector");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_SectorId",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "SectorId",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Profiles");
        }
    }
}
