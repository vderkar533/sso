namespace SSO.Server.Models;

public sealed class Grade
{
    public int GradeId { get; set; }
    public string GradeCode { get; set; } = string.Empty;
    public string Band { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int HierarchyLevel { get; set; }
}
