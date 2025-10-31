using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace freela_match_api.Migrations
{
    /// <inheritdoc />
    public partial class Alter_TbProposal_Alter_Skill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProposalSkill_UserSkills_SkillId",
                table: "ProposalSkill");

            migrationBuilder.AddForeignKey(
                name: "FK_ProposalSkill_Skills_SkillId",
                table: "ProposalSkill",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "SkillId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProposalSkill_Skills_SkillId",
                table: "ProposalSkill");

            migrationBuilder.AddForeignKey(
                name: "FK_ProposalSkill_UserSkills_SkillId",
                table: "ProposalSkill",
                column: "SkillId",
                principalTable: "UserSkills",
                principalColumn: "UserSkillId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
