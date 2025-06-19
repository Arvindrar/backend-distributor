// backendDistributor/Controllers/ShippingTypeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backendDistributor.Models; // Your models namespace
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backendDistributor.Controllers
{
    [Route("api/ShippingType")] // Explicitly set to singular to match frontend
    [ApiController]
    public class ShippingTypeController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public ShippingTypeController(CustomerDbContext context)
        {
            _context = context;
        }

        // GET: api/ShippingType
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShippingType>>> GetShippingTypes()
        {
            if (_context.ShippingTypes == null)
            {
                return NotFound("ShippingTypes DbSet is null.");
            }
            // Using ShippingType model from your namespace
            return await _context.ShippingTypes.OrderBy(st => st.Name).ToListAsync();
        }

        // GET: api/ShippingType/5 (Example for CreatedAtAction)
        [HttpGet("{id}")]
        public async Task<ActionResult<ShippingType>> GetShippingType(int id)
        {
            if (_context.ShippingTypes == null)
            {
                return NotFound("ShippingTypes DbSet is null.");
            }
            var shippingType = await _context.ShippingTypes.FindAsync(id);

            if (shippingType == null)
            {
                return NotFound();
            }

            return shippingType;
        }

        // POST: api/ShippingType
        [HttpPost]
        public async Task<ActionResult<ShippingType>> PostShippingType(ShippingType shippingType)
        {
            if (_context.ShippingTypes == null)
            {
                return Problem("Entity set 'CustomerDbContext.ShippingTypes' is null.");
            }

            if (string.IsNullOrWhiteSpace(shippingType.Name))
            {
                ModelState.AddModelError("Name", "Shipping type name cannot be empty.");
                return BadRequest(ModelState);
            }

            // Check if shipping type name already exists (case-insensitive)
            bool typeExists = await _context.ShippingTypes.AnyAsync(st => st.Name.ToLower() == shippingType.Name.ToLower());
            if (typeExists)
            {
                ModelState.AddModelError("Name", "Shipping type with this name already exists.");
                return Conflict(ModelState); // HTTP 409 Conflict
            }

            _context.ShippingTypes.Add(shippingType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShippingType), new { id = shippingType.Id }, shippingType);
        }

        // Optional: PUT for updating
        // PUT: api/ShippingType/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutShippingType(int id, ShippingType shippingType)
        {
            if (id != shippingType.Id)
            {
                return BadRequest("ShippingType ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingTypeWithSameName = await _context.ShippingTypes
                .FirstOrDefaultAsync(st => st.Name.ToLower() == shippingType.Name.ToLower() && st.Id != id);

            if (existingTypeWithSameName != null)
            {
                ModelState.AddModelError("Name", "Another shipping type with this name already exists.");
                return Conflict(ModelState);
            }

            _context.Entry(shippingType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShippingTypeExists(id))
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
        // DELETE: api/ShippingType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShippingType(int id)
        {
            if (_context.ShippingTypes == null)
            {
                return NotFound("ShippingTypes DbSet is null.");
            }
            var shippingType = await _context.ShippingTypes.FindAsync(id);
            if (shippingType == null)
            {
                return NotFound();
            }

            _context.ShippingTypes.Remove(shippingType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ShippingTypeExists(int id)
        {
            return (_context.ShippingTypes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}