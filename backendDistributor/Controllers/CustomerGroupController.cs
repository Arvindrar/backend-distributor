// backendDistributor/Controllers/CustomerGroupController.cs
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
    public class CustomerGroupController : ControllerBase
    {
        private readonly CustomerDbContext _context; // Use your existing DbContext

        public CustomerGroupController(CustomerDbContext context)
        {
            _context = context;
        }

        // GET: api/CustomerGroup
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerGroup>>> GetCustomerGroups()
        {
            if (_context.CustomerGroups == null)
            {
                return NotFound("CustomerGroups data store is not available.");
            }
            return await _context.CustomerGroups.OrderBy(cg => cg.Name).ToListAsync();
        }

        // GET: api/CustomerGroup/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerGroup>> GetCustomerGroup(int id)
        {
            if (_context.CustomerGroups == null)
            {
                return NotFound();
            }

            var customerGroup = await _context.CustomerGroups.FindAsync(id);

            if (customerGroup == null)
            {
                return NotFound($"CustomerGroup with ID {id} not found.");
            }

            return customerGroup;
        }

        // POST: api/CustomerGroup
        [HttpPost]
        public async Task<ActionResult<CustomerGroup>> PostCustomerGroup(CustomerGroup customerGroup)
        {
            if (_context.CustomerGroups == null)
            {
                return Problem("Entity set 'CustomerDbContext.CustomerGroups' is null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Optional: Check if a group with the same name already exists
            if (await _context.CustomerGroups.AnyAsync(cg => cg.Name.ToLower() == customerGroup.Name.ToLower()))
            {
                ModelState.AddModelError("Name", $"A customer group with the name '{customerGroup.Name}' already exists.");
                return BadRequest(ModelState);
            }

            _context.CustomerGroups.Add(customerGroup);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomerGroup), new { id = customerGroup.Id }, customerGroup);
        }

        // PUT: api/CustomerGroup/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomerGroup(int id, CustomerGroup customerGroup)
        {
            if (id != customerGroup.Id)
            {
                return BadRequest("ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Optional: Check if changing name to one that already exists (excluding itself)
            if (await _context.CustomerGroups.AnyAsync(cg => cg.Name.ToLower() == customerGroup.Name.ToLower() && cg.Id != id))
            {
                ModelState.AddModelError("Name", $"Another customer group with the name '{customerGroup.Name}' already exists.");
                return BadRequest(ModelState);
            }

            _context.Entry(customerGroup).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerGroupExists(id))
                {
                    return NotFound($"CustomerGroup with ID {id} not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Or return Ok(customerGroup);
        }

        // DELETE: api/CustomerGroup/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomerGroup(int id)
        {
            if (_context.CustomerGroups == null)
            {
                return NotFound();
            }

            var customerGroup = await _context.CustomerGroups.FindAsync(id);
            if (customerGroup == null)
            {
                return NotFound($"CustomerGroup with ID {id} not found.");
            }

            _context.CustomerGroups.Remove(customerGroup);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerGroupExists(int id)
        {
            return (_context.CustomerGroups?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}