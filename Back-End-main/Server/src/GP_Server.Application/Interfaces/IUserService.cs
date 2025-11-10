using GP_Server.Application.DTOs.Users;
using GP_Server.Domain.Entities;
using System.Linq.Expressions;

namespace GP_Server.Application.Interfaces;

public interface IUserService
{
    Task CreateUserAsync(CreateUserDTO user);
    Task UpdateUserAsync(CreateUserDTO user, Guid id);
    Task<IEnumerable<DetailedUserDTO>> GetUsersAsync(FilteredDTO filter);
    Task<DetailedUserDTO> GetUserByIdAsync(string id);
    Task<IEnumerable<DetailedUserDTO>> GetUsersWhereAsync(Expression<Func<ApplicationUser, bool>> predicate);
    Task DeleteUserAsync(string id);
}
