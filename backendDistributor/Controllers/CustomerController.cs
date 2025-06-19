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
    public class CustomerController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public CustomerController(CustomerDbContext context)
        {
            _context = context;
        }

        // GET: api/Customer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers(
            [FromQuery] string? group,
            [FromQuery] string? searchTerm)
        {
            if (_context.Customer == null)
            {
                return NotFound(new ProblemDetails { Title = "Customer data store is not available.", Status = StatusCodes.Status404NotFound });
            }

            var query = _context.Customer.AsQueryable();

            if (!string.IsNullOrEmpty(group))
            {
                query = query.Where(c => c.Group != null && c.Group.ToLower() == group.ToLower());
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(c =>
                    (c.Name != null && c.Name.ToLower().Contains(term)) ||
                    (c.Code != null && c.Code.ToLower().Contains(term))
                );
            }

            return await query.OrderBy(c => c.Name).ToListAsync();
        }

        // GET: api/Customer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id) // Renamed from GetCustomerById for consistency with CreatedAtAction
        {
            if (_context.Customer == null)
            {
                return NotFound(new ProblemDetails { Title = "Customer data store is not available.", Status = StatusCodes.Status404NotFound });
            }

            var customer = await _context.Customer.FindAsync(id);

            if (customer == null)
            {
                return NotFound(new ProblemDetails { Title = $"Customer with ID {id} not found.", Status = StatusCodes.Status404NotFound });
            }

            return customer;
        }

        // POST: api/Customer
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer) // customer is the DTO from request body
        {
            if (_context.Customer == null)
            {
                // For server errors, Problem is more appropriate than just NotFound or simple string.
                return Problem("Entity set 'CustomerDbContext.Customer' is null.", statusCode: StatusCodes.Status500InternalServerError);
            }

            // 1. ModelState validation (from Data Annotations on Customer model)
            // This includes [Required], [StringLength], [EmailAddress], [RegularExpression] for GSTIN, etc.
            if (!ModelState.IsValid)
            {
                // Returns a 400 Bad Request with a ProblemDetails object containing field-specific errors
                return ValidationProblem(ModelState);
            }

            // 2. Custom Business Logic Validation: Check if customer code already exists
            if (await _context.Customer.AnyAsync(c => c.Code == customer.Code))
            {
                // Add a model error specific to the 'Code' field
                ModelState.AddModelError(nameof(Customer.Code), "This Customer Code already exists."); // Using nameof for type safety
                return ValidationProblem(ModelState); // Return 400 with this specific error
            }

            // Optional: Validate if the 'Group' name exists in the CustomerGroup table
            if (!string.IsNullOrEmpty(customer.Group))
            {
                // Assuming you have a DbSet<CustomerGroup> CustomerGroups in your CustomerDbContext
                bool groupIsValid = await _context.CustomerGroups.AnyAsync(g => g.Name == customer.Group);
                if (!groupIsValid)
                {
                    ModelState.AddModelError(nameof(Customer.Group), $"Customer group '{customer.Group}' is not valid or does not exist.");
                    return ValidationProblem(ModelState);
                }
            }


            _context.Customer.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }

        // PUT: api/Customer/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer) // customer is the DTO from request body
        {
            if (id != customer.Id)
            {
                ModelState.AddModelError("IdMismatch", "The ID in the URL does not match the ID in the request body.");
                return ValidationProblem(ModelState); // Return 400
            }

            // 1. ModelState validation (from Data Annotations)
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // 2. Custom Business Logic Validation:
            // Check if changing code to one that already exists (excluding itself)
            if (await _context.Customer.AnyAsync(c => c.Code == customer.Code && c.Id != id))
            {
                ModelState.AddModelError(nameof(Customer.Code), "This Customer Code already exists for another customer.");
                return ValidationProblem(ModelState); // Return 400 with this specific error
            }

            // Optional: Validate if the 'Group' name exists in the CustomerGroup table
            if (!string.IsNullOrEmpty(customer.Group))
            {
                bool groupIsValid = await _context.CustomerGroups.AnyAsync(g => g.Name == customer.Group);
                if (!groupIsValid)
                {
                    ModelState.AddModelError(nameof(Customer.Group), $"Customer group '{customer.Group}' is not valid or does not exist.");
                    return ValidationProblem(ModelState);
                }
            }


            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound(new ProblemDetails { Title = $"Customer with ID {id} not found while trying to update.", Status = StatusCodes.Status404NotFound });
                }
                else
                {
                    // Log the concurrency exception
                    // Return a 409 Conflict with ProblemDetails
                    ModelState.AddModelError("Concurrency", "The customer record was modified by another user. Please refresh and try again.");
                    return Conflict(ValidationProblem(ModelState)); // Use ValidationProblem for consistent error structure
                }
            }
            catch (DbUpdateException ex) // Catch other potential DB update errors
            {
                // Log ex
                return Problem($"An error occurred while updating the database: {ex.InnerException?.Message ?? ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
            }


            // Return Ok with a success message or the updated object
            return Ok(new { message = "Customer updated successfully." });
            // Or: return NoContent(); // If no content needs to be returned
        }

        // DELETE: api/Customer/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            if (_context.Customer == null)
            {
                return NotFound(new ProblemDetails { Title = "Customer data store is not available.", Status = StatusCodes.Status404NotFound });
            }

            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new ProblemDetails { Title = $"Customer with ID {id} not found.", Status = StatusCodes.Status404NotFound });
            }

            try
            {
                _context.Customer.Remove(customer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) // Handle potential foreign key constraint violations or other DB errors
            {
                // Log ex
                // Check if the error is due to FK constraint. If so, provide a user-friendly message.
                // For example, if a customer has orders, they might not be deletable directly.
                // This part requires specific knowledge of your DB schema and error codes.
                // A generic message for now:
                ModelState.AddModelError("DeleteError", $"Could not delete customer. They might be associated with other records (e.g., orders). Details: {ex.InnerException?.Message ?? ex.Message}");
                return Conflict(ValidationProblem(ModelState)); // 409 Conflict is often used for such cases
            }


            return Ok(new { message = "Customer deleted successfully." }); // Or NoContent()
        }

        private bool CustomerExists(int id)
        {
            return (_context.Customer?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}