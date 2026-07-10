using LeaveManager.Common.Enums;
using LeaveManager.Data;
using LeaveManager.Entities;
using Microsoft.EntityFrameworkCore;

public static class EmployeeSeeder
{
    private const string TemporaryRakeshEmail = "aradhana.shinde1@gmail.com";

    public static void Seed(AppDbContext context)
    {
        if (!context.Employees.Any())
        {
            SeedInitialLeadership(context);
        }

        UpdateTemporaryRakeshEmail(context);
        EnsureAradhanaDemoEmployee(context);
    }

    private static void SeedInitialLeadership(AppDbContext context)
    {
        var preeti = new Employee
        {
            EmployeeCode = "HR001",
            FullName = "Preeti Patil",
            Email = "preeti@company.com",
            Department = "Human Resources",
            Designation = "HR Manager",
            JobRole = "HR Operations",
            EmploymentType = "Full-time",
            Location = "Mumbai",
            SalaryStructureDetails = "CTC 14 LPA | Fixed 12 LPA | Variable 2 LPA",
            JoinDate = new DateTime(2024, 4, 8),
            Role = Role.HRL2,
            IsActive = true
        };

        var rakesh = new Employee
        {
            EmployeeCode = "ORG001",
            FullName = "Rakesh Patil",
            Email = TemporaryRakeshEmail,
            Department = "Leadership",
            Designation = "Organization Head",
            JobRole = "Business Leadership",
            EmploymentType = "Full-time",
            Location = "Mumbai",
            SalaryStructureDetails = "Executive compensation structure",
            JoinDate = new DateTime(2023, 1, 2),
            Role = Role.OrganizationHead,
            IsActive = true
        };

        context.Employees.AddRange(preeti, rakesh);
        context.SaveChanges();
        SeedPreetiLogin(context);
        SeedPreetiLeaveBalances(context);
        SeedRakeshLogin(context);
        SeedRakeshLeaveBalances(context);
    }

    private static void ResetToSingleHrRecord(AppDbContext context)
    {
        context.LeaveApplications.RemoveRange(context.LeaveApplications);
        context.EmployeeLeaveBalances.RemoveRange(context.EmployeeLeaveBalances);
        context.EmployeeOnboardingDocuments.RemoveRange(context.EmployeeOnboardingDocuments);
        context.EmployeeOnboardingProfiles.RemoveRange(context.EmployeeOnboardingProfiles);
        context.UserLogins.RemoveRange(context.UserLogins);
        context.EmployeeTeams.RemoveRange(context.EmployeeTeams);

        foreach (var employee in context.Employees)
        {
            employee.PrimaryTeamId = null;
        }

        context.SaveChanges();

        context.Teams.RemoveRange(context.Teams);
        context.Projects.RemoveRange(context.Projects);
        context.Employees.RemoveRange(context.Employees);
        context.SaveChanges();

        context.Employees.Add(new Employee
        {
            EmployeeCode = "HR001",
            FullName = "Preeti Patil",
            Email = "preeti@company.com",
            Department = "Human Resources",
            Designation = "HR Manager",
            JobRole = "HR Operations",
            EmploymentType = "Full-time",
            Location = "Mumbai",
            SalaryStructureDetails = "CTC 14 LPA | Fixed 12 LPA | Variable 2 LPA",
            JoinDate = new DateTime(2024, 4, 8),
            Role = Role.HRL2,
            IsActive = true
        });

        context.SaveChanges();
    }

    private static void SeedPreetiLogin(AppDbContext context)
    {
        var preeti = context.Employees.First(x => x.EmployeeCode == "HR001");

        context.UserLogins.Add(new UserLogin
        {
            EmployeeId = preeti.Id,
            Username = "preeti",
            Password = "demo123",
            IsActive = true
        });

        context.SaveChanges();
    }

