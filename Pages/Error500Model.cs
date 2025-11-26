// SharedNavigation/Pages/Error500Model.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;

namespace SharedNavigation.Pages
{
    /// <summary>
    /// Page Model สำหรับ Server Error (HTTP 500)
    /// ใช้แสดงหน้าข้อผิดพลาดเมื่อเกิด Exception ภายในระบบ
    /// </summary>
    public class Error500Model : PageModel
    {
        private readonly ILogger<Error500Model> _logger;

        public Error500Model(ILogger<Error500Model> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Error ID สำหรับ Reference
        /// </summary>
        public string ErrorId { get; set; } = "";

        /// <summary>
        /// Error Message ที่แสดงให้ผู้ใช้เห็น (User-friendly)
        /// </summary>
        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// รายละเอียดข้อผิดพลาดเพิ่มเติม (Technical details)
        /// </summary>
        public string ErrorDetails { get; set; } = "";

        /// <summary>
        /// Stack Trace (แสดงเฉพาะ Development)
        /// </summary>
        public string StackTrace { get; set; } = "";

        /// <summary>
        /// เวลาที่เกิดข้อผิดพลาด
        /// </summary>
        public DateTime ErrorTime { get; set; }

        /// <summary>
        /// Request Path ที่เกิดข้อผิดพลาด
        /// </summary>
        public string RequestPath { get; set; } = "";

        /// <summary>
        /// กำหนดว่าจะแสดง Stack Trace หรือไม่
        /// </summary>
        public bool ShowStackTrace { get; set; }

        /// <summary>
        /// กำหนดว่าจะแสดง Technical Details หรือไม่
        /// </summary>
        public bool ShowDetails { get; set; }

        // TempData สำหรับรับค่าจาก Controller
        [TempData]
        public string? TempErrorMessage { get; set; }

        [TempData]
        public string? TempErrorDetails { get; set; }

        [TempData]
        public string? TempErrorStackTrace { get; set; }

        /// <summary>
        /// OnGet - เรียกเมื่อมีการ Redirect มาที่หน้านี้
        /// </summary>
        /// <param name="message">Error message จาก Query String</param>
        /// <param name="stackTrace">Stack trace จาก Query String</param>
        public void OnGet(string? message = null, string? stackTrace = null)
        {
            // สร้าง Error ID สำหรับ Reference
            ErrorId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            
            // ดึงข้อมูลจาก TempData ก่อน ถ้าไม่มีค่อยใช้จาก Query String
            ErrorMessage = TempErrorMessage ?? message ?? "An unknown error has occurred.";
            ErrorDetails = TempErrorDetails ?? "";
            StackTrace = TempErrorStackTrace ?? stackTrace ?? "";
            
            // เก็บข้อมูล Request
            RequestPath = HttpContext.Request.Path;
            ErrorTime = DateTime.Now;
            
            // ตรวจสอบ Environment
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            ShowStackTrace = environment == "Development";
            ShowDetails = environment == "Development" || environment == "Staging";
            
            // Log error สำหรับ monitoring
            _logger.LogError(
                "Error 500 displayed. ID: {ErrorId}, Path: {Path}, Message: {Message}",
                ErrorId,
                RequestPath,
                ErrorMessage
            );
        }

        /// <summary>
        /// OnPost - ถ้ามีการ POST มาที่หน้านี้ (เช่นจาก Form)
        /// </summary>
        public void OnPost()
        {
            OnGet();
        }
    }
}