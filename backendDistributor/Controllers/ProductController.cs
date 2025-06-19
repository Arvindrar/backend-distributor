using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backendDistributor.Models;
using backendDistributor.Dtos; // Assuming ProductCreateDto and potentially ProductUpdateDto are here
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http; // For IFormFile
using System.IO;                 // For Path, Directory, FileStream
using System;                    // For Guid, Exception
using Microsoft.AspNetCore.Hosting; // For IWebHostEnvironment

namespace backendDistributor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly CustomerDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment; // For serving/saving files

        public ProductController(CustomerDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
            [FromQuery] string? group,
            [FromQuery] string? searchTerm)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'CustomerDbContext.Products' is null.", statusCode: 500);
            }

            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(group))
            {
                query = query.Where(p => p.Group != null && p.Group.ToLower() == group.ToLower());
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(term)) ||
                    (p.SKU != null && p.SKU.ToLower().Contains(term)) ||
                    (p.Group != null && p.Group.ToLower().Contains(term))
                );
            }
            return await query.OrderBy(p => p.Name).ToListAsync();
        }

        // GET: api/Product/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'CustomerDbContext.Products' is null.", statusCode: 500);
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            return product;
        }

        // POST: api/Product
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromForm] ProductCreateDto productDto)
        {
            if (_context.Products == null) return Problem("Entity set 'CustomerDbContext.Products' is null.", statusCode: 500);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Check for existing SKU
            if (await _context.Products.AnyAsync(p => p.SKU == productDto.SKU))
            {
                ModelState.AddModelError("SKU", $"A product with SKU '{productDto.SKU}' already exists.");
                return BadRequest(ModelState);
            }

            // Handle Product Group (create if not exists)
            var productGroupEntity = await _context.ProductGroups
                                     .FirstOrDefaultAsync(pg => pg.Name.ToLower() == productDto.ProductGroup.ToLower());
            if (productGroupEntity == null)
            {
                // Ensure ProductGroup name is not empty if creating a new one
                if (string.IsNullOrWhiteSpace(productDto.ProductGroup))
                {
                    ModelState.AddModelError("ProductGroup", "Product group name cannot be empty.");
                    return BadRequest(ModelState);
                }
                productGroupEntity = new ProductGroup { Name = productDto.ProductGroup };
                _context.ProductGroups.Add(productGroupEntity);
                // Note: SaveChangesAsync will be called later, saving both product and potentially new group
            }

            var product = new Product
            {
                SKU = productDto.SKU,
                Name = productDto.ProductName,
                Group = productDto.ProductGroup, // Store the name of the group
                UOM = productDto.UOM,
                HSN = productDto.HSN,
                UOMGroup = productDto.UOMGroup,
                // RetailPrice and WholesalePrice are parsed from string DTO properties
            };

            // Parse and assign prices
            if (decimal.TryParse(productDto.RetailPrice, out decimal retailP))
            {
                product.RetailPrice = retailP;
            }
            else if (!string.IsNullOrEmpty(productDto.RetailPrice)) // If not empty but couldn't parse
            {
                ModelState.AddModelError("RetailPrice", "Invalid retail price format.");
                // Potentially return BadRequest(ModelState) here if prices are mandatory and invalid
            }

            if (decimal.TryParse(productDto.WholesalePrice, out decimal wholesaleP))
            {
                product.WholesalePrice = wholesaleP;
            }
            else if (!string.IsNullOrEmpty(productDto.WholesalePrice)) // If not empty but couldn't parse
            {
                ModelState.AddModelError("WholesalePrice", "Invalid wholesale price format.");
                // Potentially return BadRequest(ModelState) here
            }

            // If there were parsing errors for prices and you want to stop:
            if (!ModelState.IsValid) return BadRequest(ModelState);


            // Image handling
            if (productDto.ProductImage != null && productDto.ProductImage.Length > 0)
            {
                var uploadsFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsFolderPath)) Directory.CreateDirectory(uploadsFolderPath);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetExtension(productDto.ProductImage.FileName);
                var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await productDto.ProductImage.CopyToAsync(stream);
                    }
                    product.ImageFileName = uniqueFileName; // Store only the file name
                }
                catch (Exception ex)
                {
                    // Log the exception (e.g., using a logger)
                    Console.WriteLine($"Error saving product image: {ex.Message}");
                    ModelState.AddModelError("ProductImage", "Could not save the product image due to an internal error.");
                    return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }


        // PUT: api/Product/{id}
        // For PUT, you might want a ProductUpdateDto similar to ProductCreateDto,
        // but often properties are optional for update.
        // Here, I'm using ProductCreateDto for simplicity, but adjust as needed.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutProduct(int id, [FromForm] ProductCreateDto productDto) // Consider a ProductUpdateDto
        {
            if (_context.Products == null) return Problem("Entity set 'CustomerDbContext.Products' is null.", statusCode: 500);

            var productToUpdate = await _context.Products.FindAsync(id);
            if (productToUpdate == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            // Validate ModelState (e.g., if using data annotations on ProductUpdateDto)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if SKU is being changed and if the new SKU already exists for another product
            if (productToUpdate.SKU != productDto.SKU && await _context.Products.AnyAsync(p => p.SKU == productDto.SKU && p.Id != id))
            {
                ModelState.AddModelError("SKU", $"Another product with SKU '{productDto.SKU}' already exists.");
                return BadRequest(ModelState);
            }

            // Update product properties
            productToUpdate.SKU = productDto.SKU;
            productToUpdate.Name = productDto.ProductName;
            productToUpdate.UOM = productDto.UOM;
            productToUpdate.HSN = productDto.HSN;
            productToUpdate.UOMGroup = productDto.UOMGroup;

            // Handle Product Group (update if different, create if new and not exists)
            if (productToUpdate.Group?.ToLower() != productDto.ProductGroup?.ToLower())
            {
                if (string.IsNullOrWhiteSpace(productDto.ProductGroup))
                {
                    ModelState.AddModelError("ProductGroup", "Product group name cannot be empty if changing.");
                    return BadRequest(ModelState);
                }
                var productGroupEntity = await _context.ProductGroups
                                         .FirstOrDefaultAsync(pg => pg.Name.ToLower() == productDto.ProductGroup.ToLower());
                if (productGroupEntity == null)
                {
                    productGroupEntity = new ProductGroup { Name = productDto.ProductGroup };
                    _context.ProductGroups.Add(productGroupEntity);
                }
                productToUpdate.Group = productDto.ProductGroup; // Update the group name string
            }


            // Parse and update prices
            if (decimal.TryParse(productDto.RetailPrice, out decimal retailP))
            {
                productToUpdate.RetailPrice = retailP;
            }
            else if (!string.IsNullOrEmpty(productDto.RetailPrice))
            {
                ModelState.AddModelError("RetailPrice", "Invalid retail price format.");
            }

            if (decimal.TryParse(productDto.WholesalePrice, out decimal wholesaleP))
            {
                productToUpdate.WholesalePrice = wholesaleP;
            }
            else if (!string.IsNullOrEmpty(productDto.WholesalePrice))
            {
                ModelState.AddModelError("WholesalePrice", "Invalid wholesale price format.");
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);


            // Image handling for update (delete old if new one provided)
            if (productDto.ProductImage != null && productDto.ProductImage.Length > 0)
            {
                // Delete old image if it exists
                if (!string.IsNullOrEmpty(productToUpdate.ImageFileName))
                {
                    var oldImagePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "products", productToUpdate.ImageFileName);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                        catch (IOException ioEx)
                        {
                            // Log this error, but don't necessarily stop the update
                            Console.WriteLine($"Error deleting old image '{productToUpdate.ImageFileName}': {ioEx.Message}");
                        }
                    }
                }

                // Save new image
                var uploadsFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsFolderPath)) Directory.CreateDirectory(uploadsFolderPath);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetExtension(productDto.ProductImage.FileName);
                var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await productDto.ProductImage.CopyToAsync(stream);
                    }
                    productToUpdate.ImageFileName = uniqueFileName;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving updated product image: {ex.Message}");
                    ModelState.AddModelError("ProductImage", "Could not save the new product image.");
                    return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
                }
            }
            // If productDto.ProductImage is null, the existing image (if any) is kept.
            // If you want to allow REMOVING an image, you'd need another DTO flag like "RemoveImage = true".

            _context.Entry(productToUpdate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound($"Product with ID {id} not found during update.");
                }
                else
                {
                    throw; // Or handle concurrency conflict more gracefully
                }
            }
            catch (DbUpdateException ex)
            {
                // Log ex.InnerException for more details
                return Problem($"An error occurred while updating the product in the database: {ex.Message}", statusCode: 500);
            }

            return NoContent(); // Standard for successful PUT
            // Or: return Ok(productToUpdate); // To return the updated entity
        }


        // DELETE: api/Product/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'CustomerDbContext.Products' is null.", statusCode: 500);
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            // Delete associated image file if it exists
            if (!string.IsNullOrEmpty(product.ImageFileName))
            {
                var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "products", product.ImageFileName);
                if (System.IO.File.Exists(imagePath))
                {
                    try
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    catch (IOException ioEx)
                    {
                        // Log this error. Depending on policy, you might still proceed with DB deletion
                        // or return an error if image deletion is critical.
                        Console.WriteLine($"Could not delete image file '{product.ImageFileName}' for product ID {id}: {ioEx.Message}");
                        // Optionally, return a server error if image deletion is critical
                        // return Problem($"Could not delete associated image file. Database record not deleted.", statusCode: 500);
                    }
                }
            }

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Log ex.InnerException for more details
                // This could be due to foreign key constraints if this product is referenced elsewhere
                return Conflict($"An error occurred while deleting the product. It might be in use. Details: {ex.Message}");
            }


            return NoContent(); // Standard for successful DELETE
        }

        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}