using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManager.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamBasedApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Project_ProjectId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Employees_PrimaryApproverId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Employees_SecondaryApproverId",
                table: "Project");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Project",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Project_PrimaryApproverId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Project_SecondaryApproverId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "PrimaryApproverId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "SecondaryApproverId",
                table: "Project");

            migrationBuilder.RenameTable(
                name: "Project",
                newName: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ProjectId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Employees");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                table: "Projects",
                column: "Id");

            migrationBuilder.AddColumn<int>(
                name: "PrimaryTeamId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    LeadId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Employees_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Teams_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeTeams",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTeams", x => new { x.EmployeeId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_EmployeeTeams_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTeams_TeamId",
                table: "EmployeeTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PrimaryTeamId",
                table: "Employees",
                column: "PrimaryTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_LeadId",
                table: "Teams",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_ProjectId",
                table: "Teams",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Teams_PrimaryTeamId",
                table: "Employees",
                column: "PrimaryTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Teams_PrimaryTeamId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "EmployeeTeams");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                table: "Projects");

            migrationBuilder.RenameTable(
                name: "Projects",
                newName: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PrimaryTeamId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PrimaryTeamId",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrimaryApproverId",
                table: "Project",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SecondaryApproverId",
                table: "Project",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Project",
                table: "Project",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ProjectId",
                table: "Employees",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_PrimaryApproverId",
                table: "Project",
                column: "PrimaryApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_SecondaryApproverId",
                table: "Project",
                column: "SecondaryApproverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Project_ProjectId",
                table: "Employees",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Employees_PrimaryApproverId",
                table: "Project",
                column: "PrimaryApproverId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Employees_SecondaryApproverId",
                table: "Project",
                column: "SecondaryApproverId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
