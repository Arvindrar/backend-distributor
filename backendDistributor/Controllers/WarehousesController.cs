// Controllers/WarehousesController.cs
using backendDistributor.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backendDistributor.Controllers
{
    [Route("api/[controller]")] // Route will be /api/Warehouses
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public WarehousesController(CustomerDbContext context)
        {
            _context = context;
        }

        // GET: api/Warehouses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetWarehouses()
        {
            if (_context.Warehouses == null)
            {
                return NotFound("Warehouses DbSet is null.");
            }
            // Order by Name or Code, depending on preference
            return await _context.Warehouses.OrderBy(w => w.Name).ToListAsync();
        }

        // GET: api/Warehouses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Warehouse>> GetWarehouse(int id)
        {
            if (_context.Warehouses == null)
            {
                return NotFound("Warehouses DbSet is null.");
            }
            var warehouse = await _context.Warehouses.FindAsync(id);

            if (warehouse == null)
            {
                return NotFound($"Warehouse with ID {id} not found.");
            }

            return warehouse;
        }

        // POST: api/Warehouses
        [HttpPost]
        public async Task<ActionResult<Warehouse>> PostWarehouse(Warehouse warehouse)
        {
            if (_context.Warehouses == null)
            {
                return Problem("Entity set 'CustomerDbContext.Warehouses' is null.");
            }

            // Basic validation (DataAnnotations on model will also be checked)
            if (string.IsNullOrWhiteSpace(warehouse.Code))
            {
                ModelState.AddModelError("Code", "Warehouse code cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(warehouse.Name))
            {
                ModelState.AddModelError("Name", "Warehouse name cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(warehouse.Address))
            {
                ModelState.AddModelError("Address", "Warehouse address cannot be empty.");
            }

            // Trim values before validation and saving
            warehouse.Code = warehouse.Code?.Trim();
            warehouse.Name = warehouse.Name?.Trim();
            warehouse.Address = warehouse.Address?.Trim();


            // Check for uniqueness (case-insensitive)
            if (!string.IsNullOrWhiteSpace(warehouse.Code))
            {
                bool codeExists = await _context.Warehouses.AnyAsync(x => x.Code.ToLower() == warehouse.Code.ToLower());
                if (codeExists)
                {
                    ModelState.AddModelError("Code", $"Warehouse with code '{warehouse.Code}' already exists.");
                }
            }
            // Optionally check for name uniqueness if needed
            // if (!string.IsNullOrWhiteSpace(warehouse.Name))
            // {
            //     bool nameExists = await _context.Warehouses.AnyAsync(x => x.Name.ToLower() == warehouse.Name.ToLower());
            //     if (nameExists)
            //     {
            //         ModelState.AddModelError("Name", $"Warehouse with name '{warehouse.Name}' already exists.");
            //     }
            // }


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWarehouse), new { id = warehouse.Id }, warehouse);
        }

        // PUT: api/Warehouses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWarehouse(int id, Warehouse warehouse)
        {
            if (id != warehouse.Id)
            {
                return BadRequest("Warehouse ID in URL does not match Warehouse ID in body.");
            }

            if (_context.Warehouses == null)
            {
                return Problem("Entity set 'CustomerDbContext.Warehouses' is null.");
            }

            // Trim values before validation and saving
            warehouse.Code = warehouse.Code?.Trim();
            warehouse.Name = warehouse.Name?.Trim();
            warehouse.Address = warehouse.Address?.Trim();

            if (string.IsNullOrWhiteSpace(warehouse.Code))
            {
                ModelState.AddModelError("Code", "Warehouse code cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(warehouse.Name))
            {
                ModelState.AddModelError("Name", "Warehouse name cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(warehouse.Address))
            {
                ModelState.AddModelError("Address", "Warehouse address cannot be empty.");
            }


            // Check for uniqueness if the code is being changed
            if (!string.IsNullOrWhiteSpace(warehouse.Code))
            {
                bool codeExists = await _context.Warehouses.AnyAsync(x => x.Id != id && x.Code.ToLower() == warehouse.Code.ToLower());
                if (codeExists)
                {
                    ModelState.AddModelError("Code", $"Another warehouse with the code '{warehouse.Code}' already exists.");
                }
            }
            // Optionally check for name uniqueness if name is being changed
            // if (!string.IsNullOrWhiteSpace(warehouse.Name))
            // {
            //    bool nameExists = await _context.Warehouses.AnyAsync(x => x.Id != id && x.Name.ToLower() == warehouse.Name.ToLower());
            //    if (nameExists)
            //    {
            //        ModelState.AddModelError("Name", $"Another warehouse with the name '{warehouse.Name}' already exists.");
            //    }
            // }


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(warehouse).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WarehouseExists(id))
                {
                    return NotFound($"Warehouse with ID {id} not found for update.");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "A concurrency error occurred while updating the warehouse.");
                }
            }
            catch (DbUpdateException ex)
            {
                // This can catch unique constraint violations if your HasIndex().IsUnique() is set
                if (ex.InnerException != null && ex.InnerException.Message.Contains("UNIQUE constraint failed"))
                {
                    // Determine which field caused the violation if possible, or return a general message
                    if (ex.InnerException.Message.ToLower().Contains("warehouse.code")) // Adjust based on actual constraint name
                    {
                        ModelState.AddModelError("Code", $"Warehouse with code '{warehouse.Code}' already exists.");
                    }
                    else if (ex.InnerException.Message.ToLower().Contains("warehouse.name"))  // Adjust based on actual constraint name
                    {
                        ModelState.AddModelError("Name", $"Warehouse with name '{warehouse.Name}' already exists.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "A unique constraint was violated. Ensure code and name are unique.");
                    }
                    return BadRequest(ModelState);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the database: {ex.InnerException?.Message ?? ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/Warehouses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            if (_context.Warehouses == null)
            {
                return NotFound("Warehouses DbSet is null.");
            }
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                return NotFound($"Warehouse with ID {id} not found for deletion.");
            }

            // Optional: Check if this Warehouse is in use (e.g., in SalesOrderItems)
            // bool isWarehouseInUse = await _context.SalesOrderItems.AnyAsync(item => item.WarehouseLocation == warehouse.Code); // Example
            // if (isWarehouseInUse)
            // {
            //     return BadRequest($"Warehouse '{warehouse.Name} ({warehouse.Code})' cannot be deleted as it is in use.");
            // }

            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WarehouseExists(int id)
        {
            return (_context.Warehouses?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}