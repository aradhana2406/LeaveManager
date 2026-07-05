using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace LeaveManager.Common.Enums;

public static class RoleExtensions
{
    public static bool IsHrRole(this Role role)
    {
        return role == Role.HR || role == Role.HRL2;
    }

    public static bool IsManagerRole(this Role role)
    {
        return role == Role.Manager || role == Role.ManagerL2;
    }

    public static bool IsApproverRole(this Role role)
    {
        return role == Role.TeamLead || role.IsHrRole() || role == Role.OrganizationHead;
    }

    public static string GetDisplayName(this Role role)
    {
        var member = typeof(Role).GetMember(role.ToString()).FirstOrDefault();
        return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? role.ToString();
    }
}
