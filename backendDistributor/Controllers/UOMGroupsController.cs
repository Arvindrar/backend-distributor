// Controllers/UOMGroupsController.cs
using backendDistributor.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backendDistributor.Controllers
{
    [Route("api/[controller]")] // Route will be /api/UOMGroups
    [ApiController]
    public class UOMGroupsController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public UOMGroupsController(CustomerDbContext context)
        {
            _context = context;
        }

        // GET: api/UOMGroups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UOMGroup>>> GetUOMGroups()
        {
            if (_context.UOMGroups == null)
            {
                return NotFound("UOMGroups DbSet is null.");
            }
            return await _context.UOMGroups.OrderBy(ug => ug.Name).ToListAsync();
        }

        // GET: api/UOMGroups/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UOMGroup>> GetUOMGroup(int id)
        {
            if (_context.UOMGroups == null)
            {
                return NotFound("UOMGroups DbSet is null.");
            }
            var uomGroup = await _context.UOMGroups.FindAsync(id);

            if (uomGroup == null)
            {
                return NotFound($"UOM Group with ID {id} not found.");
            }

            return uomGroup;
        }

        // POST: api/UOMGroups
        [HttpPost]
        public async Task<ActionResult<UOMGroup>> PostUOMGroup(UOMGroup uomGroup)
        {
            if (_context.UOMGroups == null)
            {
                return Problem("Entity set 'CustomerDbContext.UOMGroups' is null.");
            }

            if (string.IsNullOrWhiteSpace(uomGroup.Name))
            {
                ModelState.AddModelError("Name", "UOM Group name cannot be empty.");
            }

            string trimmedNewName = uomGroup.Name.Trim();
            bool nameExists = await _context.UOMGroups.AnyAsync(x => x.Name.ToLower() == trimmedNewName.ToLower());
            if (nameExists)
            {
                ModelState.AddModelError("Name", $"UOM Group with name '{trimmedNewName}' already exists.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            uomGroup.Name = trimmedNewName; // Save the trimmed name

            _context.UOMGroups.Add(uomGroup);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUOMGroup), new { id = uomGroup.Id }, uomGroup);
        }

        // PUT: api/UOMGroups/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUOMGroup(int id, UOMGroup uomGroup)
        {
            if (id != uomGroup.Id)
            {
                return BadRequest("UOM Group ID in URL does not match UOM Group ID in body.");
            }

            if (_context.UOMGroups == null)
            {
                return Problem("Entity set 'CustomerDbContext.UOMGroups' is null.");
            }

            string trimmedNewName = uomGroup.Name.Trim();
            if (string.IsNullOrWhiteSpace(trimmedNewName))
            {
                ModelState.AddModelError("Name", "UOM Group name cannot be empty.");
            }

            bool nameExists = await _context.UOMGroups.AnyAsync(x => x.Id != id && x.Name.ToLower() == trimmedNewName.ToLower());
            if (nameExists)
            {
                ModelState.AddModelError("Name", $"Another UOM Group with the name '{trimmedNewName}' already exists.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            uomGroup.Name = trimmedNewName; // Ensure the name to be saved is trimmed

            _context.Entry(uomGroup).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UOMGroupExists(id))
                {
                    return NotFound($"UOM Group with ID {id} not found for update.");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "A concurrency error occurred.");
                }
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the database: {ex.InnerException?.Message ?? ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/UOMGroups/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUOMGroup(int id)
        {
            if (_context.UOMGroups == null)
            {
                return NotFound("UOMGroups DbSet is null.");
            }
            var uomGroup = await _context.UOMGroups.FindAsync(id);
            if (uomGroup == null)
            {
                return NotFound($"UOM Group with ID {id} not found for deletion.");
            }

            // Optional: Check if this UOM Group is in use. 
            // This depends on how UOMGroup is related to other entities (e.g., Products or UOMs).
            // Example if Products directly reference UOMGroupName:
            // bool isGroupInUseInProducts = await _context.Products.AnyAsync(p => p.UOMGroup == uomGroup.Name);
            // Example if UOMs belong to UOMGroup (and Product uses UOM):
            // bool hasUOMs = await _context.UOMs.AnyAsync(u => u.UOMGroupId == id); // Assuming UOM has UOMGroupId
            // if (hasUOMs) // or isGroupInUseInProducts
            // {
            //     return BadRequest($"UOM Group '{uomGroup.Name}' cannot be deleted because it is currently in use or contains UOMs.");
            // }


            _context.UOMGroups.Remove(uomGroup);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UOMGroupExists(int id)
        {
            return (_context.UOMGroups?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}