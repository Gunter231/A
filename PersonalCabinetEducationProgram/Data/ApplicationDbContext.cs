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
    }
}
