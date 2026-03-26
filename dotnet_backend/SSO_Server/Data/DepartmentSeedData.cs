using SSO.Server.Models;

namespace SSO.Server.Data;

public static class DepartmentSeedData
{
    public static readonly Department[] All =
    [
        new Department { DepartmentId = 1, DepartmentName = "Admin" },
        new Department { DepartmentId = 2, DepartmentName = "IT" },
        new Department { DepartmentId = 3, DepartmentName = "HR" },
        new Department { DepartmentId = 4, DepartmentName = "Safety" },
        new Department { DepartmentId = 5, DepartmentName = "Mechanical" },
        new Department { DepartmentId = 6, DepartmentName = "Civil" },
        new Department { DepartmentId = 7, DepartmentName = "Electrical" },
        new Department { DepartmentId = 8, DepartmentName = "Security" },
        new Department { DepartmentId = 9, DepartmentName = "CSR" },
        new Department { DepartmentId = 10, DepartmentName = "Projects" }
    ];
}
