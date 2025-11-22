using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace freela_match_api.Migrations
{
    /// <inheritdoc />
    public partial class Alter_TbCounterProposal_Add_Company : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "CounterProposal",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CounterProposal_CompanyId",
                table: "CounterProposal",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CounterProposal_Users_CompanyId",
                table: "CounterProposal",
                column: "CompanyId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CounterProposal_Users_CompanyId",
                table: "CounterProposal");

            migrationBuilder.DropIndex(
                name: "IX_CounterProposal_CompanyId",
                table: "CounterProposal");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "CounterProposal");
        }
    }
}
