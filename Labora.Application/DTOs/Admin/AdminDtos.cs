namespace Labora.Application.DTOs.Admin;

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public int Role { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsVerified { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminJobDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int JobType { get; set; }
    public int Status { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public Guid EmployerId { get; set; }
    public string EmployerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalWorkers { get; set; }
    public int TotalEmployers { get; set; }
    public int BlockedUsers { get; set; }
    public int TotalJobs { get; set; }
    public int ActiveJobs { get; set; }
    public int ClosedJobs { get; set; }
    public int TotalApplications { get; set; }
    public int TotalCategories { get; set; }
}

public class BlockUserRequestDto
{
    public bool IsBlocked { get; set; }
}