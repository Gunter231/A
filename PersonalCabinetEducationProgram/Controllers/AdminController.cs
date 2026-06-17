using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonalCabinetEducationProgram.Data;
using PersonalCabinetEducationProgram.Models;
using PersonalCabinetEducationProgram.Services;
using PersonalCabinetEducationProgram.ViewModels;

namespace PersonalCabinetEducationProgram.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly FileStorageSettings _storageSettings;

        public AdminController(
            ApplicationDbContext context,
            IFileStorageService fileStorageService,
            IOptions<FileStorageSettings> storageSettings)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _storageSettings = storageSettings.Value;
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeApprovalStatus(int id, string approvalStatus, string? rejectionReason)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == id && approvalStatus == "Rejected")
            {
                TempData["UsersError"] = "Нельзя отклонить собственный аккаунт.";
                return RedirectToAction(nameof(Users));
            }

            user.ApprovalStatus = approvalStatus;
            user.RejectionReason = approvalStatus == "Rejected" ? rejectionReason : null;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string fullName, string role, string post)
        {
            var user = new User
            {
                FullName = fullName,
                LinkRole = role,
                Post = post
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(int id, string fullName, string role, string post)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.FullName = fullName;
                user.LinkRole = role;
                user.Post = post;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Programs()
        {
            var programs = await _context.EducationalPrograms
                .Include(p => p.User)
                .Include(p => p.Managers).ThenInclude(m => m.User)
                .Include(p => p.Assignments).ThenInclude(a => a.Department)
                .Include(p => p.Assignments).ThenInclude(a => a.Faculty)
                .ToListAsync();

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Facultys = await _context.Facultys.ToListAsync();
            ViewBag.Managers = await _context.Users
                .Where(u => u.LinkRole == "Manager" && u.ApprovalStatus == "Approved")
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(programs);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProgramDetails(int id)
        {
            var program = await _context.EducationalPrograms
                .Include(p => p.User)
                .Include(p => p.Assignments).ThenInclude(a => a.Department)
                .Include(p => p.Assignments).ThenInclude(a => a.Faculty)
                .Include(p => p.Elements)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assignments()
        {
            var assignments = await _context.ApproverAssignments
                .Include(a => a.ApproverUser)
                .Include(a => a.AssignedByUser)
                .Include(a => a.Faculty)
                .Include(a => a.Department)
                .OrderByDescending(a => a.AssignedAt)
                .ToListAsync();

            return View(assignments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProgram(string codeReferral, string name, string educationalLevel,
            int yearApprovals, int departmentId, int facultyId, int? managerUserId)
        {
            var assignedManagerId = managerUserId ?? 1;

            var program = new EducationalProgram
            {
                CodeReferral = codeReferral,
                Name = name,
                EducationalLevel = educationalLevel,
                YearApprovals = yearApprovals,
                Status = "Разрабатывается",
                UserId = assignedManagerId
            };

            _context.EducationalPrograms.Add(program);
            await _context.SaveChangesAsync();

            var assignment = new EducationalProgramAssignment
            {
                EducationalProgramId = program.Id,
                DepartmentId = departmentId,
                FacultyId = facultyId
            };

            _context.EducationalProgramAssignments.Add(assignment);
            _context.EducationalProgramManagers.Add(new EducationalProgramManager
            {
                EducationalProgramId = program.Id,
                UserId = assignedManagerId
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Programs));
        }

        [HttpPost]
        public async Task<IActionResult> AssignProgramManager(int programId, int managerUserId)
        {
            var program = await _context.EducationalPrograms
                .Include(p => p.Managers)
                .FirstOrDefaultAsync(p => p.Id == programId);

            var manager = await _context.Users.FirstOrDefaultAsync(u => u.Id == managerUserId && u.LinkRole == "Manager" && u.ApprovalStatus == "Approved");

            if (program == null || manager == null)
            {
                return NotFound();
            }

            program.UserId = managerUserId;

            var currentAssignments = await _context.EducationalProgramManagers
                .Where(m => m.EducationalProgramId == programId)
                .ToListAsync();

            _context.EducationalProgramManagers.RemoveRange(currentAssignments);
            _context.EducationalProgramManagers.Add(new EducationalProgramManager
            {
                EducationalProgramId = programId,
                UserId = managerUserId
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Programs));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProgramElement(int programId, string typeElement, string name, string description)
        {
            var programExists = await _context.EducationalPrograms.AnyAsync(p => p.Id == programId);
            if (!programExists)
            {
                return NotFound();
            }

            var allowedTypes = new[] { "Main", "Discipline", "Practice", "GIA" };
            if (!allowedTypes.Contains(typeElement))
            {
                return BadRequest("Неизвестный тип элемента ОПОП");
            }

            _context.EducationalProgramElements.Add(new EducationalProgramElement
            {
                EducationalProgramId = programId,
                TypeElement = typeElement,
                Name = name,
                Description = description ?? string.Empty,
                StatusApprovals = string.Empty
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ProgramDetails), new { id = programId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadElement(int elementId, IFormFile file)
        {
            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            if (element == null)
            {
                return NotFound();
            }

            if (file != null && file.Length > 0 && string.IsNullOrEmpty(element.FilePath))
            {
                var oldStatus = element.StatusApprovals;
                var uniqueFileName = await _fileStorageService.SaveFileAsync(file);

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
                    Comment = $"Администратор загрузил файл: {file.FileName}"
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ProgramDetails), new { id = element.EducationalProgramId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveElement(int elementId, string? comment)
        {
            return await ChangeElementStatus(elementId, "Согласовано", comment ?? "Согласовано администратором");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendElementToRevision(int elementId, string? comment)
        {
            return await ChangeElementStatus(elementId, "На доработку", comment ?? "Отправлено на доработку администратором");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PublishElement(int elementId, string? comment)
        {
            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            if (element == null)
            {
                return NotFound();
            }

            if (element.StatusApprovals != "Согласовано")
            {
                return BadRequest("Опубликовать можно только согласованный элемент");
            }

            return await ChangeElementStatus(elementId, "Опубликовано на сайте", comment ?? "Опубликовано администратором");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadElement(int elementId)
        {
            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            if (element == null || string.IsNullOrEmpty(element.FilePath))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_storageSettings.StoragePath, element.FilePath);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", element.FileName ?? "download.pdf");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PreviewElement(int elementId)
        {
            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            if (element == null || string.IsNullOrEmpty(element.FilePath))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_storageSettings.StoragePath, element.FilePath);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            Response.Headers.Append("Content-Disposition", $"inline; filename=\"{element.FileName ?? "preview.pdf"}\"");
            return File(fileBytes, "application/pdf");
        }

        private async Task<IActionResult> ChangeElementStatus(int elementId, string newStatus, string comment)
        {
            var element = await _context.EducationalProgramElements.FindAsync(elementId);
            if (element == null)
            {
                return NotFound();
            }

            var oldStatus = element.StatusApprovals;
            element.StatusApprovals = newStatus;

            _context.ElementStatusHistory.Add(new ElementStatusHistory
            {
                EducationalProgramElementId = elementId,
                UserId = GetCurrentUserId(),
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangeDate = DateTime.Now,
                Comment = comment
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ProgramDetails), new { id = element.EducationalProgramId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignApproverToFaculty(int approverUserId, int facultyId)
        {
            return await CreateApproverAssignment(approverUserId, facultyId, null, nameof(Faculties));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignApproverToDepartment(int approverUserId, int departmentId)
        {
            return await CreateApproverAssignment(approverUserId, null, departmentId, nameof(Departments));
        }

        private async Task<IActionResult> CreateApproverAssignment(int approverUserId, int? facultyId, int? departmentId, string redirectAction)
        {
            if (facultyId == null && departmentId == null)
            {
                return BadRequest();
            }

            var approver = await _context.Users.FirstOrDefaultAsync(u => u.Id == approverUserId && u.LinkRole == "Approver" && u.ApprovalStatus == "Approved");
            if (approver == null)
            {
                return NotFound();
            }

            if (facultyId != null)
            {
                var facultyExists = await _context.Facultys.AnyAsync(f => f.Id == facultyId);
                if (!facultyExists) return NotFound();
            }

            if (departmentId != null)
            {
                var departmentExists = await _context.Departments.AnyAsync(d => d.Id == departmentId);
                if (!departmentExists) return NotFound();
            }

            var exists = await _context.ApproverAssignments.AnyAsync(a =>
                a.ApproverUserId == approverUserId && a.FacultyId == facultyId && a.DepartmentId == departmentId);

            if (!exists)
            {
                _context.ApproverAssignments.Add(new ApproverAssignment
                {
                    ApproverUserId = approverUserId,
                    FacultyId = facultyId,
                    DepartmentId = departmentId,
                    AssignedByUserId = GetCurrentUserId(),
                    AssignedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(redirectAction);
        }

        public async Task<IActionResult> Departments()
        {
            var departments = await _context.Departments.ToListAsync();
            ViewBag.Approvers = await _context.Users
                .Where(u => u.LinkRole == "Approver" && u.ApprovalStatus == "Approved")
                .OrderBy(u => u.FullName)
                .ToListAsync();
            return View(departments);
        }

        public async Task<IActionResult> DepartmentDetails(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            var programs = await _context.EducationalPrograms
                .Where(p => p.Assignments.Any(a => a.DepartmentId == id))
                .Include(p => p.Assignments).ThenInclude(a => a.Department)
                .Include(p => p.Assignments).ThenInclude(a => a.Faculty)
                .Include(p => p.Elements)
                .OrderBy(p => p.CodeReferral)
                .ToListAsync();

            var viewModel = new OrganizationDocumentsViewModel
            {
                PageTitle = "Документы кафедры",
                EntityType = "Department",
                EntityId = department.Id,
                EntityName = department.Name,
                Programs = programs
            };

            return View("OrganizationDocuments", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment(string codeDepartment, string name)
        {
            var dept = new Departments
            {
                CodeDepartment = codeDepartment,
                Name = name
            };

            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Departments));
        }

        public async Task<IActionResult> Faculties()
        {
            var faculties = await _context.Facultys.ToListAsync();
            ViewBag.Approvers = await _context.Users
                .Where(u => u.LinkRole == "Approver" && u.ApprovalStatus == "Approved")
                .OrderBy(u => u.FullName)
                .ToListAsync();
            return View(faculties);
        }

        public async Task<IActionResult> FacultyDetails(int id)
        {
            var faculty = await _context.Facultys.FindAsync(id);
            if (faculty == null)
            {
                return NotFound();
            }

            var programs = await _context.EducationalPrograms
                .Where(p => p.Assignments.Any(a => a.FacultyId == id))
                .Include(p => p.Assignments).ThenInclude(a => a.Department)
                .Include(p => p.Assignments).ThenInclude(a => a.Faculty)
                .Include(p => p.Elements)
                .OrderBy(p => p.CodeReferral)
                .ToListAsync();

            var viewModel = new OrganizationDocumentsViewModel
            {
                PageTitle = "Документы факультета",
                EntityType = "Faculty",
                EntityId = faculty.Id,
                EntityName = faculty.Name,
                Programs = programs
            };

            return View("OrganizationDocuments", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFaculty(string name)
        {
            var faculty = new Facultys { Name = name };
            _context.Facultys.Add(faculty);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Faculties));
        }
    }
}
