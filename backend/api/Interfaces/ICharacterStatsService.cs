using api.DTOs;

namespace api.Interfaces
{
    public interface ICharacterStatsService
    {
        Task UpdateCharacterStatsAsync(List<SwipeDTO> swipes);
    }
}
