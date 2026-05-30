using Microsoft.EntityFrameworkCore;
using PersonalCabinetEducationProgram.Models;
using System.Numerics;

namespace PersonalCabinetEducationProgram.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<EducationalProgram> EducationalPrograms { get; set; }
        public DbSet<EducationalProgramElement> EducationalProgramElements { get; set; }
        public DbSet<EducationalProgramElementComment> EducationalProgramElementComment { get; set; }

        public ApplicationDbContext() { }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed User
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                FullName = "Иванов Иван Иванович",
                LinkRole = "Manager",
                Post = "Заведующий кафедрой"
            });

            // Seed Educational Program
            modelBuilder.Entity<EducationalProgram>().HasData(new EducationalProgram
            {
                Id = 1,
                CodeReferral = "01.03.02",
                Name = "Прикладная математика и информатика",
                EducationalLevel = "Бакалавриат",
                Status = "Активна",
                UserId = 1
            });

            // Seed Elements
            modelBuilder.Entity<EducationalProgramElement>().HasData(
                new EducationalProgramElement
                {
                    Id = 1,
                    EducationalProgramId = 1,
                    TypeElement = "Main",
                    Name = "Учебный план (очный)",
                    Description = "Основной учебный план",
                    StatusApprovals = "" // Не загружено
                },
                new EducationalProgramElement
                {
                    Id = 2,
                    EducationalProgramId = 1,
                    TypeElement = "Main",
                    Name = "Пояснительная записка",
                    Description = "Общая информация",
                    StatusApprovals = "Отклонено"
                },
                new EducationalProgramElement
                {
                    Id = 3,
                    EducationalProgramId = 1,
                    TypeElement = "Discipline",
                    Name = "Философия",
                    Description = "Б1.О.01",
                    StatusApprovals = "Принято"
                }
            );
        }
    }
}
