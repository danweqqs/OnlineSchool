using System.ComponentModel.DataAnnotations;

namespace OnlineSchool.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = null!;

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        public DateTime DateRegistered { get; set; } = DateTime.Now;

        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
