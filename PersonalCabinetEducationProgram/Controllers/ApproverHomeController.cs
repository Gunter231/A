using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonalCabinetEducationProgram.Data;
using PersonalCabinetEducationProgram.Models;
using PersonalCabinetEducationProgram.Services;

namespace PersonalCabinetEducationProgram.Controllers
{
    [Authorize(Roles = "Approver,Admin")]
    public class ApproverHomeController : Controller
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly FileStorageSettings _storageSettings;
        private readonly ApplicationDbContext _context;

        public ApproverHomeController(
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
            var programs = await _context.EducationalPrograms
                .Include(p => p.Assignments).ThenInclude(a => a.Department)
                .Include(p => p.Assignments).ThenInclude(a => a.Faculty)
                .ToListAsync();

            int? selectedProgramId = programId ?? programs.FirstOrDefault()?.Id;

            var currentUserId = GetCurrentUserId();
            var approverAssignments = await _context.ApproverAssignments
                .Where(a => a.ApproverUserId == currentUserId)
                .ToListAsync();

            programs = programs.Where(p =>
                approverAssignments.Any(a => p.Assignments.Any(pa =>
                    (a.FacultyId != null && pa.FacultyId == a.FacultyId) ||
                    (a.DepartmentId != null && pa.DepartmentId == a.DepartmentId))))
                .ToList();

            selectedProgramId ??= programs.FirstOrDefault()?.Id;

            var elements = selectedProgramId == null
                ? new List<EducationalProgramElement>()
                : await _context.EducationalProgramElements
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
        public async Task<IActionResult> Approve(int elementId, string comment)
        {
            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            if (element == null)
                return NotFound();

            if (element.StatusApprovals != "Загружено" && element.StatusApprovals != "На рассмотрении" && element.StatusApprovals != "На доработку")
                return BadRequest("Элемент не может быть согласован в текущем статусе");

            string oldStatus = element.StatusApprovals;
            element.StatusApprovals = "Согласовано";

            _context.ElementStatusHistory.Add(new ElementStatusHistory
            {
                EducationalProgramElementId = elementId,
                UserId = GetCurrentUserId(),
                OldStatus = oldStatus,
                NewStatus = "Согласовано",
                ChangeDate = DateTime.Now,
                Comment = comment ?? "Согласовано"
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { programId = element.EducationalProgramId });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int elementId, string comment)
        {
            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            if (element == null)
                return NotFound();

            if (element.StatusApprovals != "Загружено" && element.StatusApprovals != "На рассмотрении")
                return BadRequest("Элемент не может быть отклонён в текущем статусе");

            string oldStatus = element.StatusApprovals;
            element.StatusApprovals = "На доработку";

            _context.ElementStatusHistory.Add(new ElementStatusHistory
            {
                EducationalProgramElementId = elementId,
                UserId = GetCurrentUserId(),
                OldStatus = oldStatus,
                NewStatus = "На доработку",
                ChangeDate = DateTime.Now,
                Comment = comment ?? "Отправлено на доработку"
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { programId = element.EducationalProgramId });
        }

        [HttpPost]
        public async Task<IActionResult> SendForReview(int elementId)
        {
            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            if (element == null)
                return NotFound();

            if (element.StatusApprovals != "Загружено")
                return BadRequest("Только загруженные элементы можно отправить на рассмотрение");

            string oldStatus = element.StatusApprovals;
            element.StatusApprovals = "На рассмотрении";

            _context.ElementStatusHistory.Add(new ElementStatusHistory
            {
                EducationalProgramElementId = elementId,
                UserId = GetCurrentUserId(),
                OldStatus = oldStatus,
                NewStatus = "На рассмотрении",
                ChangeDate = DateTime.Now,
                Comment = "Отправлено на рассмотрение"
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
            ViewBag.ReturnController = nameof(ApproverHomeController).Replace("Controller", "");

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
            ViewBag.ReturnController = nameof(ApproverHomeController).Replace("Controller", "");
            return View("~/Views/ManagerHome/Comments.cshtml", comments);
        }
    }
}
