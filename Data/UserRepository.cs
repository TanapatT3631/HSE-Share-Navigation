using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using SharedNavigation.Configuration;
using SharedNavigation.Models;

namespace SharedNavigation.Data
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly UserRegistrationOptions _options;
        private readonly ILogger<SqlUserRepository> _logger;
         public SqlUserRepository(IOptions<UserRegistrationOptions> options, ILogger<SqlUserRepository> logger)
        {
            _options = options.Value;
            _logger = logger;
            
            if (string.IsNullOrEmpty(_options.ConnectionString))
            {
                throw new ArgumentException("Connection string not provided in UserRegistrationOptions");
            }
        }

        public async Task<UserProfile?> GetUserByObjectIdAsync(string azureAdObjectId)
        {
            var sql = $@"
                SELECT Id, AzureAdObjectId, Email, DisplayName, Department, Plant, CreatedAt, UpdatedAt, IsActive, LastedSignInDate
                FROM [{_options.TableName}] 
                WHERE AzureAdObjectId = @AzureAdObjectId AND IsActive = 1";

            try
            {
                using var connection = new SqlConnection(_options.ConnectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.Add("@AzureAdObjectId", SqlDbType.NVarChar, 100).Value = azureAdObjectId;

                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return MapToUserProfile(reader);
                }

                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while getting user by ObjectId: {ObjectId}", azureAdObjectId);
                throw;
            }
        }

        public async Task<bool> UserExistsAsync(string azureAdObjectId)
        {
            var sql = $@"
                SELECT COUNT(1) 
                FROM [{_options.TableName}] 
                WHERE AzureAdObjectId = @AzureAdObjectId AND IsActive = 1";

            try
            {
                using var connection = new SqlConnection(_options.ConnectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                command.Parameters.Add("@AzureAdObjectId", SqlDbType.NVarChar, 100).Value = azureAdObjectId;

                var count = await command.ExecuteScalarAsync();
                return Convert.ToInt32(count) > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while checking if user exists: {ObjectId}", azureAdObjectId);
                throw;
            }
        }

        public async Task<UserProfile> CreateUserAsync(UserProfile userProfile)
        {
            var sql = $@"
                INSERT INTO [{_options.TableName}] (Id, AzureAdObjectId, Email, DisplayName, Department, Plant, CreatedAt, UpdatedAt, IsActive , LastedSignInDate)
                VALUES (@Id, @AzureAdObjectId, @Email, @DisplayName, @Department, @Plant, GETDATE(), @UpdatedAt, @IsActive , GETDATE());
                
                SELECT Id, AzureAdObjectId, Email, DisplayName, Department, Plant, CreatedAt, UpdatedAt, IsActive
                FROM [{_options.TableName}] 
                WHERE Id = @Id";

            try
            {
                userProfile.Id = Guid.NewGuid();
                //userProfile.CreatedAt = DateTime.UtcNow;
                userProfile.IsActive = true;

                using var connection = new SqlConnection(_options.ConnectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                AddUserParameters(command, userProfile);

                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    var result = MapToUserProfile(reader);
                    _logger.LogInformation("Created new user profile for ObjectId: {ObjectId}", userProfile.AzureAdObjectId);
                    return result;
                }

                throw new InvalidOperationException("Failed to create user profile");
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while creating user: {ObjectId}", userProfile.AzureAdObjectId);
                throw;
            }
        }

        public async Task<UserProfile> UpdateUserAsync(UserProfile userProfile)
        {
            var sql = $@"
                UPDATE [{_options.TableName}] 
                SET Email = @Email, 
                    DisplayName = @DisplayName, 
                    Department = @Department,
                    Plant = @Plant,
                    UpdatedAt = GETDATE(),
                    IsActive = @IsActive,
                    LastedSignInDate = GETDATE()
                WHERE Id = @Id;
                
                SELECT Id, AzureAdObjectId, Email, DisplayName, Department, Plant, CreatedAt, UpdatedAt, IsActive, LastedSignInDate
                FROM [{_options.TableName}] 
                WHERE Id = @Id";

            try
            {
                userProfile.UpdatedAt = DateTime.UtcNow;

                using var connection = new SqlConnection(_options.ConnectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(sql, connection);
                AddUserParameters(command, userProfile);

                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    var result = MapToUserProfile(reader);
                    _logger.LogInformation("Updated user profile for ObjectId: {ObjectId}", userProfile.AzureAdObjectId);
                    return result;
                }

                throw new InvalidOperationException("Failed to update user profile");
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while updating user: {ObjectId}", userProfile.AzureAdObjectId);
                throw;
            }
        }

        private static UserProfile MapToUserProfile(SqlDataReader reader)
        {
            return new UserProfile
            {
                Id = reader.GetGuid("Id"),
                AzureAdObjectId = reader.GetString("AzureAdObjectId"),
                Email = reader.GetString("Email"),
                DisplayName = reader.GetString("DisplayName"),
                Department = reader.GetString("Department"),
                Plant = reader.GetString("Plant"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.IsDBNull("UpdatedAt") ? null : reader.GetDateTime("UpdatedAt"),
                IsActive = reader.GetBoolean("IsActive"),
                LastedSignInDate = reader.IsDBNull("UpdatedAt") ? null : reader.GetDateTime("LastedSignInDate")
            };
        }
        private static void AddUserParameters(SqlCommand command, UserProfile userProfile)
        {
            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = userProfile.Id;
            command.Parameters.Add("@AzureAdObjectId", SqlDbType.NVarChar, 100).Value = userProfile.AzureAdObjectId;
            command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = userProfile.Email;
            command.Parameters.Add("@DisplayName", SqlDbType.NVarChar, 255).Value = userProfile.DisplayName;
            command.Parameters.Add("@Department", SqlDbType.NVarChar, 20).Value = userProfile.Department;
            command.Parameters.Add("@Plant", SqlDbType.NVarChar, 4).Value = userProfile.Plant;
            command.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = userProfile.CreatedAt;
            command.Parameters.Add("@UpdatedAt", SqlDbType.DateTime2).Value = (object?)userProfile.UpdatedAt ?? DBNull.Value;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = userProfile.IsActive;
            command.Parameters.Add("@LastedSignInDate", SqlDbType.DateTime2).Value = (object?)userProfile.LastedSignInDate ?? DBNull.Value;
        }
    }
}
