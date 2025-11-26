namespace SharedNavigation.Models
{
    public class PlantModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
    public enum PlantCode
    {
        AmaP, HmjP
    }
    public class Plant
    {
        public PlantCode Code { get; set; }
        public string PlantCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longtitude { get; set; }
        public bool IsDefault { get; set; }
    }
    public class PlantSelectionResult
    {
        public PlantCode SelectedPlant { get; set; }
        public bool IsChanged { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class PlantCookieData
    {
        public PlantCode CurrentPlant { get; set; }
        public PlantCode DefaultPlant { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string ObjectId { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class PlantSelectorModel
    {
        public List<Plant> AvailablePlants { get; set; } = new();
        public PlantCode CurrentPlant { get; set; }
        public PlantCode DefaultPlant { get; set; }
        public string UserObjectId { get; set; } = string.Empty;
        public bool ShowDefaultInfo { get; set; } = true;
        public string CssClass { get; set; } = string.Empty;
        public string ContainerId { get; set; } = "plantSelector";
    }
    // เพิ่ม Request/Response Models สำหรับ API
    public class PlantChangeRequest
    {
        public string PlantCode { get; set; } = string.Empty;
        public long Timestamp { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    public class PlantDataRequest
    {
        public string PlantId { get; set; } = string.Empty;
        public long Timestamp { get; set; }
        public string CurrentUrl { get; set; } = string.Empty;
        public string Referrer { get; set; } = string.Empty;
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    public class PlantResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Plant> Data { get; set; } = new();
    }
     public class PlantSelectorOptions
    {
        public bool AutoReload { get; set; } = true;
        public bool ShowClearOption { get; set; } = false;
        public string PlaceholderText { get; set; } = "Select Plant";
        public string ButtonClass { get; set; } = "btn-outline-secondary";
        public bool ShowPlantCode { get; set; } = true;
        public int MaxDropdownHeight { get; set; } = 300;
    }

}