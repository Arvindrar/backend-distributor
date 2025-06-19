// backendDistributor/Controllers/SalesEmployeeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backendDistributor.Models; // Your models namespace
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backendDistributor.Controllers
{
    [Route("api/SalesEmployee")] // Explicitly set to singular to match frontend
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
            // Using SalesEmployee model from your namespace
            return await _context.SalesEmployees.OrderBy(se => se.Name).ToListAsync();
        }

        // GET: api/SalesEmployee/5 (Example for CreatedAtAction and potential future use)
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
            if (_context.SalesEmployees == null)
            {
                return Problem("Entity set 'CustomerDbContext.SalesEmployees' is null.");
            }

            if (string.IsNullOrWhiteSpace(salesEmployee.Name))
            {
                ModelState.AddModelError("Name", "Sales employee name cannot be empty.");
                return BadRequest(ModelState);
            }

            // Check if sales employee name already exists (case-insensitive)
            bool employeeExists = await _context.SalesEmployees.AnyAsync(se => se.Name.ToLower() == salesEmployee.Name.ToLower());
            if (employeeExists)
            {
                ModelState.AddModelError("Name", "Sales employee with this name already exists.");
                // Return 409 Conflict, which frontend is set up to potentially parse as "already exists"
                return Conflict(ModelState);
            }

            _context.SalesEmployees.Add(salesEmployee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSalesEmployee), new { id = salesEmployee.Id }, salesEmployee);
        }

        // Optional: PUT for updating
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

            var existingEmployeeWithSameName = await _context.SalesEmployees
                .FirstOrDefaultAsync(se => se.Name.ToLower() == salesEmployee.Name.ToLower() && se.Id != id);

            if (existingEmployeeWithSameName != null)
            {
                ModelState.AddModelError("Name", "Another sales employee with this name already exists.");
                return Conflict(ModelState);
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

        // Optional: DELETE for deleting
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