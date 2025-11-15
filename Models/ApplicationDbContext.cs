using Microsoft.EntityFrameworkCore;

namespace ChurchService.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<PersonImage> PersonImages { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    }
}
