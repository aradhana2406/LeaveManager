using System.ComponentModel.DataAnnotations;

namespace LeaveManager.Common.Enums;

/// <summary>
/// Enum representing different employee roles in the system
/// </summary>
public enum Role
{
    [Display(Name = "Software Engineer")]
    Employee = 1,

    [Display(Name = "Team Lead")]
    TeamLead = 2,

    [Display(Name = "HR L1")]
    HR = 3,

    [Display(Name = "Senior Software Engineer")]
    SeniorSoftwareEngineer = 4,

    [Display(Name = "Technical Manager L1")]
    Manager = 5,

    [Display(Name = "Organization Head")]
    OrganizationHead = 6,

    [Display(Name = "HR L2")]
    HRL2 = 7,

    [Display(Name = "Technical Manager L2")]
    ManagerL2 = 8
}
