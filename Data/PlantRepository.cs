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
                FROM [{_options.TablePlant}]";
            try
            {
                using var connection = new SqlConnection(_options.ConnectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);


                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    result.Add(MapToPlant(reader));
                }

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
                
                PlantCode = reader.GetString("AzureAdObjectId"),
                Name = reader.GetString("Email"),
                Location = reader.GetString("DisplayName"),
                Country = reader.GetString("Department"),
                Latitude = reader.GetDecimal("Plant"),
                Longtitude = reader.GetDecimal("CreatedAt"),
                
            };
        }
    }
    
}