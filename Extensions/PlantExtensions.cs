using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SharedNavigation.Models;
using SharedNavigation.Services;
using System.Threading.Tasks;

namespace SharedNavigation.Extensions
{
    /// <summary>
    /// Extension methods สำหรับเข้าถึง Current Plant ใน Controllers และ Views
    /// </summary>
    public static class PlantExtensions
    {
        private const string PLANT_SESSION_KEY = "SelectedPlant_Id";
        private const string PLANT_COOKIE_KEY = "SelectedPlant";

        // ========================================
        // Extension Methods สำหรับ Controller
        // ========================================

        /// <summary>
        /// Get current plant ID from Session/Cookie
        /// Usage: var plantId = this.GetCurrentPlantId();
        /// </summary>
        public static string? GetCurrentPlantId(this Controller controller)
        {
            // Try Session first
            var sessionValue = controller.HttpContext.Session.GetString(PLANT_SESSION_KEY);
            if (!string.IsNullOrEmpty(sessionValue))
                return sessionValue;

            // Try Cookie
            var cookieValue = controller.Request.Cookies[PLANT_COOKIE_KEY];
            return cookieValue;
        }

        /// <summary>
        /// Get current plant object
        /// Usage: var plant = await this.GetCurrentPlantAsync(plantService);
        /// </summary>
        public static async Task<Plant?> GetCurrentPlantAsync(
            this Controller controller, 
            IPlantService plantService)
        {
            return await plantService.GetCurrentPlantAsync();
        }

        /// <summary>
        /// Set current plant
        /// Usage: await this.SetCurrentPlantAsync("HmjP", plantService);
        /// </summary>
        public static async Task SetCurrentPlantAsync(
            this Controller controller,
            string plantCode,
            IPlantService plantService)
        {
            await plantService.SetCurrentPlantAsync(plantCode);
        }

        /// <summary>
        /// Check if specific plant is currently selected
        /// Usage: if (this.IsPlantSelected("HmjP")) { ... }
        /// </summary>
        public static bool IsPlantSelected(this Controller controller, string plantCode)
        {
            var currentPlant = controller.GetCurrentPlantId();
            return currentPlant?.Equals(plantCode, StringComparison.OrdinalIgnoreCase) == true;
        }

        /// <summary>
        /// Add current plant info to ViewData
        /// Usage: await this.AddPlantToViewDataAsync(plantService);
        /// Then in View: @ViewData["CurrentPlant"]
        /// </summary>
        public static async Task AddPlantToViewDataAsync(
            this Controller controller,
            IPlantService plantService)
        {
            var plant = await plantService.GetCurrentPlantAsync();
            controller.ViewData["CurrentPlant"] = plant;
            controller.ViewData["CurrentPlantId"] = plant?.PlantCode;
            controller.ViewData["CurrentPlantName"] = plant?.Name;
        }

        // ========================================
        // Extension Methods สำหรับ HttpContext
        // ========================================

        /// <summary>
        /// Get current plant ID from HttpContext
        /// Usage: var plantId = HttpContext.GetCurrentPlantId();
        /// </summary>
        public static string? GetCurrentPlantId(this HttpContext context)
        {
            var sessionValue = context.Session.GetString(PLANT_SESSION_KEY);
            if (!string.IsNullOrEmpty(sessionValue))
                return sessionValue;

            var cookieValue = context.Request.Cookies[PLANT_COOKIE_KEY];
            return cookieValue;
        }

        /// <summary>
        /// Get current plant object from HttpContext
        /// Usage: var plant = await HttpContext.GetCurrentPlantAsync(plantService);
        /// </summary>
        public static async Task<Plant?> GetCurrentPlantAsync(
            this HttpContext context, 
            IPlantService plantService)
        {
            return await plantService.GetCurrentPlantAsync();
        }
    }

    /// <summary>
    /// Extension Methods สำหรับใช้ใน Razor Views
    /// </summary>
    public static class PlantViewExtensions
    {
        /// <summary>
        /// Get current plant ID in View
        /// Usage: @Html.GetCurrentPlantId()
        /// </summary>
        public static string? GetCurrentPlantId(this IHtmlHelper htmlHelper)
        {
            var context = htmlHelper.ViewContext.HttpContext;
            return context.GetCurrentPlantId();
        }

        /// <summary>
        /// Check if specific plant is selected in View
        /// Usage: @if(Html.IsPlantSelected("HmjP")) { ... }
        /// </summary>
        public static bool IsPlantSelected(this IHtmlHelper htmlHelper, string plantCode)
        {
            var currentPlant = htmlHelper.GetCurrentPlantId();
            return currentPlant?.Equals(plantCode, StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}