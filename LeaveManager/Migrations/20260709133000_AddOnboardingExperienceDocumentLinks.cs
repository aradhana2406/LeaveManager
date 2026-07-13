using LeaveManager.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManager.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260709133000_AddOnboardingExperienceDocumentLinks")]
    public partial class AddOnboardingExperienceDocumentLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeOnboardingExperienceId",
                table: "EmployeeOnboardingDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOnboardingDocuments_EmployeeOnboardingExperienceId",
                table: "EmployeeOnboardingDocuments",
                column: "EmployeeOnboardingExperienceId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeOnboardingDocuments_EmployeeOnboardingExperiences_EmployeeOnboardingExperienceId",
                table: "EmployeeOnboardingDocuments",
                column: "EmployeeOnboardingExperienceId",
                principalTable: "EmployeeOnboardingExperiences",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeOnboardingDocuments_EmployeeOnboardingExperiences_EmployeeOnboardingExperienceId",
                table: "EmployeeOnboardingDocuments");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeOnboardingDocuments_EmployeeOnboardingExperienceId",
                table: "EmployeeOnboardingDocuments");

            migrationBuilder.DropColumn(
                name: "EmployeeOnboardingExperienceId",
                table: "EmployeeOnboardingDocuments");
        }
    }
}