    private static void SeedRakeshLogin(AppDbContext context)
    {
        var rakesh = context.Employees.First(x => x.EmployeeCode == "ORG001");

        context.UserLogins.Add(new UserLogin
        {
            EmployeeId = rakesh.Id,
            Username = "rakesh",
            Password = "demo123",
            IsActive = true
        });

        context.SaveChanges();
    }

    private static void UpdateTemporaryRakeshEmail(AppDbContext context)
    {
        var rakesh = context.Employees.FirstOrDefault(x => x.EmployeeCode == "ORG001");
        if (rakesh == null || rakesh.Email == TemporaryRakeshEmail)
        {
            return;
        }

        rakesh.Email = TemporaryRakeshEmail;
        context.SaveChanges();
    }

    private static void EnsureAradhanaDemoEmployee(AppDbContext context)
    {
        var lead = context.Employees.FirstOrDefault(x => x.EmployeeCode == "HR001" && x.IsActive) ??
            context.Employees.FirstOrDefault(x => x.Role.IsHrRole() && x.IsActive);

        if (lead == null)
        {
            return;
        }

        var project = context.Projects.FirstOrDefault(x => x.Name == "People Operations Demo");
        if (project == null)
        {
            project = new Project { Name = "People Operations Demo" };
            context.Projects.Add(project);
            context.SaveChanges();
        }

        var team = context.Teams.FirstOrDefault(x => x.Name == "Employee Support" && x.ProjectId == project.Id);
        if (team == null)
        {
            team = new Team
            {
                Name = "Employee Support",
                ProjectId = project.Id,
                LeadId = lead.Id
            };

            context.Teams.Add(team);
            context.SaveChanges();
        }
        else if (team.LeadId != lead.Id)
        {
            team.LeadId = lead.Id;
            context.SaveChanges();
        }

        var employee = context.Employees.FirstOrDefault(x => x.EmployeeCode == "67") ??
            context.Employees.FirstOrDefault(x => x.Email == "aradhana.shinde@relisfottechnologies.com") ??
            context.Employees.FirstOrDefault(x => x.Email == "aradhana.shinde@company.com") ??
            context.Employees.FirstOrDefault(x => x.EmployeeCode == "ARADHANA");

        if (employee == null)
        {
            employee = new Employee
            {
                EmployeeCode = "67",
                FullName = "Aradhana Shinde",
                Email = "aradhana.shinde@company.com",
                Department = "Engineering",
                Designation = "Software Engineer",
                JobRole = "Software Engineer",
                EmploymentType = "Full-time",
                Location = "Mumbai",
                SalaryStructureDetails = "CTC 8.5 LPA | Fixed 7.2 LPA | Variable 1.3 LPA",
                JoinDate = new DateTime(2026, 7, 9),
                PrimaryTeamId = team.Id,
                Role = Role.Employee,
                IsActive = true
            };

            context.Employees.Add(employee);
            context.SaveChanges();
        }
        else
        {
            employee.FullName = string.IsNullOrWhiteSpace(employee.FullName) ? "Aradhana Shinde" : employee.FullName;
            employee.Email = string.IsNullOrWhiteSpace(employee.Email) ? "aradhana.shinde@company.com" : employee.Email;
            employee.Department = string.IsNullOrWhiteSpace(employee.Department) ? "Engineering" : employee.Department;
            employee.Designation = string.IsNullOrWhiteSpace(employee.Designation) ? "Software Engineer" : employee.Designation;
            employee.JobRole = string.IsNullOrWhiteSpace(employee.JobRole) ? "Software Engineer" : employee.JobRole;
            employee.EmploymentType = string.IsNullOrWhiteSpace(employee.EmploymentType) ? "Full-time" : employee.EmploymentType;
            employee.Location = string.IsNullOrWhiteSpace(employee.Location) ? "Mumbai" : employee.Location;
            employee.SalaryStructureDetails = string.IsNullOrWhiteSpace(employee.SalaryStructureDetails)
                ? "CTC 8.5 LPA | Fixed 7.2 LPA | Variable 1.3 LPA"
                : employee.SalaryStructureDetails;
            employee.PrimaryTeamId ??= team.Id;
            employee.Role = Role.Employee;
            employee.IsActive = true;
            context.SaveChanges();
        }

        var membershipExists = context.EmployeeTeams.Any(x => x.EmployeeId == employee.Id && x.TeamId == team.Id);
        if (!membershipExists)
        {
            context.EmployeeTeams.Add(new EmployeeTeam
            {
                EmployeeId = employee.Id,
                TeamId = team.Id
            });
            context.SaveChanges();
        }

        var employeeLogin = context.UserLogins.FirstOrDefault(x => x.EmployeeId == employee.Id);
        var aradhanaLogin = context.UserLogins.FirstOrDefault(x => x.Username == "aradhana");
        var login = aradhanaLogin ?? employeeLogin;

        if (employeeLogin != null && aradhanaLogin != null && employeeLogin.Id != aradhanaLogin.Id)
        {
            employeeLogin.Username = employee.EmployeeCode.Trim().ToLowerInvariant() + "-legacy";
            employeeLogin.IsActive = false;
        }

        if (login == null)
        {
            context.UserLogins.Add(new UserLogin
            {
                EmployeeId = employee.Id,
                Username = "aradhana",
                Password = "demo123",
                IsActive = true
            });
        }
        else
        {
            login.EmployeeId = employee.Id;
            login.Username = "aradhana";
            login.Password = "demo123";
            login.IsActive = true;
        }

        var duplicateDemoEmployee = context.Employees.FirstOrDefault(x => x.EmployeeCode == "ARADHANA" && x.Id != employee.Id);
        if (duplicateDemoEmployee != null)
        {
            duplicateDemoEmployee.IsActive = false;
        }

        EnsureDefaultLeaveBalances(context, employee.Id);
        context.SaveChanges();
    }

