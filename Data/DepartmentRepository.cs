using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using SharedNavigation.Configuration;
using SharedNavigation.Models;

namespace SharedNavigation.Data
{
     public class DepartmentRepository
    {
        private readonly DepatmentOprions _options;
        private readonly ILogger<DepartmentRepository> _logger;
         public DepartmentRepository(IOptions<DepatmentOprions> options, ILogger<DepartmentRepository> logger)
        {
            _options = options.Value;
            _logger = logger;
            
            if (string.IsNullOrEmpty(_options.ConnectionString))
            {
                throw new ArgumentException("Connection string not provided in UserRegistrationOptions");
            }
        }
    }
    
}