using Labora.Application.Common;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;

namespace Labora.Application.Tests.Sms;

internal sealed class FakeOtpRepository : IOtpRepository
{
    private readonly Dictionary<Guid, OtpVerification> _rows = new();

    public IReadOnlyCollection<OtpVerification> Rows => _rows.Values;

    public Task<OtpVerification?> GetByIdAsync(Guid id) => throw new NotImplementedException();
    public Task<IEnumerable<OtpVerification>> GetAllAsync() => throw new NotImplementedException();
    public Task<IEnumerable<OtpVerification>> FindAsync(System.Linq.Expressions.Expression<Func<OtpVerification, bool>> predicate) => throw new NotImplementedException();
    public Task DeleteAsync(Guid id) => throw new NotImplementedException();
    public Task<bool> ExistsAsync(Guid id) => throw new NotImplementedException();
    public Task<OtpVerification?> GetByOperationTokenHashAsync(string operationTokenHash) => throw new NotImplementedException();

    public Task<OtpVerification> AddAsync(OtpVerification entity)
    {
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        _rows[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<OtpVerification> UpdateAsync(OtpVerification entity)
    {
        _rows[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<OtpVerification?> GetLatestByPhoneAndPurposeAsync(string phoneNumber, OtpPurpose purpose)
        => Task.FromResult<OtpVerification?>(null);

    public Task<OtpVerification?> GetByIdForUpdateAsync(Guid id)
        => Task.FromResult(_rows.TryGetValue(id, out OtpVerification? row) ? row : null);

    public Task<int> GetSendCountSinceAsync(string phoneNumber, OtpPurpose purpose, DateTime sinceUtc)
        => Task.FromResult(0);
}

internal sealed class FakeOtpSecurityService : IOtpSecurityService
{
    public string GenerateOtpCode() => "123456";
    public string GenerateOperationToken() => "operation-token";
    public string HashOtpCode(Guid verificationId, OtpPurpose purpose, string normalizedPhone, string code) => "code-hash";
    public bool VerifyOtpCode(Guid verificationId, OtpPurpose purpose, string normalizedPhone, string code, string codeHash) => true;
    public string HashOperationToken(Guid verificationId, string rawOperationToken) => "operation-token-hash";
    public bool VerifyOperationToken(Guid verificationId, string rawOperationToken, string operationTokenHash) => true;
}

internal sealed class FakePhoneNumberNormalizer : IPhoneNumberNormalizer
{
    public string Normalize(string phoneNumber) => phoneNumber;
}

internal sealed class FakeOtpAbuseGuard : IOtpAbuseGuard
{
    public Task EnsureNotBlockedAsync(string phoneNumber, OtpRequestContext requestContext) => Task.CompletedTask;

    public Task RecordStartAttemptAsync(string phoneNumber, OtpPurpose purpose, Guid? verificationId, OtpRequestContext requestContext)
        => Task.CompletedTask;

    public Task<OtpBlockVerdict?> EvaluateStartAttemptInCurrentTransactionAsync(
        string phoneNumber, OtpPurpose purpose, Guid? verificationId, OtpRequestContext requestContext)
        => Task.FromResult<OtpBlockVerdict?>(null);

    public Task RecordSmsSentAsync(string phoneNumber, OtpPurpose purpose, Guid? verificationId, OtpRequestContext requestContext)
        => Task.CompletedTask;
}

internal sealed class FakeOtpRequestContextProvider : IOtpRequestContextProvider
{
    public OtpRequestContext GetContext() => new();
}

internal sealed class ThrowingSmsSender : ISmsSender
{
    private readonly Exception _exception;

    public ThrowingSmsSender(Exception exception)
    {
        _exception = exception;
    }

    public Task SendAsync(string phoneNumber, string message) => throw _exception;
}
