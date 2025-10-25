using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace freela_match_api.Migrations
{
    /// <inheritdoc />
    public partial class Alter_TbPortfolio_Add_IsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Portfolio",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Portfolio");
        }
    }
}
