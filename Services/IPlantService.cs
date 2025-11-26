using SharedNavigation.Models;
namespace SharedNavigation.Services
{
    public interface IPlantService
    {
        Task<List<Plant?>> GetPlantsAsync();
        Task<Plant?> GetCurrentPlantAsync();
        Task<string?> GetCurrentPlantIdAsync();
        Task SetCurrentPlantAsync(string plantId);
        //Task ClearCurrentPlantAsync();
        Task<bool> IsValidPlantAsync(string plantId);
        
        // Cache Methods
        Task RefreshPlantsAsync();
        
        // Events
        event Action<PlantChangedEventArgs>? OnPlantChanged;


    }
    public class PlantChangedEventArgs
    {
        public string PlantId { get; set; } = string.Empty;
        public string PlantName { get; set; } = string.Empty;
        public Plant? Plant { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
