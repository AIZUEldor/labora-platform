using AutoMapper;
using Labora.Application.DTOs.Jobs;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class SavedJobService : ISavedJobService
{
    private readonly ISavedJobRepository _savedJobRepository;
    private readonly IMapper _mapper;

    public SavedJobService(ISavedJobRepository savedJobRepository, IMapper mapper)
    {
        _savedJobRepository = savedJobRepository;
        _mapper = mapper;
    }

    public async Task<List<JobResponseDto>> GetSavedJobsAsync(Guid userId)
    {
        List<SavedJob> savedJobs = await _savedJobRepository.GetByUserIdAsync(userId);
        return savedJobs.Select(s => _mapper.Map<JobResponseDto>(s.Job)).ToList();
    }

    public async Task SaveJobAsync(Guid userId, Guid jobId)
    {
        SavedJob? existing = await _savedJobRepository.GetByUserAndJobAsync(userId, jobId);
        if (existing is not null)
            throw new InvalidOperationException("Bu ish allaqachon saqlangan.");

        SavedJob savedJob = new SavedJob
        {
            UserId = userId,
            JobId = jobId,
        };
        await _savedJobRepository.AddAsync(savedJob);
    }

    public async Task UnsaveJobAsync(Guid userId, Guid jobId)
    {
        SavedJob? savedJob = await _savedJobRepository.GetByUserAndJobAsync(userId, jobId);
        if (savedJob is null)
            throw new InvalidOperationException("Saqlangan ish topilmadi.");

        await _savedJobRepository.DeleteAsync(savedJob);
    }

    public async Task<bool> IsJobSavedAsync(Guid userId, Guid jobId)
    {
        SavedJob? savedJob = await _savedJobRepository.GetByUserAndJobAsync(userId, jobId);
        return savedJob is not null;
    }
}