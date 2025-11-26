// SharedNavigation/Pages/ErrorModel.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace SharedNavigation.Pages
{
    /// <summary>
    /// Page Model สำหรับ Generic Error Page
    /// รองรับทุก HTTP Status Code
    /// </summary>
    public class ErrorModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// HTTP Status Code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// หัวข้อ Error
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// ข้อความอธิบาย Error
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// คำแนะนำสำหรับผู้ใช้
        /// </summary>
        public string Suggestion { get; set; } = "";

        /// <summary>
        /// Request Path ที่เกิดข้อผิดพลาด
        /// </summary>
        public string RequestPath { get; set; } = "";

        /// <summary>
        /// Request ID สำหรับ Tracking
        /// </summary>
        public string RequestId { get; set; } = "";

        /// <summary>
        /// เวลาที่เกิดข้อผิดพลาด
        /// </summary>
        public DateTime ErrorTime { get; set; }

        /// <summary>
        /// แสดง Action Buttons ตาม Status Code
        /// </summary>
        public List<ErrorAction> Actions { get; set; } = new();

        /// <summary>
        /// กำหนดว่าควรแสดง Technical Details หรือไม่
        /// </summary>
        public bool ShowTechnicalDetails { get; set; }

        // TempData
        [TempData]
        public string? TempErrorMessage { get; set; }

        [TempData]
        public string? TempErrorDetails { get; set; }

        /// <summary>
        /// OnGet - เรียกเมื่อมีการ Redirect มาที่หน้านี้
        /// </summary>
        /// <param name="statusCode">HTTP Status Code</param>
        public void OnGet(int? statusCode = null)
        {
            // ตั้งค่า Status Code
            StatusCode = statusCode ?? 500;
            
            // เก็บข้อมูล Request
            RequestPath = HttpContext.Request.Path;
            RequestId = HttpContext.TraceIdentifier ?? Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            ErrorTime = DateTime.Now;
            
            // ตรวจสอบ Environment
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            ShowTechnicalDetails = environment == "Development";
            
            // กำหนด Title, Message, Suggestion ตาม Status Code
            SetErrorContent();
            
            // กำหนด Actions ตาม Status Code
            SetErrorActions();
            
            // Log error
            _logger.LogWarning(
                "Error {StatusCode} displayed. Path: {Path}, Request ID: {RequestId}",
                StatusCode,
                RequestPath,
                RequestId
            );
        }

        /// <summary>
        /// กำหนดเนื้อหาของ Error Page ตาม Status Code
        /// </summary>
        private void SetErrorContent()
        {
            // ใช้ค่าจาก TempData ก่อนถ้ามี
            var customMessage = TempErrorMessage;
            var customDetails = TempErrorDetails;

            (Title, Message, Suggestion) = StatusCode switch
            {
                // 4xx Client Errors
                400 => (
                    "คำขอไม่ถูกต้อง",
                    customMessage ?? "ข้อมูลที่ส่งมาไม่ถูกต้อง กรุณาตรวจสอบและลองใหม่อีกครั้ง",
                    "กรุณาตรวจสอบข้อมูลที่กรอกให้ถูกต้องและครบถ้วน"
                ),
                401 => (
                    "ไม่ได้รับอนุญาต",
                    customMessage ?? "คุณต้องเข้าสู่ระบบก่อนเพื่อเข้าถึงหน้านี้",
                    "กรุณาเข้าสู่ระบบเพื่อดำเนินการต่อ"
                ),
                403 => (
                    "ไม่มีสิทธิ์เข้าถึง",
                    customMessage ?? "คุณไม่มีสิทธิ์เข้าถึงหน้านี้ หรือทำรายการนี้",
                    "หากคุณคิดว่านี่เป็นข้อผิดพลาด กรุณาติดต่อผู้ดูแลระบบ"
                ),
                404 => (
                    "ไม่พบหน้าที่ต้องการ",
                    customMessage ?? "ขออภัย หน้าที่คุณกำลังมองหาไม่มีอยู่ในระบบ อาจถูกย้ายหรือลบไปแล้ว",
                    "ลองตรวจสอบ URL หรือใช้เมนูด้านบนเพื่อค้นหาหน้าที่ต้องการ"
                ),
                405 => (
                    "Method ไม่ถูกต้อง",
                    customMessage ?? "คำขอนี้ใช้ HTTP Method ที่ไม่ถูกต้อง",
                    "กรุณาตรวจสอบวิธีการเรียกใช้งาน API"
                ),
                408 => (
                    "คำขอหมดเวลา",
                    customMessage ?? "การเชื่อมต่อหมดเวลา กรุณาลองใหม่อีกครั้ง",
                    "ตรวจสอบการเชื่อมต่ออินเทอร์เน็ตและลองอีกครั้ง"
                ),

                // 5xx Server Errors
                500 => (
                    "เกิดข้อผิดพลาดของระบบ",
                    customMessage ?? "ขออภัย เกิดข้อผิดพลาดภายในระบบ ทีมงานของเรากำลังดำเนินการแก้ไข",
                    "กรุณาลองใหม่อีกครั้งในภายหลัง หรือติดต่อทีมสนับสนุน"
                ),
                502 => (
                    "Gateway Error",
                    customMessage ?? "เกิดข้อผิดพลาดในการเชื่อมต่อกับเซิร์ฟเวอร์",
                    "กรุณารอสักครู่และลองใหม่อีกครั้ง"
                ),
                503 => (
                    "บริการไม่พร้อมใช้งาน",
                    customMessage ?? "ระบบกำลังปิดปรับปรุง หรือมีผู้ใช้งานพร้อมกันมากเกินไป",
                    "กรุณาลองใหม่อีกครั้งในภายหลัง"
                ),
                504 => (
                    "Gateway Timeout",
                    customMessage ?? "การเชื่อมต่อกับเซิร์ฟเวอร์หมดเวลา",
                    "เซิร์ฟเวอร์อาจกำลังโหลดหนัก กรุณาลองใหม่อีกครั้ง"
                ),

                // Default
                _ => (
                    "เกิดข้อผิดพลาด",
                    customMessage ?? $"เกิดข้อผิดพลาด (รหัส: {StatusCode}) กรุณาลองใหม่อีกครั้ง",
                    "หากปัญหายังคงอยู่ กรุณาติดต่อทีมสนับสนุน"
                )
            };

            // เพิ่ม Technical Details ถ้ามี
            if (!string.IsNullOrEmpty(customDetails) && ShowTechnicalDetails)
            {
                Message += $"\n\nรายละเอียดเพิ่มเติม: {customDetails}";
            }
        }

        /// <summary>
        /// กำหนด Action Buttons ตาม Status Code
        /// </summary>
        private void SetErrorActions()
        {
            Actions = new List<ErrorAction>();

            // ปุ่มกลับหน้าแรกสำหรับทุก Error
            Actions.Add(new ErrorAction
            {
                Label = "🏠 กลับหน้าแรก",
                Url = "/",
                ButtonClass = "btn-primary"
            });

            // ปุ่มเฉพาะตาม Status Code
            switch (StatusCode)
            {
                case 401:
                case 403:
                    Actions.Add(new ErrorAction
                    {
                        Label = "🔑 เข้าสู่ระบบ",
                        Url = "/Account/Login",
                        ButtonClass = "btn-secondary"
                    });
                    break;

                case 404:
                    Actions.Add(new ErrorAction
                    {
                        Label = "⬅️ ย้อนกลับ",
                        Url = "javascript:history.back()",
                        ButtonClass = "btn-secondary"
                    });
                    break;

                case 500:
                case 502:
                case 503:
                case 504:
                    Actions.Add(new ErrorAction
                    {
                        Label = "🔄 โหลดหน้าใหม่",
                        Url = "javascript:location.reload()",
                        ButtonClass = "btn-secondary"
                    });
                    Actions.Add(new ErrorAction
                    {
                        Label = "📞 ติดต่อสนับสนุน",
                        Url = "/support",
                        ButtonClass = "btn-warning"
                    });
                    break;

                default:
                    Actions.Add(new ErrorAction
                    {
                        Label = "⬅️ ย้อนกลับ",
                        Url = "javascript:history.back()",
                        ButtonClass = "btn-secondary"
                    });
                    break;
            }
        }
    }

    /// <summary>
    /// Action Button สำหรับ Error Page
    /// </summary>
    public class ErrorAction
    {
        public string Label { get; set; } = "";
        public string Url { get; set; } = "";
        public string ButtonClass { get; set; } = "btn-primary";
    }
}