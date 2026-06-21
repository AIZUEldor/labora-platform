using Labora.Application.DTOs.Admin;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly ICategoryRepository _categoryRepository;

    public AdminService(
        IUserRepository userRepository,
        IJobRepository jobRepository,
        IJobApplicationRepository jobApplicationRepository,
        ICategoryRepository categoryRepository)
    {
        _userRepository = userRepository;
        _jobRepository = jobRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
    {
        IEnumerable<User> users = await _userRepository.GetAllAsync();

        return users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Age = u.Age,
                PhoneNumber = u.PhoneNumber,
                Role = (int)u.Role,
                City = u.City,
                Country = u.Country,
                IsVerified = u.IsVerified,
                IsBlocked = u.IsBlocked,
                IsDeleted = u.IsDeleted,
                CreatedAt = u.CreatedAt,
            });
    }

    public async Task<AdminUserDto> BlockUserAsync(Guid userId, bool isBlocked)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException($"Id={userId} bo'lgan foydalanuvchi topilmadi.");

        user.IsBlocked = isBlocked;
        User updatedUser = await _userRepository.UpdateAsync(user);

        return new AdminUserDto
        {
            Id = updatedUser.Id,
            FirstName = updatedUser.FirstName,
            LastName = updatedUser.LastName,
            Age = updatedUser.Age,
            PhoneNumber = updatedUser.PhoneNumber,
            Role = (int)updatedUser.Role,
            City = updatedUser.City,
            Country = updatedUser.Country,
            IsVerified = updatedUser.IsVerified,
            IsBlocked = updatedUser.IsBlocked,
            IsDeleted = updatedUser.IsDeleted,
            CreatedAt = updatedUser.CreatedAt,
        };
    }

    public async Task<IEnumerable<AdminJobDto>> GetAllJobsAsync()
    {
        IEnumerable<Job> jobs = await _jobRepository.GetAllAsync();
        IEnumerable<User> users = await _userRepository.GetAllAsync();

        Dictionary<Guid, string> employerNames = users.ToDictionary(
            u => u.Id,
            u => $"{u.FirstName} {u.LastName}");

        return jobs
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new AdminJobDto
            {
                Id = j.Id,
                Title = j.Title,
                Salary = j.Salary,
                JobType = (int)j.JobType,
                Status = (int)j.Status,
                City = j.City,
                Country = j.Country,
                EmployerId = j.EmployerId,
                EmployerName = employerNames.TryGetValue(j.EmployerId, out string? name) ? name : "—",
                CreatedAt = j.CreatedAt,
            });
    }

    public async Task DeleteJobAsync(Guid jobId)
    {
        Job? job = await _jobRepository.GetByIdAsync(jobId);
        if (job is null)
            throw new InvalidOperationException($"Id={jobId} bo'lgan ish topilmadi.");

        job.IsDeleted = true;
        await _jobRepository.UpdateAsync(job);
    }

    public async Task<AdminStatsDto> GetStatsAsync()
    {
        IEnumerable<User> users = await _userRepository.GetAllAsync();
        IEnumerable<Job> jobs = await _jobRepository.GetAllAsync();
        IEnumerable<JobApplication> applications = await _jobApplicationRepository.GetAllAsync();
        IEnumerable<Category> categories = await _categoryRepository.GetAllAsync();

        return new AdminStatsDto
        {
            TotalUsers = users.Count(),
            TotalWorkers = users.Count(u => u.Role == UserRole.Worker),
            TotalEmployers = users.Count(u => u.Role == UserRole.Employer),
            BlockedUsers = users.Count(u => u.IsBlocked),
            TotalJobs = jobs.Count(),
            ActiveJobs = jobs.Count(j => j.Status == JobStatus.Active),
            ClosedJobs = jobs.Count(j => j.Status == JobStatus.Closed),
            TotalApplications = applications.Count(),
            TotalCategories = categories.Count(),
        };
    }
}