using api.Authentication.Models.DTOs;
using MapleTinder.Shared.Models.Entities;

namespace api.Authentication.Interfaces
{
    public interface IAuthService
    {
        Task<ApplicationUser?> RegisterAsync(RegisterDTO request);
        Task<string?> LoginAsync(LoginDTO request);
    }
}
