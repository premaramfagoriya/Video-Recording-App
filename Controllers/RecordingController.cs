using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RecordingApp.Models;
using RecordingApp.Data;

namespace RecordingApp.Controllers
{
    public class RecordingController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public RecordingController(IWebHostEnvironment environment, ApplicationDbContext context)
        {
            _environment = environment;
            _context = context;
        }

        public IActionResult Index()
        {
            // Serve the static HTML file from wwwroot
            var filePath = Path.Combine(_environment.WebRootPath, "recorder.html");
            return PhysicalFile(filePath, "text/html");
        }

        [HttpPost]
        public async Task<IActionResult> UploadRecording()
        {
            try
            {
                Console.WriteLine("UploadRecording endpoint hit");

                if (!Request.HasFormContentType)
                {
                    return BadRequest("Expected form data");
                }

                var userName = Request.Form["userName"].FirstOrDefault();
                var userEmail = Request.Form["userEmail"].FirstOrDefault();
                var durationValue = Request.Form["duration"].FirstOrDefault();

                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(durationValue))
                {
                    return BadRequest("Missing required form fields");
                }

                if (!int.TryParse(durationValue, out int duration))
                {
                    return BadRequest("Invalid duration value");
                }

                var file = Request.Form.Files["videoFile"];
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded or file is empty");
                }

                // Ensure recordings folder exists
                var uploadsDir = Path.Combine(_environment.WebRootPath, "recordings");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                // Generate unique filename
                var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{userName.Replace(" ", "_")}.webm";

                // Build relative + absolute paths
                var relativePath = Path.Combine("recordings", fileName);
                var filePath = Path.Combine(_environment.WebRootPath, relativePath);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Save DB record with relative path for web use
                var recording = new UserRecording
                {
                    UserName = userName,
                    UserEmail = userEmail,
                    FilePath = "/" + relativePath.Replace("\\", "/"),
                    RecordingDate = DateTime.Now,
                    FileSize = file.Length,
                    Duration = duration
                };

                _context.UserRecordings.Add(recording);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Recording saved successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UploadRecording: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult Test()
        {
            return Ok(new { message = "RecordingController is working!" });
        }

        public IActionResult Admin()
        {
            var recordings = _context.UserRecordings
                .OrderByDescending(r => r.RecordingDate)
                .ToList();

            return View(recordings);
        }
    }
}
