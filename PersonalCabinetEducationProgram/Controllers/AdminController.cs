using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PersonalCabinetEducationProgram.Data;
using PersonalCabinetEducationProgram.Models;
using PersonalCabinetEducationProgram.ViewModels;

namespace PersonalCabinetEducationProgram.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
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

        public async Task<IActionResult> Departments()
        {
            var departments = await _context.Departments.ToListAsync();
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
