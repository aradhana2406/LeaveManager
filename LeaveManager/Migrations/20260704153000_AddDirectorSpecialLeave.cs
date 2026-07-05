using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManager.Migrations
{
    /// <inheritdoc />
    public partial class AddDirectorSpecialLeave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LeaveTypes",
                columns: new[] { "Id", "AccrualPerMonth", "AdvanceNoticeDays", "IsAccrued", "IsPaid", "Name", "RequiresAdvanceNotice" },
                values: new object[] { 5, 0m, 0, false, true, "Director Special Leave", false });

            migrationBuilder.Sql("""
                INSERT INTO EmployeeLeaveBalances (EmployeeId, LeaveTypeId, AllocatedLeaves, UsedLeaves)
                SELECT e.Id, 5, CAST(0 AS decimal(5,2)), CAST(0 AS decimal(5,2))
                FROM Employees e
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM EmployeeLeaveBalances b
                    WHERE b.EmployeeId = e.Id AND b.LeaveTypeId = 5
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM EmployeeLeaveBalances
                WHERE LeaveTypeId = 5;
                """);

            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
