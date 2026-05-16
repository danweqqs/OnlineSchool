using System.ComponentModel.DataAnnotations;

namespace OnlineSchool.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Time { get; set; }

        public int DurationMinutes { get; set; } = 50;

        [MaxLength(50)]
        public string Status { get; set; } = "Scheduled";

        public int SubjectId { get; set; }
        public virtual Subject? Subject { get; set; }

        public int TeacherId { get; set; }
        public virtual Teacher? Teacher { get; set; }

        public int StudentId { get; set; }
        public virtual Student? Student { get; set; }
    }
}