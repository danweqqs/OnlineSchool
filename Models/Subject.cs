using System.ComponentModel.DataAnnotations;

namespace OnlineSchool.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
