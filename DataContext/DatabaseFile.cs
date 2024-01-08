using Microsoft.EntityFrameworkCore;
using RMS_Management_System.Models;
using RMS_Management_System.Migrations.Models;

namespace RMS_Management_System.DataContext
{
    public class DatabaseFile:DbContext
    {
        public DatabaseFile(DbContextOptions<DatabaseFile> opt):base(opt)
        {
            
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }  
        public DbSet<RMS_Management_System.Migrations.Models.StudentViewEdit> StudentViewEdit { get; set; } = default!;
    }
}
