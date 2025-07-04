// Controllers/UOMGroupsController.cs
//using backendDistributor.Data; // Assuming your DbContext is in a Data folder
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

        /// <summary>
        /// Gets a list of all UOM Groups, ordered by name.
        /// </summary>
        /// <returns>A list of UOM Groups.</returns>
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

        /// <summary>
        /// Gets a specific UOM Group by its ID.
        /// </summary>
        /// <param name="id">The ID of the UOM Group.</param>
        /// <returns>The requested UOM Group.</returns>
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

        /// <summary>
        /// Creates a new UOM Group.
        /// </summary>
        /// <param name="uomGroup">The UOM Group data from the request body.</param>
        /// <returns>The newly created UOM Group.</returns>
        // POST: api/UOMGroups
        [HttpPost]
        public async Task<ActionResult<UOMGroup>> PostUOMGroup(UOMGroup uomGroup)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.UOMGroups == null)
            {
                return Problem("Entity set 'CustomerDbContext.UOMGroups' is null.");
            }

            // Trim and validate name
            string trimmedNewName = uomGroup.Name.Trim();
            if (string.IsNullOrWhiteSpace(trimmedNewName))
            {
                ModelState.AddModelError("Name", "UOM Group name cannot be empty.");
                return BadRequest(ModelState);
            }

            bool nameExists = await _context.UOMGroups.AnyAsync(x => x.Name.ToLower() == trimmedNewName.ToLower());
            if (nameExists)
            {
                ModelState.AddModelError("Name", $"UOM Group with name '{trimmedNewName}' already exists.");
                return BadRequest(ModelState);
            }

            // Assign trimmed and processed values to a new entity
            var newUomGroup = new UOMGroup
            {
                Name = trimmedNewName,
                Description = uomGroup.Description?.Trim() // Trim description if provided
                // CreatedDate is set by the model's constructor
            };

            _context.UOMGroups.Add(newUomGroup);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUOMGroup), new { id = newUomGroup.Id }, newUomGroup);
        }

        /// <summary>
        /// Updates an existing UOM Group.
        /// </summary>
        /// <param name="id">The ID of the UOM Group to update.</param>
        /// <param name="uomGroup">The updated UOM Group data.</param>
        /// <returns>An HTTP status code indicating the result.</returns>
        // PUT: api/UOMGroups/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUOMGroup(int id, UOMGroup uomGroup)
        {
            if (id != uomGroup.Id)
            {
                return BadRequest("UOM Group ID in URL does not match UOM Group ID in body.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Fetch the existing entity from the database
            var uomGroupToUpdate = await _context.UOMGroups.FindAsync(id);
            if (uomGroupToUpdate == null)
            {
                return NotFound($"UOM Group with ID {id} not found for update.");
            }

            // Trim and validate name
            string trimmedNewName = uomGroup.Name.Trim();
            if (string.IsNullOrWhiteSpace(trimmedNewName))
            {
                ModelState.AddModelError("Name", "UOM Group name cannot be empty.");
                return BadRequest(ModelState);
            }

            // Check if another group already has the new name
            bool nameExists = await _context.UOMGroups.AnyAsync(x => x.Id != id && x.Name.ToLower() == trimmedNewName.ToLower());
            if (nameExists)
            {
                ModelState.AddModelError("Name", $"Another UOM Group with the name '{trimmedNewName}' already exists.");
                return BadRequest(ModelState);
            }

            // Update only the properties that are allowed to be changed
            uomGroupToUpdate.Name = trimmedNewName;
            uomGroupToUpdate.Description = uomGroup.Description?.Trim();
            // Do NOT update CreatedDate

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UOMGroupExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "A concurrency error occurred.");
                }
            }

            return NoContent(); // Success, no content to return
        }

        /// <summary>
        /// Deletes a UOM Group.
        /// </summary>
        /// <param name="id">The ID of the UOM Group to delete.</param>
        /// <returns>An HTTP status code indicating the result.</returns>
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

            // BEST PRACTICE: Before deleting, check if this UOM Group is in use.
            // This requires knowledge of other models. If a 'Product' or 'UOM' model
            // has a foreign key to UOMGroup, you should check it here.
            // Example: bool isInUse = await _context.Products.AnyAsync(p => p.UOMGroupId == id);
            // if (isInUse)
            // {
            //     return BadRequest($"UOM Group '{uomGroup.Name}' cannot be deleted because it is currently in use.");
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