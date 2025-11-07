using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace freela_match_api.Migrations
{
    /// <inheritdoc />
    public partial class Alter_TbUser_Add_IsAvailable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Portfolio_UserId",
                table: "Portfolio",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Portfolio_Users_UserId",
                table: "Portfolio",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Portfolio_Users_UserId",
                table: "Portfolio");

            migrationBuilder.DropIndex(
                name: "IX_Portfolio_UserId",
                table: "Portfolio");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Users");
        }
    }
}
