using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManager.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeJoinDateAndMaternityDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "JoinDate",
                table: "Employees",
                type: "date",
                nullable: false,
                defaultValueSql: "CAST(GETUTCDATE() AS date)");

            migrationBuilder.InsertData(
                table: "LeaveTypes",
                columns: new[] { "Id", "AccrualPerMonth", "AdvanceNoticeDays", "IsAccrued", "IsPaid", "Name", "RequiresAdvanceNotice" },
                values: new object[] { 4, 0m, 30, false, true, "Maternity Leave", true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "JoinDate",
                table: "Employees");
        }
    }
}
