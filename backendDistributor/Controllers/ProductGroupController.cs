using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backendDistributor.Models; // Assuming ProductGroup model is in here
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace backendDistributor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductGroupController : ControllerBase
    {
        private readonly CustomerDbContext _context; // Assuming CustomerDbContext also contains ProductGroups DbSet

        public ProductGroupController(CustomerDbContext context)
        {
            _context = context;
        }

        // GET: api/ProductGroup
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductGroup>>> GetProductGroups()
        {
            if (_context.ProductGroups == null)
            {
                // This indicates a configuration problem - DbSet might not be initialized
                return Problem("Entity set 'CustomerDbContext.ProductGroups' is null. Check DbContext configuration.", statusCode: 500);
            }
            return await _context.ProductGroups.OrderBy(pg => pg.Name).ToListAsync();
        }

        // GET: api/ProductGroup/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductGroup>> GetProductGroupById(int id)
        {
            if (_context.ProductGroups == null)
            {
                return Problem("Entity set 'CustomerDbContext.ProductGroups' is null.", statusCode: 500);
            }
            var productGroup = await _context.ProductGroups.FindAsync(id);

            if (productGroup == null)
            {
                return NotFound($"Product group with ID {id} not found.");
            }
            return productGroup;
        }

        // POST: api/ProductGroup
        [HttpPost]
        public async Task<ActionResult<ProductGroup>> PostProductGroup(ProductGroup productGroup)
        {
            if (_context.ProductGroups == null)
            {
                return Problem("Entity set 'CustomerDbContext.ProductGroups' is null. Check DbContext configuration.", statusCode: 500);
            }

            // Basic validation check from model attributes
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if group name already exists (case-insensitive)
            if (await _context.ProductGroups.AnyAsync(pg => pg.Name.ToLower() == productGroup.Name.ToLower()))
            {
                ModelState.AddModelError("Name", $"A product group with the name '{productGroup.Name}' already exists.");
                return BadRequest(ModelState); // Returns HTTP 400
            }

            _context.ProductGroups.Add(productGroup);
            await _context.SaveChangesAsync();

            // Return 201 Created with a link to the new resource and the resource itself
            return CreatedAtAction(nameof(GetProductGroupById), new { id = productGroup.Id }, productGroup);
        }

        // PUT: api/ProductGroup/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductGroup(int id, ProductGroup productGroup)
        {
            if (id != productGroup.Id)
            {
                return BadRequest("ID in URL does not match ID in request body.");
            }

            if (_context.ProductGroups == null)
            {
                return Problem("Entity set 'CustomerDbContext.ProductGroups' is null.", statusCode: 500);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if trying to update to a name that already exists for another group
            if (await _context.ProductGroups.AnyAsync(pg => pg.Name.ToLower() == productGroup.Name.ToLower() && pg.Id != id))
            {
                ModelState.AddModelError("Name", $"Another product group with the name '{productGroup.Name}' already exists.");
                return BadRequest(ModelState);
            }

            _context.Entry(productGroup).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductGroupExists(id))
                {
                    return NotFound($"Product group with ID {id} not found.");
                }
                else
                {
                    // Log the exception, rethrow, or return a specific error
                    throw; // Rethrowing will likely result in a 500 Internal Server Error
                }
            }
            catch (DbUpdateException ex)
            {
                // Log the exception (ex.InnerException might have more details)
                // Consider more specific error handling if needed, e.g., for foreign key constraints
                return Problem($"An error occurred while updating the database: {ex.Message}", statusCode: 500);
            }


            return NoContent(); // Standard response for a successful PUT if not returning the entity
            // Or: return Ok(productGroup); // If you want to return the updated entity
        }


        // DELETE: api/ProductGroup/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductGroup(int id)
        {
            if (_context.ProductGroups == null)
            {
                return Problem("Entity set 'CustomerDbContext.ProductGroups' is null.", statusCode: 500);
            }

            var productGroup = await _context.ProductGroups.FindAsync(id);
            if (productGroup == null)
            {
                return NotFound($"Product group with ID {id} not found.");
            }

            // Optional: Check for dependencies before deleting if ProductGroup is referenced by other entities
            // For example, if Products table has a ProductGroupId foreign key:
            // var relatedProducts = await _context.Products.AnyAsync(p => p.ProductGroupId == id);
            // if (relatedProducts)
            // {
            //     return BadRequest($"Cannot delete product group '{productGroup.Name}' as it is currently associated with one or more products.");
            // }

            try
            {
                _context.ProductGroups.Remove(productGroup);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // This can happen due to database constraints (e.g., foreign keys preventing delete)
                // Log the exception (ex.InnerException often has more details)
                // Consider returning a more specific error to the client
                return Conflict($"An error occurred while deleting the product group. It might be in use. Details: {ex.Message}");
            }


            return NoContent(); // Standard successful delete response
        }

        private bool ProductGroupExists(int id)
        {
            // Ensure ProductGroups is not null before trying to query it
            return (_context.ProductGroups?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}