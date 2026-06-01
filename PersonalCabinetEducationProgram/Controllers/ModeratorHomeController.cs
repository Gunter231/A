using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonalCabinetEducationProgram.Data;
using PersonalCabinetEducationProgram.Models;
using PersonalCabinetEducationProgram.Services;

namespace PersonalCabinetEducationProgram.Controllers
{
    public class ModeratorHomeController : Controller
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly FileStorageSettings _storageSettings;
        private readonly ApplicationDbContext _context;

        public ModeratorHomeController(
            IFileStorageService fileStorageService,
            IOptions<FileStorageSettings> storageSettings,
            ApplicationDbContext context)
        {
            _fileStorageService = fileStorageService;
            _storageSettings = storageSettings.Value;
            _context = context;
        }

        public async Task<IActionResult> Index(int? programId, string tab = "disciplines")
        {
            var programs = await _context.EducationalPrograms
                .Include(p => p.Assignments).ThenInclude(a => a.Department)
                .Include(p => p.Assignments).ThenInclude(a => a.Faculty)
                .ToListAsync();

            var selectedProgramId = programId ?? programs.FirstOrDefault()?.Id;

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
        public async Task<IActionResult> Publish(int elementId, string comment)
        {
            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            if (element == null)
                return NotFound();

            if (element.StatusApprovals != "Согласовано")
                return BadRequest("Только согласованные элементы могут быть опубликованы");

            string oldStatus = element.StatusApprovals;
            element.StatusApprovals = "Опубликовано на сайте";

            _context.ElementStatusHistory.Add(new ElementStatusHistory
            {
                EducationalProgramElementId = elementId,
                UserId = 3,
                OldStatus = oldStatus,
                NewStatus = "Опубликовано на сайте",
                ChangeDate = DateTime.Now,
                Comment = comment ?? "Опубликовано на сайте"
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { programId = element.EducationalProgramId });
        }

        [HttpPost]
        public async Task<IActionResult> Unpublish(int elementId, string comment)
        {
            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            if (element == null)
                return NotFound();

            if (element.StatusApprovals != "Опубликовано на сайте")
                return BadRequest("Только опубликованные элементы могут быть сняты с публикации");

            string oldStatus = element.StatusApprovals;
            element.StatusApprovals = "Согласовано";

            _context.ElementStatusHistory.Add(new ElementStatusHistory
            {
                EducationalProgramElementId = elementId,
                UserId = 3,
                OldStatus = oldStatus,
                NewStatus = "Согласовано",
                ChangeDate = DateTime.Now,
                Comment = comment ?? "Снято с публикации"
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
                UserId = 3,
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

            return View("~/Views/ManagerHome/History.cshtml");
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
            return View("~/Views/ManagerHome/Comments.cshtml", comments);
        }
    }
}
