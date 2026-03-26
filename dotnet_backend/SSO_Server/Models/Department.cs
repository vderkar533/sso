namespace SSO.Server.Models;

public sealed class Department
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
