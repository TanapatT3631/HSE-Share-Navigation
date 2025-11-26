using SharedNavigation.Models;
using Microsoft.Extensions.Logging;
using SharedNavigation.Data;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SharedNavigation.Services
{
    public class PlantService : IPlantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
         private readonly IPlantRepository _plantRepository;
        private const string PLANT_SESSION_KEY = "SelectedPlant_Id";
        private const string PLANTS_CACHE_KEY = "Plants_Cache";
        private const string CACHE_TIMESTAMP_KEY = "Plants_Cache_Timestamp";
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

        private readonly ILogger<PlantService> _logger;
        public event Action<PlantChangedEventArgs>? OnPlantChanged;

        public PlantService(IHttpContextAccessor httpContextAccessor, HttpClient httpClient,ILogger<PlantService> logger,IPlantRepository plantRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _plantRepository = plantRepository;
            _logger = logger;
        }

        public async Task<List<Plant?>> GetPlantsAsync()
        {
            try
            {
                // Check cache first
                var cachedPlants = GetCachedPlants();
                if (cachedPlants?.Any() == true)
                {
                    return cachedPlants;
                }

                // Fetch from API
                return await GetPlantsfromDatabase();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting plants");
                return new List<Plant?>();
            }
            
        }
        private async Task<List<Plant?>> GetPlantsfromDatabase()
        {
            List<Plant?> response = await _plantRepository.GetPlantsAsync();
            return response ?? new List<Plant?>();
        }
        public async Task<Plant?> GetCurrentPlantAsync()
        {
            var plantId = await GetCurrentPlantIdAsync();
            if (string.IsNullOrEmpty(plantId))
                return null;

            // 3. Await the collection of plants.
            var plants = await GetPlantsAsync();

            // 4. Use the string variable 'plantId' in the LINQ query.
            return plants.FirstOrDefault(p => p?.PlantCode == plantId);
        }

        public async Task<string?> GetCurrentPlantIdAsync()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                return await Task.FromResult(session?.GetString(PLANT_SESSION_KEY));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current plant ID");
                return null;
            }
        }

        public async Task SetCurrentPlantAsync(string plantId)
        {
            if (string.IsNullOrWhiteSpace(plantId))
                throw new ArgumentException("Plant ID cannot be null or empty", nameof(plantId));

            if (!await IsValidPlantAsync(plantId))
                throw new InvalidOperationException($"Plant with ID '{plantId}' not found or inactive");

            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null)
                    throw new InvalidOperationException("Session is not available");

                session.SetString(PLANT_SESSION_KEY, plantId);
                
                // Get plant details for event
                var plant = await GetPlantByIdAsync(plantId);
                
                _logger.LogInformation("Plant changed to: {PlantId} - {PlantName}", plantId, plant?.Name ?? "Unknown");
                
                // Trigger event
                OnPlantChanged?.Invoke(new PlantChangedEventArgs
                {
                    PlantId = plantId,
                    PlantName = plant?.Name ?? "",
                    Plant = plant
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting current plant: {PlantId}", plantId);
                throw;
            }

        }
        public async Task<bool> IsValidPlantAsync(string plantId)
        {
            if (string.IsNullOrWhiteSpace(plantId))
                return false;

            var plants = await GetPlantsAsync();
            return plants.Any(p => p?.PlantCode == plantId);
        }
        public async Task RefreshPlantsAsync()
        {
            ClearCache();
            await GetPlantsAsync();
        }
        private void ClearCache()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                session?.Remove(PLANTS_CACHE_KEY);
                session?.Remove(CACHE_TIMESTAMP_KEY);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
            }
        }
        private List<Plant?>? GetCachedPlants()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return null;

                var timestamp = session.GetString(CACHE_TIMESTAMP_KEY);
                if (string.IsNullOrEmpty(timestamp) ||
                    !DateTime.TryParse(timestamp, out var cacheTime) ||
                    DateTime.UtcNow.Subtract(cacheTime) > CacheExpiration)
                {
                    return null;
                }

                var cachedData = session.GetString(PLANTS_CACHE_KEY);
                if (string.IsNullOrEmpty(cachedData))
                    return null;

                return JsonSerializer.Deserialize<List<Plant?>>(cachedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached plants");
                return null;
            }
        }
        private async Task<Plant?> GetPlantByIdAsync(string plantId)
        {
            var plants = await GetPlantsAsync();
            return plants.FirstOrDefault(p => p?.PlantCode == plantId);
        }
    }
}