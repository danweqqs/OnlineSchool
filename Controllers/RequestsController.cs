using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Models;

namespace OnlineSchool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestsController : ControllerBase
    {
        private readonly SchoolContext _context;
        private static readonly string[] AllowedTypes = { "DayOff", "Reschedule" };

        public RequestsController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var requests = await _context.Requests
                .Include(r => r.Teacher)
                .OrderByDescending(r => r.Date)
                .Select(r => new
                {
                    r.Id,
                    r.Type,
                    r.Description,
                    r.Date,
                    r.Status,
                    Teacher = r.Teacher != null ? r.Teacher.FullName : null,
                    r.TeacherId
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var request = await _context.Requests
                .Include(r => r.Teacher)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound(new { message = $"Request with id {id} not found" });

            return Ok(new
            {
                request.Id,
                request.Type,
                request.Description,
                request.Date,
                request.Status,
                Teacher = request.Teacher?.FullName,
                request.TeacherId
            });
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<object>>> GetPending()
        {
            var requests = await _context.Requests
                .Include(r => r.Teacher)
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.Date)
                .Select(r => new
                {
                    r.Id,
                    r.Type,
                    r.Description,
                    r.Date,
                    r.Status,
                    Teacher = r.Teacher != null ? r.Teacher.FullName : null,
                    r.TeacherId
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost]
        public async Task<ActionResult<Request>> Create(Request request)
        {
            if (!AllowedTypes.Contains(request.Type))
                return BadRequest(new { message = $"Invalid request type. Allowed: {string.Join(", ", AllowedTypes)}" });

            if (!await _context.Teachers.AnyAsync(t => t.Id == request.TeacherId))
                return BadRequest(new { message = $"Teacher with id {request.TeacherId} not found" });

            var hasPending = await _context.Requests.AnyAsync(r =>
                r.TeacherId == request.TeacherId &&
                r.Type == request.Type &&
                r.Status == "Pending");
            if (hasPending)
                return Conflict(new { message = $"Teacher already has a pending '{request.Type}' request" });

            request.Status = "Pending";
            request.Date = DateTime.UtcNow;

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
        }

        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null) return NotFound(new { message = $"Request with id {id} not found" });

            if (request.Status != "Pending")
                return BadRequest(new { message = $"Can only approve pending requests. Current status: {request.Status}" });

            request.Status = "Approved";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request approved", request.Id, request.Status });
        }

        [HttpPatch("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null) return NotFound(new { message = $"Request with id {id} not found" });

            if (request.Status != "Pending")
                return BadRequest(new { message = $"Can only reject pending requests. Current status: {request.Status}" });

            request.Status = "Rejected";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request rejected", request.Id, request.Status });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null) return NotFound(new { message = $"Request with id {id} not found" });

            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
