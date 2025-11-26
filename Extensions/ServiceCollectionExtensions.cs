using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedNavigation.Configuration;
using SharedNavigation.Data;
using SharedNavigation.Services;
using SharedNavigation.ViewComponents;
namespace SharedNavigation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserRegistration(
            this IServiceCollection services,
            string connectionString)
        {
            services.Configure<UserRegistrationOptions>(options =>
            {
                options.ConnectionString = connectionString;
            });


            services.AddScoped<IUserRepository, SqlUserRepository>();
            services.AddScoped<IUserRegistrationService, UserRegistrationService>();

            services.AddDataProtection(); // สำหรับ cookie encryption



            return services;
        }

        public static IServiceCollection AddUserRegistration(
            this IServiceCollection services,
            Action<UserRegistrationOptions> configureOptions)
        {
            services.Configure(configureOptions);
            services.AddScoped<IUserRepository, SqlUserRepository>();
            services.AddScoped<IUserRegistrationService, UserRegistrationService>();

            services.AddDataProtection(); // สำหรับ cookie encryption
            //services.AddScoped<IPlantRepository, PlantRepository>();


            return services;
        }

        public static IServiceCollection AddPlantSelector(
            this IServiceCollection services,
            Action<PlantSelectorConfiguration>? configure = null)
        {
            // Configure options
            var config = new PlantSelectorConfiguration();
            configure?.Invoke(config);
            services.AddSingleton(config);

            // Add required services
            services.AddHttpContextAccessor();
            services.AddScoped<IPlantService, PlantService>();
            services.AddScoped<IPlantRepository, PlantRepository>();
            // Add HTTP Client if API URL is provided
            if (!string.IsNullOrEmpty(config.ApiBaseUrl))
            {
                services.AddHttpClient<IPlantService, PlantService>(client =>
                {
                    client.BaseAddress = new Uri(config.ApiBaseUrl);
                    client.Timeout = config.HttpTimeout;
                });
            }

            return services;
        }

    }
    public class PlantSelectorConfiguration
    {
        public string ApiBaseUrl { get; set; } = "";
        public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(15);
    }
}

