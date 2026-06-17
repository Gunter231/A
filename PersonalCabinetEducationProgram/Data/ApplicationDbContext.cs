using Microsoft.EntityFrameworkCore;
using PersonalCabinetEducationProgram.Models;

namespace PersonalCabinetEducationProgram.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<EducationalProgram> EducationalPrograms { get; set; }
        public DbSet<EducationalProgramElement> EducationalProgramElements { get; set; }
        public DbSet<EducationalProgramElementComment> EducationalProgramElementComment { get; set; }
        public DbSet<EducationalProgramManager> EducationalProgramManagers { get; set; }
        public DbSet<Departments> Departments { get; set; }
        public DbSet<Facultys> Facultys { get; set; }
        public DbSet<EducationalProgramAssignment> EducationalProgramAssignments { get; set; }
        public DbSet<ElementStatusHistory> ElementStatusHistory { get; set; }
        public DbSet<ApproverAssignment> ApproverAssignments { get; set; }

        public ApplicationDbContext() { }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApproverAssignment>(entity =>
            {
                entity.HasOne(a => a.ApproverUser)
                    .WithMany(u => u.ApproverAssignments)
                    .HasForeignKey(a => a.ApproverUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.AssignedByUser)
                    .WithMany()
                    .HasForeignKey(a => a.AssignedByUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Manager", Description = "Руководитель ОПОП" },
                new Role { Id = 2, Name = "Approver", Description = "Согласующий" },
                new Role { Id = 3, Name = "Moderator", Description = "Модератор" },
                new Role { Id = 4, Name = "Admin", Description = "Администратор" }
            );

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "manager", PasswordHash = "866485796cfa8d7c0cf7111640205b83076433547577511d81f8030ae99ecea5", FullName = "Иванов Иван Иванович", LinkRole = "Manager", Post = "Заведующий кафедрой", ApprovalStatus = "Approved" },
                new User { Id = 2, Username = "approver", PasswordHash = "1c391319644c0c6e9f5955e44e55862a8fd27b3b9d9863456500096ccf512db3", FullName = "Петрова Анна Сергеевна", LinkRole = "Approver", Post = "Декан факультета", ApprovalStatus = "Approved" },
                new User { Id = 3, Username = "moderator", PasswordHash = "4c8425b174053ea6935b29c2b0e0aa4e2eab1a01b784e6ac91b8bdce9c26235a", FullName = "Сидоров Петр Алексеевич", LinkRole = "Moderator", Post = "Модератор", ApprovalStatus = "Approved" },
                new User { Id = 4, Username = "admin", PasswordHash = "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9", FullName = "Козлова Мария Ивановна", LinkRole = "Admin", Post = "Администратор", ApprovalStatus = "Approved" }
            );

            // Seed Faculties
            modelBuilder.Entity<Facultys>().HasData(
                new Facultys { Id = 1, Name = "Факультет информационных технологий" },
                new Facultys { Id = 2, Name = "Факультет математики и механики" },
                new Facultys { Id = 3, Name = "Факультет педагогического образования" }
            );

            // Seed Departments
            modelBuilder.Entity<Departments>().HasData(
                new Departments { Id = 1, CodeDepartment = "Каф.ПМИ", Name = "Кафедра прикладной математики и информатики" },
                new Departments { Id = 2, CodeDepartment = "Каф.ИВТ", Name = "Кафедра информационных вычислительных технологий" },
                new Departments { Id = 3, CodeDepartment = "Каф.МАТЕМ", Name = "Кафедра математического анализа" }
            );

            // Seed Educational Programs
            modelBuilder.Entity<EducationalProgram>().HasData(
                new EducationalProgram
                {
                    Id = 1,
                    CodeReferral = "01.03.02",
                    Name = "Прикладная математика и информатика",
                    EducationalLevel = "Бакалавриат",
                    Status = "Разрабатывается",
                    UserId = 1
                },
                new EducationalProgram
                {
                    Id = 2,
                    CodeReferral = "09.03.01",
                    Name = "Информатика и вычислительная техника",
                    EducationalLevel = "Бакалавриат",
                    Status = "Разрабатывается",
                    UserId = 1
                },
                new EducationalProgram
                {
                    Id = 3,
                    CodeReferral = "44.03.05",
                    Name = "Педагогическое образование (Математика. Информатика)",
                    EducationalLevel = "Бакалавриат",
                    Status = "Разрабатывается",
                    UserId = 1
                }
            );

            // Seed Managers
            modelBuilder.Entity<EducationalProgramManager>().HasData(
                new EducationalProgramManager { Id = 1, EducationalProgramId = 1, UserId = 1 },
                new EducationalProgramManager { Id = 2, EducationalProgramId = 2, UserId = 1 },
                new EducationalProgramManager { Id = 3, EducationalProgramId = 3, UserId = 1 }
            );

            // Seed Assignments
            modelBuilder.Entity<EducationalProgramAssignment>().HasData(
                new EducationalProgramAssignment { Id = 1, EducationalProgramId = 1, DepartmentId = 1, FacultyId = 1 },
                new EducationalProgramAssignment { Id = 2, EducationalProgramId = 2, DepartmentId = 2, FacultyId = 1 },
                new EducationalProgramAssignment { Id = 3, EducationalProgramId = 3, DepartmentId = 1, FacultyId = 3 }
            );

            // Seed Elements
            modelBuilder.Entity<EducationalProgramElement>().HasData(
                new EducationalProgramElement
                {
                    Id = 1,
                    EducationalProgramId = 1,
                    TypeElement = "Main",
                    Name = "Учебный план (очный)",
                    Description = "Основной учебный план",
                    StatusApprovals = ""
                },
                new EducationalProgramElement
                {
                    Id = 2,
                    EducationalProgramId = 1,
                    TypeElement = "Main",
                    Name = "Пояснительная записка",
                    Description = "Общая информация",
                    StatusApprovals = "На доработку"
                },
                new EducationalProgramElement
                {
                    Id = 3,
                    EducationalProgramId = 1,
                    TypeElement = "Main",
                    Name = "Календарный учебный график",
                    Description = "График обучения",
                    StatusApprovals = ""
                },
                new EducationalProgramElement
                {
                    Id = 4,
                    EducationalProgramId = 1,
                    TypeElement = "Main",
                    Name = "Программа воспитательной работы",
                    Description = "Воспитательная программа",
                    StatusApprovals = ""
                },
                new EducationalProgramElement
                {
                    Id = 5,
                    EducationalProgramId = 1,
                    TypeElement = "Main",
                    Name = "Календарный план воспитательной работы",
                    Description = "Календарный план",
                    StatusApprovals = ""
                },
                new EducationalProgramElement
                {
                    Id = 6,
                    EducationalProgramId = 1,
                    TypeElement = "Discipline",
                    Name = "Философия",
                    Description = "Б1.О.01",
                    StatusApprovals = "Согласовано"
                },
                new EducationalProgramElement
                {
                    Id = 7,
                    EducationalProgramId = 1,
                    TypeElement = "Discipline",
                    Name = "Математический анализ",
                    Description = "Б1.О.02",
                    StatusApprovals = "На рассмотрении"
                },
                new EducationalProgramElement
                {
                    Id = 8,
                    EducationalProgramId = 1,
                    TypeElement = "Discipline",
                    Name = "Линейная алгебра",
                    Description = "Б1.О.03",
                    StatusApprovals = ""
                },
                new EducationalProgramElement
                {
                    Id = 9,
                    EducationalProgramId = 1,
                    TypeElement = "Discipline",
                    Name = "Программирование",
                    Description = "Б1.О.04",
                    StatusApprovals = "Согласовано"
                },
                new EducationalProgramElement
                {
                    Id = 10,
                    EducationalProgramId = 1,
                    TypeElement = "Discipline",
                    Name = "Базы данных",
                    Description = "Б1.О.05",
                    StatusApprovals = ""
                },
                new EducationalProgramElement
                {
                    Id = 11,
                    EducationalProgramId = 1,
                    TypeElement = "Practice",
                    Name = "Учебная практика",
                    Description = "Практика 1",
                    StatusApprovals = ""
                },
                new EducationalProgramElement
                {
                    Id = 12,
                    EducationalProgramId = 1,
                    TypeElement = "Practice",
                    Name = "Производственная практика",
                    Description = "Практика 2",
                    StatusApprovals = ""
                },
                new EducationalProgramElement
                {
                    Id = 13,
                    EducationalProgramId = 1,
                    TypeElement = "GIA",
                    Name = "Государственный экзамен",
                    Description = "ГИА",
                    StatusApprovals = ""
                },
                new EducationalProgramElement
                {
                    Id = 14,
                    EducationalProgramId = 1,
                    TypeElement = "GIA",
                    Name = "Выпускная квалификационная работа",
                    Description = "ВКР",
                    StatusApprovals = ""
                }
            );
        }
    }
}
