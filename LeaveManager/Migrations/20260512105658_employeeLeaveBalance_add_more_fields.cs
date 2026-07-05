using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManager.Migrations
{
    /// <inheritdoc />
    public partial class employeeLeaveBalance_add_more_fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AppliedOn",
                table: "LeaveApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalReason",
                table: "LeaveApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "LeaveApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOn",
                table: "LeaveApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedById",
                table: "LeaveApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedOn",
                table: "LeaveApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_ApproverId",
                table: "LeaveApplications",
                column: "ApproverId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveApplications_Employees_ApproverId",
                table: "LeaveApplications",
                column: "ApproverId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveApplications_Employees_ApproverId",
                table: "LeaveApplications");

            migrationBuilder.DropIndex(
                name: "IX_LeaveApplications_ApproverId",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "AppliedOn",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "ApprovalReason",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "ApprovedOn",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "RejectedById",
                table: "LeaveApplications");

            migrationBuilder.DropColumn(
                name: "RejectedOn",
                table: "LeaveApplications");
        }
    }
}
