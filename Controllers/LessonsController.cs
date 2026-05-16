using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Models;

namespace OnlineSchool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonsController : ControllerBase
    {
        private readonly SchoolContext _context;

        public LessonsController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var lessons = await _context.Lessons
                .Include(l => l.Subject)
                .Include(l => l.Teacher)
                .Include(l => l.Student)
                .OrderBy(l => l.Date).ThenBy(l => l.Time)
                .Select(l => new
                {
                    l.Id,
                    l.Date,
                    l.Time,
                    l.DurationMinutes,
                    l.Status,
                    Subject = l.Subject != null ? l.Subject.Name : null,
                    Teacher = l.Teacher != null ? l.Teacher.FullName : null,
                    Student = l.Student != null ? l.Student.FullName : null,
                    l.SubjectId,
                    l.TeacherId,
                    l.StudentId
                })
                .ToListAsync();

            return Ok(lessons);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Subject)
                .Include(l => l.Teacher)
                .Include(l => l.Student)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null) return NotFound(new { message = $"Lesson with id {id} not found" });

            return Ok(new
            {
                lesson.Id,
                lesson.Date,
                lesson.Time,
                lesson.DurationMinutes,
                lesson.Status,
                Subject = lesson.Subject?.Name,
                Teacher = lesson.Teacher?.FullName,
                Student = lesson.Student?.FullName,
                lesson.SubjectId,
                lesson.TeacherId,
                lesson.StudentId
            });
        }

        [HttpPost]
        public async Task<ActionResult<Lesson>> Create(Lesson lesson)
        {
            if (!await _context.Subjects.AnyAsync(s => s.Id == lesson.SubjectId))
                return BadRequest(new { message = $"Subject with id {lesson.SubjectId} not found" });

            if (!await _context.Teachers.AnyAsync(t => t.Id == lesson.TeacherId))
                return BadRequest(new { message = $"Teacher with id {lesson.TeacherId} not found" });

            if (!await _context.Students.AnyAsync(s => s.Id == lesson.StudentId))
                return BadRequest(new { message = $"Student with id {lesson.StudentId} not found" });

            var teacherConflict = await _context.Lessons.AnyAsync(l =>
                l.TeacherId == lesson.TeacherId &&
                l.Date == lesson.Date &&
                l.Time == lesson.Time &&
                l.Status != "Cancelled");
            if (teacherConflict)
                return Conflict(new { message = "Teacher already has a lesson at this date and time" });

            var studentConflict = await _context.Lessons.AnyAsync(l =>
                l.StudentId == lesson.StudentId &&
                l.Date == lesson.Date &&
                l.Time == lesson.Time &&
                l.Status != "Cancelled");
            if (studentConflict)
                return Conflict(new { message = "Student already has a lesson at this date and time" });

            if (string.IsNullOrEmpty(lesson.Status))
                lesson.Status = "Scheduled";

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = lesson.Id }, lesson);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Lesson lesson)
        {
            if (id != lesson.Id)
                return BadRequest(new { message = "Id mismatch" });

            if (!await _context.Subjects.AnyAsync(s => s.Id == lesson.SubjectId))
                return BadRequest(new { message = $"Subject with id {lesson.SubjectId} not found" });
            if (!await _context.Teachers.AnyAsync(t => t.Id == lesson.TeacherId))
                return BadRequest(new { message = $"Teacher with id {lesson.TeacherId} not found" });
            if (!await _context.Students.AnyAsync(s => s.Id == lesson.StudentId))
                return BadRequest(new { message = $"Student with id {lesson.StudentId} not found" });

            _context.Entry(lesson).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Lessons.AnyAsync(l => l.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return NotFound(new { message = $"Lesson with id {id} not found" });

            if (lesson.Status == "Cancelled")
                return BadRequest(new { message = "Lesson is already cancelled" });

            lesson.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Lesson cancelled", lesson.Id, lesson.Status });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return NotFound(new { message = $"Lesson with id {id} not found" });

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
