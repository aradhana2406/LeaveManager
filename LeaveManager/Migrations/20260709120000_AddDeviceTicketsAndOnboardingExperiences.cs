using System;
using LeaveManager.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManager.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260709120000_AddDeviceTicketsAndOnboardingExperiences")]
    public partial class AddDeviceTicketsAndOnboardingExperiences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeDeviceTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    AssignedHrId = table.Column<int>(type: "int", nullable: true),
                    RequestType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DeviceType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NotificationTo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, defaultValue: "devicehelp@company.com"),
                    NotificationCc = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, defaultValue: "hr@company.com"),
                    Subject = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDeviceTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDeviceTickets_Employees_AssignedHrId",
                        column: x => x.AssignedHrId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeDeviceTickets_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeOnboardingExperiences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeOnboardingProfileId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    YearsOfExperience = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    RelievingEmailForwarded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeOnboardingExperiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeOnboardingExperiences_EmployeeOnboardingProfiles_EmployeeOnboardingProfileId",
                        column: x => x.EmployeeOnboardingProfileId,
                        principalTable: "EmployeeOnboardingProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDeviceTicketTimelineEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeDeviceTicketId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDeviceTicketTimelineEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDeviceTicketTimelineEvents_EmployeeDeviceTickets_EmployeeDeviceTicketId",
                        column: x => x.EmployeeDeviceTicketId,
                        principalTable: "EmployeeDeviceTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDeviceTicketTimelineEvents_EmployeeDeviceTicketId",
                table: "EmployeeDeviceTicketTimelineEvents",
                column: "EmployeeDeviceTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDeviceTickets_AssignedHrId",
                table: "EmployeeDeviceTickets",
                column: "AssignedHrId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDeviceTickets_EmployeeId",
                table: "EmployeeDeviceTickets",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOnboardingExperiences_EmployeeOnboardingProfileId",
                table: "EmployeeOnboardingExperiences",
                column: "EmployeeOnboardingProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeDeviceTicketTimelineEvents");

            migrationBuilder.DropTable(
                name: "EmployeeOnboardingExperiences");

            migrationBuilder.DropTable(
                name: "EmployeeDeviceTickets");
        }
    }
}
