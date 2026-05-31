 using Labora.Domain.Entities;
using Labora.Domain.Interfaces;

namespace Labora.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<bool> PhoneNumberExistsAsync(string phoneNumber);
    Task<IEnumerable<User>> GetWorkerUsersAsync();
}