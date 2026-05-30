using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonalCabinetEducationProgram.Data;
using PersonalCabinetEducationProgram.Models;
using PersonalCabinetEducationProgram.Services;

namespace PersonalCabinetEducationProgram.Controllers
{
    public class ManagerHomeController : Controller
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly FileStorageSettings _storageSettings;
        private readonly ApplicationDbContext _context;

        public ManagerHomeController(IFileStorageService fileStorageService, IOptions<FileStorageSettings> storageSettings, ApplicationDbContext context)
        {
            _fileStorageService = fileStorageService;
            _storageSettings = storageSettings.Value;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var elements = await _context.EducationalProgramElements
                .Include(e => e.EducationalProgram)
                .ToListAsync();
            return View(elements);
        }
        [HttpPost]
        public async Task<IActionResult> Upload(int elementId, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var element = await _context.EducationalProgramElements.FindAsync(elementId);
                if (element != null)
                {
                    string uniqueFileName = await _fileStorageService.SaveFileAsync(file);

                    element.FilePath = uniqueFileName;
                    element.FileName = file.FileName;
                    element.UploadDate = DateOnly.FromDateTime(DateTime.Now);
                    element.StatusApprovals = "На рассмотрении";

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Download(int elementId)
        {
            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            if (element == null || string.IsNullOrEmpty(element.FilePath))
            {
                return NotFound();
            }

            string filePath = Path.Combine(_storageSettings.StoragePath, element.FilePath);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/octet-stream", element.FileName ?? "download");
        }
    }
}
