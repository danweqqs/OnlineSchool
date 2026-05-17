using System.ComponentModel.DataAnnotations;

namespace OnlineSchool.Models
{
    public class Teacher
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = null!;

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        public int? SubjectId { get; set; }
        public virtual Subject? Subject { get; set; }

        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
    }
}