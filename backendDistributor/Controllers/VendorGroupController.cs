// backendDistributor/Controllers/VendorGroupController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backendDistributor.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace backendDistributor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorGroupController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public VendorGroupController(CustomerDbContext context)
        {
            _context = context;
        }

        // GET: api/VendorGroup
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VendorGroup>>> GetVendorGroups()
        {
            if (_context.VendorGroups == null)
            {
                return NotFound("VendorGroups data store is not available.");
            }
            return await _context.VendorGroups.OrderBy(vg => vg.Name).ToListAsync();
        }

        // GET: api/VendorGroup/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VendorGroup>> GetVendorGroup(int id)
        {
            if (_context.VendorGroups == null)
            {
                return NotFound();
            }
            var vendorGroup = await _context.VendorGroups.FindAsync(id);
            if (vendorGroup == null)
            {
                return NotFound($"VendorGroup with ID {id} not found.");
            }
            return vendorGroup;
        }

        // POST: api/VendorGroup
        [HttpPost]
        public async Task<ActionResult<VendorGroup>> PostVendorGroup(VendorGroup vendorGroup)
        {
            if (_context.VendorGroups == null)
            {
                return Problem("Entity set 'CustomerDbContext.VendorGroups' is null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ensure the incoming name is not null or empty before comparing
            if (string.IsNullOrWhiteSpace(vendorGroup.Name))
            {
                ModelState.AddModelError("Name", "Vendor Group Name cannot be empty.");
                return BadRequest(ModelState);
            }

            // CORRECTED: Case-insensitive check that can be translated to SQL
            var vendorGroupNameLower = vendorGroup.Name.ToLower(); // Convert input to lower once
            if (await _context.VendorGroups.AnyAsync(vg =>
                    vg.Name != null && // Still good to check for null in DB data
                    vg.Name.ToLower() == vendorGroupNameLower))
            {
                return Conflict(new { message = $"A vendor group with the name '{vendorGroup.Name}' already exists." });
            }

            _context.VendorGroups.Add(vendorGroup);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVendorGroup), new { id = vendorGroup.Id }, vendorGroup);
        }

        // PUT: api/VendorGroup/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVendorGroup(int id, VendorGroup vendorGroup)
        {
            if (id != vendorGroup.Id)
            {
                return BadRequest("ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(vendorGroup.Name))
            {
                ModelState.AddModelError("Name", "Vendor Group Name cannot be empty.");
                return BadRequest(ModelState);
            }

            // CORRECTED: Case-insensitive check that can be translated to SQL
            var vendorGroupNameLower = vendorGroup.Name.ToLower(); // Convert input to lower once
            if (await _context.VendorGroups.AnyAsync(vg =>
                    vg.Id != id && // Exclude the current group being updated
                    vg.Name != null &&
                    vg.Name.ToLower() == vendorGroupNameLower))
            {
                return Conflict(new { message = $"Another vendor group with the name '{vendorGroup.Name}' already exists." });
            }

            _context.Entry(vendorGroup).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VendorGroupExists(id))
                {
                    return NotFound($"VendorGroup with ID {id} not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/VendorGroup/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVendorGroup(int id)
        {
            if (_context.VendorGroups == null)
            {
                return NotFound();
            }
            var vendorGroup = await _context.VendorGroups.FindAsync(id);
            if (vendorGroup == null)
            {
                return NotFound($"VendorGroup with ID {id} not found.");
            }
            _context.VendorGroups.Remove(vendorGroup);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool VendorGroupExists(int id)
        {
            return (_context.VendorGroups?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}