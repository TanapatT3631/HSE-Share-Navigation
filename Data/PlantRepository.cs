using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using SharedNavigation.Configuration;
using SharedNavigation.Models;

namespace SharedNavigation.Data
{
     public class PlantRepository : IPlantRepository
    {
        private readonly UserRegistrationOptions _options;
        private readonly ILogger<PlantRepository> _logger;
        public PlantRepository(IOptions<UserRegistrationOptions> options, ILogger<PlantRepository> logger)
        {
            _options = options.Value;
            _logger = logger;

            if (string.IsNullOrEmpty(_options.ConnectionString))
            {
                throw new ArgumentException("Connection string not provided in UserRegistrationOptions");
            }
        }

        public async Task<List<Plant?>> GetPlantsAsync()
        {
            List<Plant?> result = new List<Plant?>();
            var sql = $@"
                SELECT PLANT_CODE, PLANT_NAME, PLANT_LOCATION, PLANT_COUNTRY, Latitude, Longtitude
                FROM [{_options.TablePlant}]
                ORDER BY PLANT_CODE";
            try
            {
                using var connection = new SqlConnection(_options.ConnectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);


                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(MapToPlant(reader));
                }
                _logger.LogInformation("Retrieved {Count} plants from database", result.Count);
                return result;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while getting Plants");
                throw;
            }
        }
        
        private static Plant MapToPlant(SqlDataReader reader)
        {
            return new Plant
            {
                
                PlantCode = reader.GetString("PLANT_CODE"),
                Name = reader.GetString("PLANT_NAME"),
                Location = reader.GetString("PLANT_LOCATION"),
                Country = reader.GetString("PLANT_COUNTRY"),
                Latitude = reader.IsDBNull("Latitude") ? 0 : reader.GetDecimal("Latitude"),
                Longtitude = reader.IsDBNull("Longtitude") ? 0 : reader.GetDecimal("Longtitude")
                
            };
        }
    }
    
}