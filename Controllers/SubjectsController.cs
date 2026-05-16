using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Models;

namespace OnlineSchool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly SchoolContext _context;

        public SubjectsController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Subject>>> GetAll()
        {
            return await _context.Subjects.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Subject>> GetById(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound(new { message = $"Subject with id {id} not found" });
            return subject;
        }

        [HttpGet("{id}/lessons")]
        public async Task<ActionResult<IEnumerable<Lesson>>> GetLessons(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound(new { message = $"Subject with id {id} not found" });

            var lessons = await _context.Lessons
                .Where(l => l.SubjectId == id)
                .Include(l => l.Teacher)
                .Include(l => l.Student)
                .ToListAsync();

            return Ok(lessons);
        }

        [HttpPost]
        public async Task<ActionResult<Subject>> Create(Subject subject)
        {
            var exists = await _context.Subjects.AnyAsync(s => s.Name == subject.Name);
            if (exists)
                return Conflict(new { message = $"Subject '{subject.Name}' already exists" });

            if (string.IsNullOrWhiteSpace(subject.Name))
                return BadRequest(new { message = "Subject name is required" });

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = subject.Id }, subject);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Subject subject)
        {
            if (id != subject.Id)
                return BadRequest(new { message = "Id mismatch" });

            var duplicate = await _context.Subjects
                .AnyAsync(s => s.Name == subject.Name && s.Id != id);
            if (duplicate)
                return Conflict(new { message = $"Subject '{subject.Name}' already exists" });

            _context.Entry(subject).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Subjects.AnyAsync(s => s.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound(new { message = $"Subject with id {id} not found" });

            var hasLessons = await _context.Lessons.AnyAsync(l => l.SubjectId == id);
            if (hasLessons)
                return Conflict(new { message = "Cannot delete subject that has lessons. Delete lessons first." });

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
