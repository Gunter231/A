using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalCabinetEducationProgram.Data;
using PersonalCabinetEducationProgram.Models;

namespace PersonalCabinetEducationProgram.Controllers
{
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
                .Include(p => p.Assignments).ThenInclude(a => a.Department)
                .Include(p => p.Assignments).ThenInclude(a => a.Faculty)
                .ToListAsync();

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Facultys = await _context.Facultys.ToListAsync();

            return View(programs);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProgram(string codeReferral, string name, string educationalLevel,
            int yearApprovals, int departmentId, int facultyId)
        {
            var program = new EducationalProgram
            {
                CodeReferral = codeReferral,
                Name = name,
                EducationalLevel = educationalLevel,
                YearApprovals = yearApprovals,
                Status = "Разрабатывается",
                UserId = 1
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
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Programs));
        }

        public async Task<IActionResult> Departments()
        {
            var departments = await _context.Departments.ToListAsync();
            return View(departments);
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
