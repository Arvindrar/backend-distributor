// backendDistributor/Controllers/SalesEmployeeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backendDistributor.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backendDistributor.Controllers
{
    [Route("api/SalesEmployee")]
    [ApiController]
    public class SalesEmployeeController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public SalesEmployeeController(CustomerDbContext context)
        {
            _context = context;
        }

        // GET: api/SalesEmployee
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesEmployee>>> GetSalesEmployees()
        {
            if (_context.SalesEmployees == null)
            {
                return NotFound("SalesEmployees DbSet is null.");
            }
            return await _context.SalesEmployees.OrderBy(se => se.Code).ToListAsync();
        }

        // GET: api/SalesEmployee/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SalesEmployee>> GetSalesEmployee(int id)
        {
            if (_context.SalesEmployees == null)
            {
                return NotFound("SalesEmployees DbSet is null.");
            }
            var salesEmployee = await _context.SalesEmployees.FindAsync(id);

            if (salesEmployee == null)
            {
                return NotFound();
            }

            return salesEmployee;
        }

        // POST: api/SalesEmployee
        [HttpPost]
        public async Task<ActionResult<SalesEmployee>> PostSalesEmployee(SalesEmployee salesEmployee)
        {
            // Model validation for [Required], [StringLength], etc. is handled automatically by [ApiController]
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.SalesEmployees == null)
            {
                return Problem("Entity set 'CustomerDbContext.SalesEmployees' is null.");
            }

            // Check for duplicate Code or Name (case-insensitive)
            bool codeExists = await _context.SalesEmployees.AnyAsync(se => se.Code.ToLower() == salesEmployee.Code.ToLower());
            if (codeExists)
            {
                // Return a specific, user-friendly error string that the frontend can easily detect.
                return Conflict("Sales Employee Already Exists! (Code must be unique).");
            }

            bool nameExists = await _context.SalesEmployees.AnyAsync(se => se.Name.ToLower() == salesEmployee.Name.ToLower());
            if (nameExists)
            {
                return Conflict("Sales Employee Already Exists! (Name must be unique).");
            }

            _context.SalesEmployees.Add(salesEmployee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSalesEmployee), new { id = salesEmployee.Id }, salesEmployee);
        }

        // PUT: api/SalesEmployee/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSalesEmployee(int id, SalesEmployee salesEmployee)
        {
            if (id != salesEmployee.Id)
            {
                return BadRequest("SalesEmployee ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the new Code or Name conflicts with another existing record
            var existingByCode = await _context.SalesEmployees.FirstOrDefaultAsync(se => se.Code.ToLower() == salesEmployee.Code.ToLower() && se.Id != id);
            if (existingByCode != null)
            {
                return Conflict("Another sales employee with this code already exists.");
            }

            var existingByName = await _context.SalesEmployees.FirstOrDefaultAsync(se => se.Name.ToLower() == salesEmployee.Name.ToLower() && se.Id != id);
            if (existingByName != null)
            {
                return Conflict("Another sales employee with this name already exists.");
            }

            _context.Entry(salesEmployee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalesEmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // DELETE: api/SalesEmployee/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSalesEmployee(int id)
        {
            if (_context.SalesEmployees == null)
            {
                return NotFound("SalesEmployees DbSet is null.");
            }
            var salesEmployee = await _context.SalesEmployees.FindAsync(id);
            if (salesEmployee == null)
            {
                return NotFound();
            }

            _context.SalesEmployees.Remove(salesEmployee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SalesEmployeeExists(int id)
        {
            return (_context.SalesEmployees?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}