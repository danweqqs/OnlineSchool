using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Models;

namespace OnlineSchool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly SchoolContext _context;

        public StudentsController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetAll()
        {
            return await _context.Students.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetById(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound(new { message = $"Student with id {id} not found" });
            return student;
        }

        [HttpGet("{id}/schedule")]
        public async Task<ActionResult<IEnumerable<Lesson>>> GetSchedule(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound(new { message = $"Student with id {id} not found" });

            var lessons = await _context.Lessons
                .Where(l => l.StudentId == id)
                .Include(l => l.Subject)
                .Include(l => l.Teacher)
                .OrderBy(l => l.Date).ThenBy(l => l.Time)
                .ToListAsync();

            return Ok(lessons);
        }

        [HttpPost]
        public async Task<ActionResult<Student>> Create(Student student)
        {
            if (string.IsNullOrWhiteSpace(student.FullName))
                return BadRequest(new { message = "Student name is required" });

            if (!string.IsNullOrEmpty(student.Email))
            {
                var emailExists = await _context.Students.AnyAsync(s => s.Email == student.Email);
                if (emailExists)
                    return Conflict(new { message = $"Student with email '{student.Email}' already exists" });
            }

            student.DateRegistered = DateTime.UtcNow;

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Student student)
        {
            if (id != student.Id)
                return BadRequest(new { message = "Id mismatch" });

            if (!string.IsNullOrEmpty(student.Email))
            {
                var duplicate = await _context.Students
                    .AnyAsync(s => s.Email == student.Email && s.Id != id);
                if (duplicate)
                    return Conflict(new { message = $"Email '{student.Email}' is already taken" });
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Students.AnyAsync(s => s.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound(new { message = $"Student with id {id} not found" });

            var hasLessons = await _context.Lessons.AnyAsync(l => l.StudentId == id);
            if (hasLessons)
                return Conflict(new { message = "Cannot delete student who has lessons. Delete lessons first." });

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
