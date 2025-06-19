// backendDistributor/Controllers/VendorController.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backendDistributor.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace backendDistributor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public VendorController(CustomerDbContext context)
        {
            _context = context;
        }

        // GET: api/Vendor
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vendor>>> GetVendors(
            [FromQuery] string? group,      // Filter by Vendor Group name
            [FromQuery] string? searchTerm) // Search by Name or Code
        {
            if (_context.Vendors == null)
            {
                return NotFound(new ProblemDetails { Title = "Vendor data store is not available.", Status = StatusCodes.Status404NotFound });
            }

            var query = _context.Vendors.AsQueryable();

            if (!string.IsNullOrEmpty(group))
            {
                query = query.Where(v => v.Group != null && v.Group.ToLower() == group.ToLower());
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(v =>
                    (v.Name != null && v.Name.ToLower().Contains(term)) ||
                    (v.Code != null && v.Code.ToLower().Contains(term))
                );
            }

            return await query.OrderBy(v => v.Name).ToListAsync();
        }

        // GET: api/Vendor/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vendor>> GetVendor(int id)
        {
            if (_context.Vendors == null)
            {
                return NotFound(new ProblemDetails { Title = "Vendor data store is not available.", Status = StatusCodes.Status404NotFound });
            }

            var vendor = await _context.Vendors.FindAsync(id);

            if (vendor == null)
            {
                return NotFound(new ProblemDetails { Title = $"Vendor with ID {id} not found.", Status = StatusCodes.Status404NotFound });
            }

            return vendor;
        }

        // POST: api/Vendor
        [HttpPost]
        public async Task<ActionResult<Vendor>> PostVendor(Vendor vendor)
        {
            if (_context.Vendors == null)
            {
                return Problem("Entity set 'CustomerDbContext.Vendors' is null.", statusCode: StatusCodes.Status500InternalServerError);
            }

            // 1. ModelState validation (from Data Annotations on Vendor model)
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // 2. Custom Business Logic Validation: Check if vendor code already exists
            if (await _context.Vendors.AnyAsync(v => v.Code == vendor.Code))
            {
                ModelState.AddModelError(nameof(Vendor.Code), "This Vendor Code already exists.");
                return ValidationProblem(ModelState);
            }

            // 3. Optional: Validate if the 'Group' name exists in the VendorGroup table
            // This assumes you have a DbSet<VendorGroup> VendorGroups in your CustomerDbContext
            // and VendorGroup has a 'Name' property.
            if (!string.IsNullOrEmpty(vendor.Group))
            {
                if (_context.VendorGroups == null) // Defensive check
                {
                    ModelState.AddModelError(nameof(Vendor.Group), "VendorGroup data store is not available for validation.");
                    return ValidationProblem(ModelState);
                }
                bool groupIsValid = await _context.VendorGroups.AnyAsync(g => g.Name == vendor.Group);
                if (!groupIsValid)
                {
                    ModelState.AddModelError(nameof(Vendor.Group), $"Vendor group '{vendor.Group}' is not valid or does not exist.");
                    return ValidationProblem(ModelState);
                }
            }

            // 4. Optional: Validate ShippingType against a ShippingTypes table if it's not free text
            // if (!string.IsNullOrEmpty(vendor.ShippingType))
            // {
            //     // Assuming you have a DbSet<ShippingType> ShippingTypes in your CustomerDbContext
            //     // and ShippingType has a 'Name' property.
            //     if (_context.ShippingTypes == null)
            //     {
            //         ModelState.AddModelError(nameof(Vendor.ShippingType), "ShippingType data store is not available for validation.");
            //         return ValidationProblem(ModelState);
            //     }
            //     bool shippingTypeIsValid = await _context.ShippingTypes.AnyAsync(st => st.Name == vendor.ShippingType);
            //     if (!shippingTypeIsValid)
            //     {
            //         ModelState.AddModelError(nameof(Vendor.ShippingType), $"Shipping Type '{vendor.ShippingType}' is not valid or does not exist.");
            //         return ValidationProblem(ModelState);
            //     }
            // }


            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVendor), new { id = vendor.Id }, vendor);
        }

        // PUT: api/Vendor/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVendor(int id, Vendor vendor)
        {
            if (id != vendor.Id)
            {
                ModelState.AddModelError("IdMismatch", "The ID in the URL does not match the ID in the request body.");
                return ValidationProblem(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // Custom Business Logic Validation:
            // Check if changing code to one that already exists (excluding itself)
            if (await _context.Vendors.AnyAsync(v => v.Code == vendor.Code && v.Id != id))
            {
                ModelState.AddModelError(nameof(Vendor.Code), "This Vendor Code already exists for another vendor.");
                return ValidationProblem(ModelState);
            }

            // Optional: Validate 'Group' name
            if (!string.IsNullOrEmpty(vendor.Group))
            {
                if (_context.VendorGroups == null)
                {
                    ModelState.AddModelError(nameof(Vendor.Group), "VendorGroup data store is not available for validation.");
                    return ValidationProblem(ModelState);
                }
                bool groupIsValid = await _context.VendorGroups.AnyAsync(g => g.Name == vendor.Group);
                if (!groupIsValid)
                {
                    ModelState.AddModelError(nameof(Vendor.Group), $"Vendor group '{vendor.Group}' is not valid or does not exist.");
                    return ValidationProblem(ModelState);
                }
            }

            // Optional: Validate 'ShippingType'
            // if (!string.IsNullOrEmpty(vendor.ShippingType))
            // {
            //      if (_context.ShippingTypes == null)
            //     {
            //         ModelState.AddModelError(nameof(Vendor.ShippingType), "ShippingType data store is not available for validation.");
            //         return ValidationProblem(ModelState);
            //     }
            //     bool shippingTypeIsValid = await _context.ShippingTypes.AnyAsync(st => st.Name == vendor.ShippingType);
            //     if (!shippingTypeIsValid)
            //     {
            //         ModelState.AddModelError(nameof(Vendor.ShippingType), $"Shipping Type '{vendor.ShippingType}' is not valid or does not exist.");
            //         return ValidationProblem(ModelState);
            //     }
            // }

            _context.Entry(vendor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VendorExists(id))
                {
                    return NotFound(new ProblemDetails { Title = $"Vendor with ID {id} not found while trying to update.", Status = StatusCodes.Status404NotFound });
                }
                else
                {
                    ModelState.AddModelError("Concurrency", "The vendor record was modified by another user. Please refresh and try again.");
                    return Conflict(ValidationProblem(ModelState));
                }
            }
            catch (DbUpdateException ex)
            {
                return Problem($"An error occurred while updating the database: {ex.InnerException?.Message ?? ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
            }

            return Ok(new { message = "Vendor updated successfully." });
        }

        // DELETE: api/Vendor/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            if (_context.Vendors == null)
            {
                return NotFound(new ProblemDetails { Title = "Vendor data store is not available.", Status = StatusCodes.Status404NotFound });
            }

            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null)
            {
                return NotFound(new ProblemDetails { Title = $"Vendor with ID {id} not found.", Status = StatusCodes.Status404NotFound });
            }

            try
            {
                _context.Vendors.Remove(vendor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("DeleteError", $"Could not delete vendor. They might be associated with other records. Details: {ex.InnerException?.Message ?? ex.Message}");
                return Conflict(ValidationProblem(ModelState));
            }

            return Ok(new { message = "Vendor deleted successfully." });
        }

        private bool VendorExists(int id)
        {
            return (_context.Vendors?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}