using SharedNavigation.Models;
using Microsoft.Extensions.Logging;
using SharedNavigation.Data;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using SharedNavigation.Extensions;

namespace SharedNavigation.Services
{
    public class PlantService : IPlantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPlantRepository _plantRepository;
        private readonly ILogger<PlantService> _logger;
        private const string PLANT_SESSION_KEY = "SelectedPlant_Id";
        private const string PLANT_COOKIE_KEY = "SelectedPlant";
        private const string PLANTS_CACHE_KEY = "Plants_Cache";
        private const string CACHE_TIMESTAMP_KEY = "Plants_Cache_Timestamp";
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan CookieExpiration = TimeSpan.FromDays(30);

        public event Action<PlantChangedEventArgs>? OnPlantChanged;

        public PlantService(
            IHttpContextAccessor httpContextAccessor, 
            ILogger<PlantService> logger, 
            IPlantRepository plantRepository)
        {
            _httpContextAccessor = httpContextAccessor;
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

                // Fetch from Database
                var plants = await GetPlantsFromDatabase();
                
                // Cache the results
                if (plants?.Any() == true)
                {
                    CachePlants(plants);
                }

                return plants ?? new List<Plant?>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting plants");
                return new List<Plant?>();
            }
        }

        private async Task<List<Plant?>> GetPlantsFromDatabase()
        {
            try
            {
                var plants = await _plantRepository.GetPlantsAsync();
                _logger.LogInformation("Loaded {Count} plants from database", plants?.Count ?? 0);
                return plants ?? new List<Plant?>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading plants from database");
                return new List<Plant?>();
            }
        }

        public async Task<Plant?> GetCurrentPlantAsync()
        {
            try
            {
                var plantId = await GetCurrentPlantIdAsync();
                
                // If no plant selected, try to get from user's default plant
                if (string.IsNullOrEmpty(plantId))
                {
                    plantId = GetUserDefaultPlant();
                    
                    // Set as current if found
                    if (!string.IsNullOrEmpty(plantId))
                    {
                        await SetCurrentPlantAsync(plantId);
                    }
                }

                if (string.IsNullOrEmpty(plantId))
                    return null;

                var plants = await GetPlantsAsync();
                return plants.FirstOrDefault(p => p?.PlantCode == plantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current plant");
                return null;
            }
        }

        public async Task<string?> GetCurrentPlantIdAsync()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return null;

                // // Try Session first
                // var sessionValue = context.Session?.GetString(PLANT_SESSION_KEY);
                // if (!string.IsNullOrEmpty(sessionValue))
                // {
                //     return sessionValue;
                // }

                // Try Cookie
                var cookieValue = context.Request.Cookies[PLANT_COOKIE_KEY];
                if (!string.IsNullOrEmpty(cookieValue))
                {
                    // Restore to session
                    context.Session?.SetString(PLANT_SESSION_KEY, cookieValue);
                    return cookieValue;
                }

                return null;
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
                var context = _httpContextAccessor.HttpContext;
                if (context == null)
                    throw new InvalidOperationException("HttpContext is not available");

                // Set in Session
                context.Session?.SetString(PLANT_SESSION_KEY, plantId);

                // Set in Cookie (persistent)
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.Add(CookieExpiration),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                };
                context.Response.Cookies.Append(PLANT_COOKIE_KEY, plantId, cookieOptions);

                // Get plant details for event
                var plant = await GetPlantByIdAsync(plantId);

                _logger.LogInformation(
                    "Plant changed to: {PlantId} - {PlantName} by user {User}", 
                    plantId, 
                    plant?.Name ?? "Unknown",
                    context.User?.GetEmail() ?? "Unknown"
                );

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

        // Private Helper Methods
        private string? GetUserDefaultPlant()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context?.User?.Identity?.IsAuthenticated == true)
                {
                    // Get from user claims or session
                    var plant = context.User.GetPlant();
                    if (!string.IsNullOrEmpty(plant))
                    {
                        return plant;
                    }

                    // Get from session
                    return context.Session?.GetString("Plant");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user default plant");
            }

            return null;
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

        private void CachePlants(List<Plant?> plants)
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return;

                var json = JsonSerializer.Serialize(plants);
                session.SetString(PLANTS_CACHE_KEY, json);
                session.SetString(CACHE_TIMESTAMP_KEY, DateTime.UtcNow.ToString("O"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching plants");
            }
        }

        private async Task<Plant?> GetPlantByIdAsync(string plantId)
        {
            var plants = await GetPlantsAsync();
            return plants.FirstOrDefault(p => p?.PlantCode == plantId);
        }
    }
}