    private static void EnsureDefaultLeaveBalances(AppDbContext context, int employeeId)
    {
        var leaveTypes = context.LeaveTypes.AsNoTracking().ToList();
        var defaultBalances = new Dictionary<string, decimal>
        {
            ["Sick/Casual Leave"] = 7m,
            ["Director Special Leave"] = 0m,
            ["Planned Leave"] = 1m,
            ["Maternity Leave"] = 60m,
            ["Unpaid Leave"] = 0m
        };

        foreach (var leaveType in leaveTypes.Where(x => defaultBalances.ContainsKey(x.Name)))
        {
            var existingBalance = context.EmployeeLeaveBalances
                .FirstOrDefault(x => x.EmployeeId == employeeId && x.LeaveTypeId == leaveType.Id);

            if (existingBalance == null)
            {
                context.EmployeeLeaveBalances.Add(new EmployeeLeaveBalance
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = leaveType.Id,
                    AllocatedLeaves = defaultBalances[leaveType.Name],
                    UsedLeaves = 0m
                });
            }
        }
    }

    private static void SeedPreetiLeaveBalances(AppDbContext context)
    {
        var preeti = context.Employees.First(x => x.EmployeeCode == "HR001");
        var leaveTypes = context.LeaveTypes.AsNoTracking().ToList();
        var defaultBalances = new Dictionary<string, decimal>
        {
            ["Sick/Casual Leave"] = 7m,
            ["Director Special Leave"] = 0m,
            ["Planned Leave"] = 1m,
            ["Maternity Leave"] = 60m,
            ["Unpaid Leave"] = 0m
        };

        var balances = leaveTypes
            .Where(leaveType => defaultBalances.ContainsKey(leaveType.Name))
            .Select(leaveType => new EmployeeLeaveBalance
            {
                EmployeeId = preeti.Id,
                LeaveTypeId = leaveType.Id,
                AllocatedLeaves = defaultBalances[leaveType.Name],
                UsedLeaves = 0m
            });

        context.EmployeeLeaveBalances.AddRange(balances);
        context.SaveChanges();
    }

    private static void SeedRakeshLeaveBalances(AppDbContext context)
    {
        var rakesh = context.Employees.First(x => x.EmployeeCode == "ORG001");
        var leaveTypes = context.LeaveTypes.AsNoTracking().ToList();
        var defaultBalances = new Dictionary<string, decimal>
        {
            ["Sick/Casual Leave"] = 7m,
            ["Director Special Leave"] = 0m,
            ["Planned Leave"] = 1m,
            ["Maternity Leave"] = 60m,
            ["Unpaid Leave"] = 0m
        };

        var balances = leaveTypes
            .Where(leaveType => defaultBalances.ContainsKey(leaveType.Name))
            .Select(leaveType => new EmployeeLeaveBalance
            {
                EmployeeId = rakesh.Id,
                LeaveTypeId = leaveType.Id,
                AllocatedLeaves = defaultBalances[leaveType.Name],
                UsedLeaves = 0m
            });

        context.EmployeeLeaveBalances.AddRange(balances);
        context.SaveChanges();
    }

    private static void SeedBaseEmployeesAndTeams(AppDbContext context)
    {
        var rahul = new Employee
        {
            EmployeeCode = "EMP001",
            FullName = "Rahul Sharma",
            Email = "rahul@company.com",
            Department = "Engineering",
            Designation = "Software Engineer",
            JobRole = ".NET Developer",
            EmploymentType = "Full-time",
            Location = "Bengaluru",
            SalaryStructureDetails = "CTC 8.5 LPA | Fixed 7.2 LPA | Variable 1.3 LPA",
            JoinDate = new DateTime(2026, 1, 13),
            Role = Role.Employee,
            IsActive = true,
        };

        var amit = new Employee
        {
            EmployeeCode = "TL001",
            FullName = "Amit Verma",
            Email = "amit@company.com",
            Department = "Engineering",
            Designation = "Team Lead",
            JobRole = "Engineering Delivery",
            EmploymentType = "Full-time",
            Location = "Pune",
            SalaryStructureDetails = "CTC 16 LPA | Fixed 13.5 LPA | Variable 2.5 LPA",
            JoinDate = new DateTime(2024, 9, 2),
            Role = Role.TeamLead,
            IsActive = true,
        };

        var priya = new Employee
        {
            EmployeeCode = "MGR001",
            FullName = "Priya Singh",
            Email = "priya@company.com",
            Department = "Engineering",
            Designation = "Delivery Manager",
            JobRole = "Technical Delivery",
            EmploymentType = "Full-time",
            Location = "Hyderabad",
            SalaryStructureDetails = "CTC 22 LPA | Fixed 18.5 LPA | Variable 3.5 LPA",
            JoinDate = new DateTime(2025, 3, 17),
            Role = Role.ManagerL2,
            IsActive = true,
        };

        var neha = new Employee
        {
            EmployeeCode = "HR001",
            FullName = "Neha Kapoor",
            Email = "neha@company.com",
            Department = "Human Resources",
            Designation = "HR Manager",
            JobRole = "HR Operations",
            EmploymentType = "Full-time",
            Location = "Mumbai",
            SalaryStructureDetails = "CTC 14 LPA | Fixed 12 LPA | Variable 2 LPA",
            JoinDate = new DateTime(2024, 4, 8),
            Role = Role.HRL2,
            IsActive = true,
        };

        var anita = new Employee
        {
            EmployeeCode = "ORG001",
            FullName = "Anita Deshmukh",
            Email = "anita@company.com",
            Department = "Leadership",
            Designation = "Head of Organization",
            JobRole = "Business Leadership",
            EmploymentType = "Full-time",
            Location = "Mumbai",
            SalaryStructureDetails = "Executive compensation structure",
            JoinDate = new DateTime(2023, 1, 2),
            Role = Role.OrganizationHead,
            IsActive = true,
        };

        context.Employees.AddRange(rahul, amit, priya, neha, anita);
        context.SaveChanges();

        var project = new Project
        {
            Name = "Engineering Delivery"
        };

        context.Projects.Add(project);
        context.SaveChanges();

        var platformTeam = new Team
        {
            Name = "Platform Team",
            ProjectId = project.Id,
            LeadId = amit.Id
        };

        var leadershipTeam = new Team
        {
            Name = "Leadership Team",
            ProjectId = project.Id,
            LeadId = anita.Id
        };

        context.Teams.AddRange(platformTeam, leadershipTeam);
        context.SaveChanges();

        rahul.PrimaryTeamId = platformTeam.Id;
        amit.PrimaryTeamId = leadershipTeam.Id;
        priya.PrimaryTeamId = leadershipTeam.Id;
        neha.PrimaryTeamId = leadershipTeam.Id;
        anita.PrimaryTeamId = leadershipTeam.Id;

        context.EmployeeTeams.AddRange(
            new EmployeeTeam { EmployeeId = rahul.Id, TeamId = platformTeam.Id },
            new EmployeeTeam { EmployeeId = amit.Id, TeamId = platformTeam.Id },
            new EmployeeTeam { EmployeeId = amit.Id, TeamId = leadershipTeam.Id },
            new EmployeeTeam { EmployeeId = priya.Id, TeamId = leadershipTeam.Id },
            new EmployeeTeam { EmployeeId = neha.Id, TeamId = leadershipTeam.Id },
            new EmployeeTeam { EmployeeId = anita.Id, TeamId = leadershipTeam.Id });

        context.SaveChanges();
    }

    private static void SeedDummyLogins(AppDbContext context)
    {
        var loginMap = new Dictionary<string, string>
        {
            ["EMP001"] = "rahul",
            ["TL001"] = "amit",
            ["MGR001"] = "priya",
            ["HR001"] = "neha",
            ["ORG001"] = "anita"
        };

        var employees = context.Employees
            .Where(x => loginMap.Keys.Contains(x.EmployeeCode))
            .ToList();

        foreach (var employee in employees)
        {
            var username = loginMap[employee.EmployeeCode];
            var existing = context.UserLogins.FirstOrDefault(x => x.EmployeeId == employee.Id);
            if (existing == null)
            {
                context.UserLogins.Add(new UserLogin
                {
                    EmployeeId = employee.Id,
                    Username = username,
                    Password = "demo123",
                    IsActive = true
                });
            }
            else
            {
                existing.Username = username;
                existing.Password = "demo123";
                existing.IsActive = true;
            }
        }

        context.SaveChanges();
    }

    private static void BackfillEmployeeProfiles(AppDbContext context)
    {
        var defaults = new Dictionary<string, (string Department, string Designation, string JobRole, string EmploymentType, string Location, string Salary, Role Role)>
        {
            ["EMP001"] = ("Engineering", "Software Engineer", ".NET Developer", "Full-time", "Bengaluru", "CTC 8.5 LPA | Fixed 7.2 LPA | Variable 1.3 LPA", Role.Employee),
            ["TL001"] = ("Engineering", "Team Lead", "Engineering Delivery", "Full-time", "Pune", "CTC 16 LPA | Fixed 13.5 LPA | Variable 2.5 LPA", Role.TeamLead),
            ["MGR001"] = ("Engineering", "Delivery Manager", "Technical Delivery", "Full-time", "Hyderabad", "CTC 22 LPA | Fixed 18.5 LPA | Variable 3.5 LPA", Role.ManagerL2),
            ["HR001"] = ("Human Resources", "HR Manager", "HR Operations", "Full-time", "Mumbai", "CTC 14 LPA | Fixed 12 LPA | Variable 2 LPA", Role.HRL2),
            ["ORG001"] = ("Leadership", "Head of Organization", "Business Leadership", "Full-time", "Mumbai", "Executive compensation structure", Role.OrganizationHead)
        };

        var employees = context.Employees
            .Where(x => defaults.Keys.Contains(x.EmployeeCode))
            .ToList();

        foreach (var employee in employees)
        {
            var profile = defaults[employee.EmployeeCode];
            employee.Department = string.IsNullOrWhiteSpace(employee.Department) ? profile.Department : employee.Department;
            employee.Designation = string.IsNullOrWhiteSpace(employee.Designation) ? profile.Designation : employee.Designation;
            employee.JobRole = string.IsNullOrWhiteSpace(employee.JobRole) ? profile.JobRole : employee.JobRole;
            employee.EmploymentType = string.IsNullOrWhiteSpace(employee.EmploymentType) ? profile.EmploymentType : employee.EmploymentType;
            employee.Location = string.IsNullOrWhiteSpace(employee.Location) ? profile.Location : employee.Location;
            employee.SalaryStructureDetails = string.IsNullOrWhiteSpace(employee.SalaryStructureDetails) ? profile.Salary : employee.SalaryStructureDetails;
            employee.Role = profile.Role;
        }

        context.SaveChanges();
    }

    private static void SeedSampleLeaveBalances(AppDbContext context)
    {
        var employees = context.Employees
            .Where(x => x.EmployeeCode == "EMP001" || x.EmployeeCode == "MGR001" || x.EmployeeCode == "67")
            .ToList();

        if (!employees.Any())
        {
            return;
        }

        var leaveTypes = context.LeaveTypes
            .AsNoTracking()
            .ToList();

        var samples = new Dictionary<string, Dictionary<string, (decimal Allocated, decimal Used)>>
        {
            ["EMP001"] = new Dictionary<string, (decimal, decimal)>
            {
                ["Sick/Casual Leave"] = (7m, 3m),
                ["Director Special Leave"] = (0m, 0m),
                ["Planned Leave"] = (1m, 0m),
                ["Maternity Leave"] = (60m, 0m),
                ["Unpaid Leave"] = (5m, 0m)
            },
            ["MGR001"] = new Dictionary<string, (decimal, decimal)>
            {
                ["Sick/Casual Leave"] = (7m, 2m),
                ["Director Special Leave"] = (0m, 0m),
                ["Planned Leave"] = (1m, 0m),
                ["Maternity Leave"] = (60m, 0m),
                ["Unpaid Leave"] = (4m, 1m)
            },
            ["67"] = new Dictionary<string, (decimal, decimal)>
            {
                ["Sick/Casual Leave"] = (7m, 1m),
                ["Director Special Leave"] = (0m, 0m),
                ["Planned Leave"] = (1m, 0m),
                ["Maternity Leave"] = (60m, 0m),
                ["Unpaid Leave"] = (3m, 0m)
            }
        };

        foreach (var employee in employees)
        {
            if (!samples.TryGetValue(employee.EmployeeCode, out var employeeBalances))
            {
                continue;
            }

            foreach (var leaveType in leaveTypes)
            {
                if (!employeeBalances.TryGetValue(leaveType.Name, out var values))
                {
                    continue;
                }

                var existingBalance = context.EmployeeLeaveBalances
                    .FirstOrDefault(x => x.EmployeeId == employee.Id && x.LeaveTypeId == leaveType.Id);

                if (existingBalance == null)
                {
                    context.EmployeeLeaveBalances.Add(new EmployeeLeaveBalance
                    {
                        EmployeeId = employee.Id,
                        LeaveTypeId = leaveType.Id,
                        AllocatedLeaves = values.Allocated,
                        UsedLeaves = values.Used
                    });
                }
                else
                {
                    existingBalance.AllocatedLeaves = values.Allocated;
                    existingBalance.UsedLeaves = values.Used;
                }
            }
        }

        context.SaveChanges();
    }
}
