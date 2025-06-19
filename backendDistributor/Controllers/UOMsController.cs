// Controllers/UOMsController.cs
using backendDistributor.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backendDistributor.Controllers
{
    [Route("api/[controller]")] // Route will be /api/UOMs
    [ApiController]
    public class UOMsController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public UOMsController(CustomerDbContext context)
        {
            _context = context;
        }

        // GET: api/UOMs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UOM>>> GetUOMs()
        {
            if (_context.UOMs == null)
            {
                return NotFound("UOMs DbSet is null.");
            }
            return await _context.UOMs.OrderBy(u => u.Name).ToListAsync();
        }

        // GET: api/UOMs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UOM>> GetUOM(int id)
        {
            if (_context.UOMs == null)
            {
                return NotFound("UOMs DbSet is null.");
            }
            var uom = await _context.UOMs.FindAsync(id);

            if (uom == null)
            {
                return NotFound($"UOM with ID {id} not found.");
            }

            return uom;
        }

        // POST: api/UOMs
        [HttpPost]
        public async Task<ActionResult<UOM>> PostUOM(UOM uom)
        {
            if (_context.UOMs == null)
            {
                return Problem("Entity set 'CustomerDbContext.UOMs' is null.");
            }

            // Basic validation (DataAnnotations on model will also be checked)
            if (string.IsNullOrWhiteSpace(uom.Name))
            {
                ModelState.AddModelError("Name", "UOM name cannot be empty.");
            }

            // Check for uniqueness (case-insensitive for robustness)
            // Ensure the uom object passed in has its Name trimmed if needed for comparison
            string trimmedNewName = uom.Name.Trim();
            bool nameExists = await _context.UOMs.AnyAsync(x => x.Name.ToLower() == trimmedNewName.ToLower());
            if (nameExists)
            {
                ModelState.AddModelError("Name", $"UOM with name '{trimmedNewName}' already exists.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Trim name before saving to ensure consistency
            uom.Name = trimmedNewName;

            _context.UOMs.Add(uom);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUOM), new { id = uom.Id }, uom);
        }

        // PUT: api/UOMs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUOM(int id, UOM uom)
        {
            if (id != uom.Id)
            {
                return BadRequest("UOM ID in URL does not match UOM ID in body.");
            }

            if (_context.UOMs == null)
            {
                return Problem("Entity set 'CustomerDbContext.UOMs' is null.");
            }

            // Trim name before validation and saving
            string trimmedNewName = uom.Name.Trim();
            if (string.IsNullOrWhiteSpace(trimmedNewName))
            {
                ModelState.AddModelError("Name", "UOM name cannot be empty.");
            }

            // Check for uniqueness if the name is being changed
            // We need to make sure we're not conflicting with ANOTHER UOM's name
            bool nameExists = await _context.UOMs.AnyAsync(x => x.Id != id && x.Name.ToLower() == trimmedNewName.ToLower());
            if (nameExists)
            {
                ModelState.AddModelError("Name", $"Another UOM with the name '{trimmedNewName}' already exists.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update the name with the trimmed version
            uom.Name = trimmedNewName;

            _context.Entry(uom).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UOMExists(id))
                {
                    return NotFound($"UOM with ID {id} not found for update.");
                }
                else
                {
                    // Log the concurrency exception or handle as needed
                    return StatusCode(StatusCodes.Status500InternalServerError, "A concurrency error occurred.");
                }
            }
            catch (DbUpdateException ex)
            {
                // Catch other potential database update errors, e.g., unique constraint violation if not caught above
                // Log ex.InnerException for more details
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the database: {ex.InnerException?.Message ?? ex.Message}");
            }


            return NoContent(); // Standard success response for PUT if no content is returned
        }


        // DELETE: api/UOMs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUOM(int id)
        {
            if (_context.UOMs == null)
            {
                return NotFound("UOMs DbSet is null.");
            }
            var uom = await _context.UOMs.FindAsync(id);
            if (uom == null)
            {
                return NotFound($"UOM with ID {id} not found for deletion.");
            }

            // Optional: Check if this UOM is in use by any Products before deleting
            // bool isUOMInUse = await _context.Products.AnyAsync(p => p.UOM == uom.Name); // Assuming Product has a UOM string property
            // if (isUOMInUse)
            // {
            //     return BadRequest($"UOM '{uom.Name}' cannot be deleted because it is currently in use by one or more products.");
            // }

            _context.UOMs.Remove(uom);
            await _context.SaveChangesAsync();

            return NoContent(); // Standard success response for DELETE
        }

        private bool UOMExists(int id)
        {
            return (_context.UOMs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}