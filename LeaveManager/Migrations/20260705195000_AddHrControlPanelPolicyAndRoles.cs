using LeaveManager.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManager.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260705195000_AddHrControlPanelPolicyAndRoles")]
    public partial class AddHrControlPanelPolicyAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HrPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllowHalfDayLeave = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    BaseRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationRoles", x => x.Id);
                });

            migrationBuilder.AddColumn<int>(
                name: "OrganizationRoleId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDays",
                table: "LeaveApplications",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE LeaveApplications
                SET TotalDays = DATEDIFF(day, FromDate, ToDate) + 1
                WHERE TotalDays = 0;
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM HrPolicies)
                BEGIN
                    INSERT INTO HrPolicies (AllowHalfDayLeave)
                    VALUES (0);
                END
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_OrganizationRoleId",
                table: "Employees",
                column: "OrganizationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationRoles_Name",
                table: "OrganizationRoles",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_OrganizationRoles_OrganizationRoleId",
                table: "Employees",
                column: "OrganizationRoleId",
                principalTable: "OrganizationRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_OrganizationRoles_OrganizationRoleId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "HrPolicies");

            migrationBuilder.DropTable(
                name: "OrganizationRoles");

            migrationBuilder.DropIndex(
                name: "IX_Employees_OrganizationRoleId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "OrganizationRoleId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TotalDays",
                table: "LeaveApplications");
        }
    }
}
