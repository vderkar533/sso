namespace SSO.Server.DTOs;

public sealed class DepartmentOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class GradeOption
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int HierarchyLevel { get; set; }
}
