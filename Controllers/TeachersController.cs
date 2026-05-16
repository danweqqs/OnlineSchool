using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Models;

namespace OnlineSchool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeachersController : ControllerBase
    {
        private readonly SchoolContext _context;

        public TeachersController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Teacher>>> GetAll()
        {
            return await _context.Teachers.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Teacher>> GetById(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound(new { message = $"Teacher with id {id} not found" });
            return teacher;
        }

        [HttpGet("{id}/schedule")]
        public async Task<ActionResult<IEnumerable<Lesson>>> GetSchedule(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound(new { message = $"Teacher with id {id} not found" });

            var lessons = await _context.Lessons
                .Where(l => l.TeacherId == id)
                .Include(l => l.Subject)
                .Include(l => l.Student)
                .OrderBy(l => l.Date).ThenBy(l => l.Time)
                .ToListAsync();

            return Ok(lessons);
        }

        [HttpGet("{id}/students")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound(new { message = $"Teacher with id {id} not found" });

            var studentIds = await _context.Lessons
                .Where(l => l.TeacherId == id)
                .Select(l => l.StudentId)
                .Distinct()
                .ToListAsync();

            var students = await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .ToListAsync();

            return Ok(students);
        }

        [HttpPost]
        public async Task<ActionResult<Teacher>> Create(Teacher teacher)
        {
            if (string.IsNullOrWhiteSpace(teacher.FullName))
                return BadRequest(new { message = "Teacher name is required" });

            if (!string.IsNullOrEmpty(teacher.Email))
            {
                var emailExists = await _context.Teachers.AnyAsync(t => t.Email == teacher.Email);
                if (emailExists)
                    return Conflict(new { message = $"Teacher with email '{teacher.Email}' already exists" });
            }

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = teacher.Id }, teacher);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Teacher teacher)
        {
            if (id != teacher.Id)
                return BadRequest(new { message = "Id mismatch" });

            if (!string.IsNullOrEmpty(teacher.Email))
            {
                var duplicate = await _context.Teachers
                    .AnyAsync(t => t.Email == teacher.Email && t.Id != id);
                if (duplicate)
                    return Conflict(new { message = $"Email '{teacher.Email}' is already taken" });
            }

            _context.Entry(teacher).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Teachers.AnyAsync(t => t.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound(new { message = $"Teacher with id {id} not found" });

            var hasLessons = await _context.Lessons.AnyAsync(l => l.TeacherId == id);
            if (hasLessons)
                return Conflict(new { message = "Cannot delete teacher who has lessons. Delete or reassign lessons first." });

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
