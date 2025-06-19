// backendDistributor/Controllers/RoutesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backendDistributor.Models; // Your models namespace
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backendDistributor.Controllers
{
    [Route("api/[controller]")] // Sets the base route to /api/Route
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public RoutesController(CustomerDbContext context)
        {
            _context = context;
        }

        // GET: api/Route
        [HttpGet]
        public async Task<ActionResult<IEnumerable<backendDistributor.Models.Route>>> GetRoutes()

        {
            if (_context.Routes == null)
            {
                return NotFound("Routes DbSet is null.");
            }
            return await _context.Routes.OrderBy(r => r.Name).ToListAsync();
        }

        // GET: api/Route/5
        // This is useful for fetching a single route by ID,
        // and also used by CreatedAtAction in the POST method.
        [HttpGet("{id}")]
        public async Task<ActionResult<backendDistributor.Models.Route>> GetRoute(int id)
        {
            if (_context.Routes == null)
            {
                return NotFound("Routes DbSet is null.");
            }
            var route = await _context.Routes.FindAsync(id);

            if (route == null)
            {
                return NotFound();
            }

            return route;
        }


        // POST: api/Route
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<backendDistributor.Models.Route>> PostRoute(backendDistributor.Models.Route route)
        {
            if (_context.Routes == null)
            {
                return Problem("Entity set 'CustomerDbContext.Routes' is null.");
            }

            // Basic validation (Model validation should also catch [Required] attribute)
            if (string.IsNullOrWhiteSpace(route.Name))
            {
                ModelState.AddModelError("Name", "Route name cannot be empty.");
                return BadRequest(ModelState);
            }

            // Check if route name already exists (case-insensitive check for better UX)
            bool routeExists = await _context.Routes.AnyAsync(r => r.Name.ToLower() == route.Name.ToLower());
            if (routeExists)
            {
                // Consistent with how frontend CustomerGroup expects "already exists" error
                // You might return a specific error object if your frontend expects it
                // For simplicity, returning a ModelState error which results in a 400
                ModelState.AddModelError("Name", "Route with this name already exists.");
                return Conflict(ModelState); // HTTP 409 Conflict is often used for this
            }

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            // Returns a 201 Created status with a Location header pointing to the new resource
            // and the newly created route in the body.
            return CreatedAtAction(nameof(GetRoute), new { id = route.Id }, route);
        }

        // Optional: PUT for updating
        // PUT: api/Route/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoute(int id, backendDistributor.Models.Route route)
        {
            if (id != route.Id)
            {
                return BadRequest("Route ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if another route (not the current one) has the new name
            var existingRouteWithSameName = await _context.Routes
                .FirstOrDefaultAsync(r => r.Name.ToLower() == route.Name.ToLower() && r.Id != id);

            if (existingRouteWithSameName != null)
            {
                ModelState.AddModelError("Name", "Another route with this name already exists.");
                return Conflict(ModelState); // Or BadRequest
            }

            _context.Entry(route).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RouteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Or Ok(route) if you want to return the updated entity
        }


        // Optional: DELETE for deleting
        // DELETE: api/Route/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            if (_context.Routes == null)
            {
                return NotFound("Routes DbSet is null.");
            }
            var route = await _context.Routes.FindAsync(id);
            if (route == null)
            {
                return NotFound();
            }

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RouteExists(int id)
        {
            return (_context.Routes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}