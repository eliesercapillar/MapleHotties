using MapleTinder.Shared.Models.Entities;

namespace api.Authentication.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user);
    }
}
