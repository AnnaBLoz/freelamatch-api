using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace freela_match_api.Migrations
{
    /// <inheritdoc />
    public partial class Alter_TbProposal_Add_Description : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Proposal",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Proposal");
        }
    }
}
