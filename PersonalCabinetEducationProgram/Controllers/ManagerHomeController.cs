using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonalCabinetEducationProgram.Data;
using PersonalCabinetEducationProgram.Models;
using PersonalCabinetEducationProgram.Services;

namespace PersonalCabinetEducationProgram.Controllers
{
    [Authorize(Roles = "Manager,Admin")]
    public class ManagerHomeController : Controller
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly FileStorageSettings _storageSettings;
        private readonly ApplicationDbContext _context;

        public ManagerHomeController(
            IFileStorageService fileStorageService,
            IOptions<FileStorageSettings> storageSettings,
            ApplicationDbContext context)
        {
            _fileStorageService = fileStorageService;
            _storageSettings = storageSettings.Value;
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        }

        public async Task<IActionResult> Index(int? programId, string tab = "disciplines")
        {
            var currentUserId = GetCurrentUserId();

            var programs = await _context.EducationalPrograms
                .Where(p => User.IsInRole("Admin") || p.UserId == currentUserId || p.Managers.Any(m => m.UserId == currentUserId))
                .Include(p => p.Assignments).ThenInclude(a => a.Department)
                .Include(p => p.Assignments).ThenInclude(a => a.Faculty)
                .Include(p => p.Managers)
                .ToListAsync();

            int? selectedProgramId = programId ?? programs.FirstOrDefault()?.Id;

            var elements = await _context.EducationalProgramElements
                .Where(e => e.EducationalProgramId == selectedProgramId)
                .Include(e => e.EducationalProgram)
                .ToListAsync();

            var comments = await _context.EducationalProgramElementComment
                .Include(c => c.User)
                .ToListAsync();

            ViewBag.Programs = programs;
            ViewBag.SelectedProgramId = selectedProgramId;
            ViewBag.ActiveTab = tab;
            ViewBag.Comments = comments;

            return View(elements);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(int elementId, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var element = await _context.EducationalProgramElements.FindAsync(elementId);
                if (element != null && element.StatusApprovals != "Согласовано" && element.StatusApprovals != "Опубликовано на сайте")
                {
                    string oldStatus = element.StatusApprovals;
                    string uniqueFileName = await _fileStorageService.SaveFileAsync(file);
                    element.FilePath = uniqueFileName;
                    element.FileName = file.FileName;
                    element.UploadDate = DateOnly.FromDateTime(DateTime.Now);
                    element.StatusApprovals = "Загружено";

                    _context.ElementStatusHistory.Add(new ElementStatusHistory
                    {
                        EducationalProgramElementId = elementId,
                        UserId = GetCurrentUserId(),
                        OldStatus = oldStatus,
                        NewStatus = "Загружено",
                        ChangeDate = DateTime.Now,
                        Comment = $"Загружен файл: {file.FileName}"
                    });

                    await _context.SaveChangesAsync();
                }
            }

            int progId = (await _context.EducationalProgramElements.FindAsync(elementId))?.EducationalProgramId ?? 1;
            return RedirectToAction(nameof(Index), new { programId = progId });
        }

        public async Task<IActionResult> Download(int elementId)
        {
            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            if (element == null || string.IsNullOrEmpty(element.FilePath))
                return NotFound();

            string filePath = Path.Combine(_storageSettings.StoragePath, element.FilePath);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", element.FileName ?? "download");
        }

        public async Task<IActionResult> Preview(int elementId)
        {
            var element = await _context.EducationalProgramElements
                .Include(e => e.EducationalProgram)
                .FirstOrDefaultAsync(e => e.Id == elementId);

            if (element == null || string.IsNullOrEmpty(element.FilePath))
                return NotFound();

            string filePath = Path.Combine(_storageSettings.StoragePath, element.FilePath);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            Response.Headers.Append("Content-Disposition", $"inline; filename=\"{element.FileName ?? "preview.pdf"}\"");
            return File(fileBytes, "application/pdf");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int elementId, string newStatus, string comment)
        {
            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            if (element == null)
                return NotFound();

            if (element.StatusApprovals == "Согласовано" || element.StatusApprovals == "Опубликовано на сайте")
                return BadRequest("Нельзя изменить статус согласованного или опубликованного элемента");

            string oldStatus = element.StatusApprovals;
            element.StatusApprovals = newStatus;

            _context.ElementStatusHistory.Add(new ElementStatusHistory
            {
                EducationalProgramElementId = elementId,
                UserId = GetCurrentUserId(),
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangeDate = DateTime.Now,
                Comment = comment ?? ""
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { programId = element.EducationalProgramId });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int elementId, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
                return RedirectToAction(nameof(Index));

            var comment = new EducationalProgramElementComment
            {
                EducationalProgramElementId = elementId,
                UserId = GetCurrentUserId(),
                DateTimeComment = DateTime.Now,
                CommentContent = commentText,
                Status = "Новый"
            };

            _context.EducationalProgramElementComment.Add(comment);
            await _context.SaveChangesAsync();

            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            return RedirectToAction(nameof(Index), new { programId = element?.EducationalProgramId ?? 1 });
        }

        public async Task<IActionResult> History(int elementId)
        {
            var element = await _context.EducationalProgramElements
                .Include(e => e.EducationalProgram)
                .FirstOrDefaultAsync(e => e.Id == elementId);

            var history = await _context.ElementStatusHistory
                .Where(h => h.EducationalProgramElementId == elementId)
                .Include(h => h.User)
                .OrderByDescending(h => h.ChangeDate)
                .ToListAsync();

            var comments = await _context.EducationalProgramElementComment
                .Where(c => c.EducationalProgramElementId == elementId)
                .Include(c => c.User)
                .OrderByDescending(c => c.DateTimeComment)
                .ToListAsync();

            ViewBag.Element = element;
            ViewBag.History = history;
            ViewBag.Comments = comments;
            ViewBag.ReturnController = nameof(ManagerHomeController).Replace("Controller", "");

            return View();
        }

        public async Task<IActionResult> Comments(int elementId)
        {
            var element = await _context.EducationalProgramElements
                .Include(e => e.EducationalProgram)
                .FirstOrDefaultAsync(e => e.Id == elementId);

            var comments = await _context.EducationalProgramElementComment
                .Where(c => c.EducationalProgramElementId == elementId)
                .Include(c => c.User)
                .OrderByDescending(c => c.DateTimeComment)
                .ToListAsync();

            ViewBag.Element = element;
            ViewBag.ReturnController = nameof(ManagerHomeController).Replace("Controller", "");
            return View(comments);
        }
    }
}
