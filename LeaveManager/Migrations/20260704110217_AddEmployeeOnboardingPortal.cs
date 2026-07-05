using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManager.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeOnboardingPortal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeOnboardingProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    PanNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AadhaarNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HasPriorExperience = table.Column<bool>(type: "bit", nullable: false),
                    PreviousEmployerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    YearsOfExperience = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    RelievingEmailForwarded = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeOnboardingProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeOnboardingProfiles_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeOnboardingDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RelativePath = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    UploadedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmployeeOnboardingProfileId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeOnboardingDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeOnboardingDocuments_EmployeeOnboardingProfiles_EmployeeOnboardingProfileId",
                        column: x => x.EmployeeOnboardingProfileId,
                        principalTable: "EmployeeOnboardingProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeOnboardingDocuments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOnboardingDocuments_EmployeeId",
                table: "EmployeeOnboardingDocuments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOnboardingDocuments_EmployeeOnboardingProfileId",
                table: "EmployeeOnboardingDocuments",
                column: "EmployeeOnboardingProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOnboardingProfiles_EmployeeId",
                table: "EmployeeOnboardingProfiles",
                column: "EmployeeId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeOnboardingDocuments");

            migrationBuilder.DropTable(
                name: "EmployeeOnboardingProfiles");
        }
    }
}
