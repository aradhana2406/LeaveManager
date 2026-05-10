using LeaveManager.Common.Enums;
using LeaveManager.Data;
using LeaveManager.Entities;

public static class EmployeeSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Employees.Any()) return;

        context.Employees.AddRange(
            new Employee
            {
                EmployeeID = "EMP001",
                FullName = "Rahul Sharma",
                Email = "rahul@company.com",
                Role = Role.Employee,
                TeamLeadId = "TL001",
                AlternateTeamLeadId = "TL002"
            },
            new Employee
            {
                EmployeeID = "TL001",
                FullName = "Amit Verma",
                Email = "amit@company.com",
                Role = Role.TeamLead,
                TeamLeadId = "HR001",
                AlternateTeamLeadId = "TL002"
            },
            new Employee
            {
                EmployeeID = "TL002",
                FullName = "Priya Singh",
                Email = "priya@company.com",
                Role = Role.TeamLead,
                TeamLeadId = "HR001"
            },
            new Employee
            {
                EmployeeID = "HR001",
                FullName = "Neha Kapoor",
                Email = "neha@company.com",
                Role = Role.HR
            }
        );

        context.SaveChanges();
    }
}