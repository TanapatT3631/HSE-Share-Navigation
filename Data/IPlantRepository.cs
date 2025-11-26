using SharedNavigation.Models;
namespace SharedNavigation.Data
{
    public interface IPlantRepository
    {
        Task<List<Plant?>> GetPlantsAsync();
    }
}