using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedNavigation.Services;
using SharedNavigation.Models;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace SharedNavigation.Controllers
{
    /// <summary>
    /// API Controller สำหรับจัดการ Plant Selection
    /// Route: /_plantselector/*
    /// </summary>
    [Route("_plantselector")]
    [ApiController]
    public class PlantSelectorController : ControllerBase
    {
        private readonly IPlantService _plantService;
        private readonly ILogger<PlantSelectorController> _logger;

        public PlantSelectorController(
            IPlantService plantService,
            ILogger<PlantSelectorController> logger)
        {
            _plantService = plantService;
            _logger = logger;
        }

        /// <summary>
        /// GET: /_plantselector/plants
        /// ดึงรายการ Plants ทั้งหมด
        /// </summary>
        [HttpGet("plants")]
        public async Task<IActionResult> GetPlants()
        {
            try
            {
                _logger.LogInformation("Getting all plants");
                
                var plants = await _plantService.GetPlantsAsync();
                
                return Ok(new
                {
                    success = true,
                    count = plants?.Count ?? 0,
                    data = plants
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plants");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error loading plants. Please try again later."
                });
            }
        }

        /// <summary>
        /// GET: /_plantselector/current
        /// ดึง Plant ที่เลือกอยู่ปัจจุบัน
        /// </summary>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentPlant()
        {
            try
            {
                _logger.LogInformation("Getting current plant");
                
                var plant = await _plantService.GetCurrentPlantAsync();
                
                if (plant == null)
                {
                    return Ok(new
                    {
                        success = true,
                        data = (object?)null,
                        message = "No plant selected"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = plant
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current plant");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error getting current plant"
                });
            }
        }

        /// <summary>
        /// POST: /_plantselector/changeplant
        /// เปลี่ยน Plant ที่เลือก
        /// Body: { "plantCode": "HmjP" }
        /// </summary>
        [HttpPost("changeplant")]
        public async Task<IActionResult> ChangePlant([FromBody] ChangePlantRequest request)
        {
            try
            {
                // Validate request
                if (request == null || string.IsNullOrWhiteSpace(request.PlantCode))
                {
                    _logger.LogWarning("Change plant request with empty plant code");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Plant code is required"
                    });
                }

                _logger.LogInformation(
                    "Attempting to change plant to {PlantCode} for user {User}",
                    request.PlantCode,
                    User?.Identity?.Name ?? "Unknown"
                );

                // Validate plant exists
                if (!await _plantService.IsValidPlantAsync(request.PlantCode))
                {
                    _logger.LogWarning("Invalid plant code: {PlantCode}", request.PlantCode);
                    return NotFound(new
                    {
                        success = false,
                        message = $"Plant '{request.PlantCode}' not found"
                    });
                }

                // Set the plant
                await _plantService.SetCurrentPlantAsync(request.PlantCode);

                // Get updated plant info
                var plant = await _plantService.GetCurrentPlantAsync();

                _logger.LogInformation(
                    "Successfully changed plant to {PlantCode} - {PlantName} for user {User}",
                    request.PlantCode,
                    plant?.Name ?? "Unknown",
                    User?.Identity?.Name ?? "Unknown"
                );

                return Ok(new
                {
                    success = true,
                    message = "Plant changed successfully",
                    plantCode = request.PlantCode,
                    plantName = plant?.Name ?? request.PlantCode,
                    plant = plant
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while changing plant to {PlantCode}", request?.PlantCode);
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing plant to {PlantCode}", request?.PlantCode);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while changing plant. Please try again."
                });
            }
        }

        /// <summary>
        /// POST: /_plantselector/refresh
        /// Refresh plants cache (บังคับให้โหลดใหม่จาก Database)
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshPlants()
        {
            try
            {
                _logger.LogInformation("Refreshing plants cache");
                
                await _plantService.RefreshPlantsAsync();
                var plants = await _plantService.GetPlantsAsync();

                _logger.LogInformation("Plants cache refreshed successfully. Count: {Count}", plants?.Count ?? 0);

                return Ok(new
                {
                    success = true,
                    message = "Plants refreshed successfully",
                    count = plants?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing plants");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error refreshing plants. Please try again."
                });
            }
        }

        /// <summary>
        /// GET: /_plantselector/test
        /// ทดสอบว่า Controller ทำงานหรือไม่
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                success = true,
                message = "PlantSelector Controller is working!",
                timestamp = DateTime.UtcNow,
                user = User?.Identity?.Name ?? "Anonymous"
            });
        }
    }

    /// <summary>
    /// Request Model สำหรับการเปลี่ยน Plant
    /// </summary>
    public class ChangePlantRequest
    {
        public string PlantCode { get; set; } = string.Empty;
    }
}