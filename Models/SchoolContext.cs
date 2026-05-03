using Microsoft.EntityFrameworkCore;

namespace OnlineSchool.Models
{
    public class SchoolContext : DbContext
    {
        public SchoolContext(DbContextOptions<SchoolContext> options)
            : base(options)
        {
        }

        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Request> Requests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasOne(l => l.Subject)
                    .WithMany(s => s.Lessons)
                    .HasForeignKey(l => l.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Teacher)
                    .WithMany(t => t.Lessons)
                    .HasForeignKey(l => l.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Student)
                    .WithMany(s => s.Lessons)
                    .HasForeignKey(l => l.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.HasOne(r => r.Teacher)
                    .WithMany(t => t.Requests)
                    .HasForeignKey(r => r.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
