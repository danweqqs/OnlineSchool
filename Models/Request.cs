using System.ComponentModel.DataAnnotations;

namespace OnlineSchool.Models
{
    public class Request
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public int TeacherId { get; set; }
        public virtual Teacher? Teacher { get; set; }
    }
}
