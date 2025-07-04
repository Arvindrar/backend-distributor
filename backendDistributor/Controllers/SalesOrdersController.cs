using backendDistributor.DTOs;
using backendDistributor.Models;
using backendDistributor.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace backendDistributor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrdersController : ControllerBase
    {
        private readonly CustomerDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SalesOrdersController(CustomerDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesOrderListDto>>> GetSalesOrders(
    [FromQuery] string? salesOrderNo,
    [FromQuery] string? customerName)
        {
            // Start with a base IQueryable to build upon
            var query = _context.SalesOrders.AsQueryable();

            // Conditionally apply the search filter for Sales Order Number
            if (!string.IsNullOrEmpty(salesOrderNo))
            {
                // Using .Contains() allows for partial string matching.
                // If you need an exact match, use: query = query.Where(o => o.SalesOrderNo == salesOrderNo);
                query = query.Where(o => o.SalesOrderNo.Contains(salesOrderNo));
            }

            // Conditionally apply the search filter for Customer Name
            if (!string.IsNullOrEmpty(customerName))
            {
                query = query.Where(o => o.CustomerName.Contains(customerName));
            }

            // Now, apply the rest of your logic to the potentially filtered query
            var orders = await query
                .Include(o => o.SalesItems)
                .Select(order => new SalesOrderListDto
                {
                    Id = order.Id,
                    SalesOrderNo = order.SalesOrderNo,
                    CustomerCode = order.CustomerCode,
                    CustomerName = order.CustomerName,
                    SODate = order.SODate,
                    SalesRemarks = order.SalesRemarks,
                    OrderTotal = order.SalesItems.Sum(i => i.Total)
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetById(Guid id)
        {
            var order = await _context.SalesOrders
                .Include(o => o.SalesItems)
                .Include(o => o.Attachments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var result = new
            {
                order.Id,
                order.SalesOrderNo,
                order.CustomerCode,
                order.CustomerName,
                order.SODate,
                order.DeliveryDate,
                order.CustomerRefNumber,
                order.ShipToAddress,
                order.SalesRemarks,
                order.SalesEmployee,
                salesItems = order.SalesItems.Select(i => new
                {
                    i.Id,
                    i.ProductCode,
                    i.ProductName,
                    i.Quantity,
                    i.UOM,
                    i.Price,
                    i.WarehouseLocation,
                    i.TaxCode,
                    i.TaxPrice,
                    i.Total
                }).ToList(),
                attachments = order.Attachments.Select(a => new
                {
                    a.Id,
                    a.FileName,
                    a.FilePath,
                    a.SalesOrderId
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> Create([FromForm] SalesOrderCreateDto dto)
        {
            // 1. Check for SalesItemsJson (this assumes frontend SalesAdd.jsx is sending it)
            if (!Request.Form.TryGetValue("SalesItemsJson", out var itemsJsonString) || string.IsNullOrEmpty(itemsJsonString.ToString()))
            {
                return BadRequest(new { message = "Missing or empty SalesItemsJson in form data." });
            }

            // 2. Deserialize SalesItemsJson
            try
            {
                // Use JsonSerializerOptions that match your Program.cs/Startup.cs if needed, though default should be fine.
                dto.SalesItems = JsonSerializer.Deserialize<List<SalesItemDto>>(itemsJsonString.ToString());
            }
            catch (JsonException ex)
            {
                return BadRequest(new { message = $"Failed to parse SalesItemsJson: {ex.Message}" });
            }

            // 3. Basic check for SalesItems presence after deserialization
            if (dto.SalesItems == null || !dto.SalesItems.Any())
            {
                return BadRequest(new { message = "SalesItems are required and cannot be empty after parsing." });
            }

            // 4. ASP.NET Core Model Validation (will check [Required], data types in SalesOrderCreateDto)
            // This happens automatically before the action method is called if you're not using .SuppressModelStateInvalidFilter()
            // If you want to explicitly check:
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns detailed validation errors
            }


            // --- Start Business Logic ---
            var tracker = await _context.SalesOrderNumberTrackers.FindAsync(1) // Assuming ID 1 for single tracker row
                          ?? new SalesOrderNumberTracker { Id = 1, LastUsedNumber = 1000000 }; // Default if not found

            if (_context.Entry(tracker).State == EntityState.Detached) // If it's a new instance from '??'
            {
                _context.SalesOrderNumberTrackers.Add(tracker);
            }
            tracker.LastUsedNumber++;
            string salesOrderNo = $"SO-{tracker.LastUsedNumber}";

            var orderId = Guid.NewGuid();
            var salesOrder = new SalesOrder
            {
                Id = orderId,
                SalesOrderNo = salesOrderNo,
                CustomerCode = dto.CustomerCode, // Assumes these are validated by ModelState or are nullable in DTO
                CustomerName = dto.CustomerName,
                SODate = dto.SODate,
                DeliveryDate = dto.DeliveryDate,
                CustomerRefNumber = dto.CustomerRefNumber,
                ShipToAddress = dto.ShipToAddress,
                SalesRemarks = dto.SalesRemarks,
                SalesEmployee = dto.SalesEmployee,
                Attachments = new List<SalesOrderAttachment>() // Initialize
            };

            // Map SalesItemDto to SalesOrderItem entity
            salesOrder.SalesItems = dto.SalesItems.Select(i => new SalesOrderItem
            {
                // Id = Guid.NewGuid(), // Let DB generate if identity column
                ProductCode = i.ProductCode ?? "",       // Handle potential null from DTO if entity expects non-null
                ProductName = i.ProductName ?? "",     // Handle potential null
                Quantity = i.Quantity,                 // decimal from DTO to decimal in Entity
                UOM = i.UOM ?? "",                     // Handle potential null
                Price = i.Price,                       // decimal from DTO to decimal in Entity
                WarehouseLocation = i.WarehouseLocation ?? "", // Handle potential null
                TaxCode = i.TaxCode,                   // string? from DTO to string? in Entity
                TaxPrice = i.TaxPrice,                 // decimal? from DTO to decimal? in Entity
                Total = i.Total                        // decimal from DTO to decimal in Entity
                                                       // SalesOrderId will be set by EF Core relationship fixup or you can set it: SalesOrderId = orderId
            }).ToList();

            // Handle File Uploads
            if (dto.UploadedFiles != null && dto.UploadedFiles.Any())
            {
                string uploadBasePath = _env.WebRootPath ?? _env.ContentRootPath;
                string uploadFolderRelative = "uploads";
                string fullUploadPath = Path.Combine(uploadBasePath, uploadFolderRelative);
                Directory.CreateDirectory(fullUploadPath);

                foreach (var file in dto.UploadedFiles)
                {
                    if (file.Length > 0)
                    {
                        var clientFileName = Path.GetFileName(file.FileName);
                        var uniqueFileName = $"{Guid.NewGuid()}_{clientFileName}";
                        var physicalFilePath = Path.Combine(fullUploadPath, uniqueFileName);
                        var relativePathForDb = Path.Combine(uploadFolderRelative, uniqueFileName).Replace(Path.DirectorySeparatorChar, '/');

                        await using (var stream = new FileStream(physicalFilePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        salesOrder.Attachments.Add(new SalesOrderAttachment
                        {
                            FileName = uniqueFileName,
                            // OriginalFileName = clientFileName, // If you have this property
                            FilePath = relativePathForDb,
                            // SalesOrderId will be set by EF Core
                        });
                    }
                }
            }

            _context.SalesOrders.Add(salesOrder);
            // _context.SalesOrderNumberTrackers.Update(tracker); // EF tracks it if loaded with FindAsync

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = $"Sales order {salesOrderNo} created successfully!", id = salesOrder.Id, salesOrderNo = salesOrderNo });
            }
            catch (DbUpdateException dbEx) // Catch specific database update errors
            {
                // Log the detailed exception, including inner exceptions
                Console.Error.WriteLine($"DbUpdateException saving order: {dbEx.ToString()}");
                // Inspect dbEx.InnerException for more specific SQL errors
                var innermostException = dbEx.InnerException;
                while (innermostException?.InnerException != null)
                {
                    innermostException = innermostException.InnerException;
                }
                string errorMessage = innermostException?.Message ?? dbEx.Message;
                return StatusCode(500, new { message = "Error saving to database.", details = errorMessage });
            }
            catch (Exception ex) // General catch-all
            {
                Console.Error.WriteLine($"Generic error saving order: {ex.ToString()}");
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(Guid id, [FromForm] SalesOrderCreateDto dto)
        {
            var existingOrder = await _context.SalesOrders
                .Include(o => o.SalesItems)
                .Include(o => o.Attachments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (existingOrder == null)
            {
                return NotFound(new { message = $"Sales Order with ID {id} not found." });
            }

            // Update scalar properties
            existingOrder.CustomerCode = dto.CustomerCode;
            existingOrder.CustomerName = dto.CustomerName;
            existingOrder.SODate = dto.SODate;
            existingOrder.DeliveryDate = dto.DeliveryDate;
            existingOrder.CustomerRefNumber = dto.CustomerRefNumber;
            existingOrder.ShipToAddress = dto.ShipToAddress;
            existingOrder.SalesRemarks = dto.SalesRemarks;
            existingOrder.SalesEmployee = dto.SalesEmployee;

            // Handle SalesItems: Replace all existing items with new ones from DTO
            if (!string.IsNullOrEmpty(dto.SalesItemsJson))
            {
                try
                {
                    var newSalesItemsDto = JsonSerializer.Deserialize<List<SalesItemDto>>(dto.SalesItemsJson);

                    var itemsToRemove = existingOrder.SalesItems.ToList();
                    _context.SalesOrderItems.RemoveRange(itemsToRemove);
                    existingOrder.SalesItems.Clear();

                    if (newSalesItemsDto != null)
                    {
                        foreach (var itemDto in newSalesItemsDto)
                        {
                            existingOrder.SalesItems.Add(new SalesOrderItem
                            {
                                ProductCode = itemDto.ProductCode,
                                ProductName = itemDto.ProductName,
                                Quantity = itemDto.Quantity,
                                UOM = itemDto.UOM,
                                Price = itemDto.Price,
                                WarehouseLocation = itemDto.WarehouseLocation,
                                TaxCode = itemDto.TaxCode,
                                TaxPrice = itemDto.TaxPrice,
                                Total = itemDto.Total,
                                SalesOrderId = existingOrder.Id
                            });
                        }
                    }
                }
                catch (JsonException ex)
                {
                    return BadRequest(new { message = $"Invalid SalesItemsJson format: {ex.Message}" });
                }
            }
            else
            {
                return BadRequest(new { message = "SalesItemsJson is required for updating sales items." });
            }

            existingOrder.Attachments ??= new List<SalesOrderAttachment>();

            // Handle Attachments Deletion
            if (!string.IsNullOrEmpty(dto.FilesToDeleteJson))
            {
                try
                {
                    var fileIdsToDelete = JsonSerializer.Deserialize<List<Guid>>(dto.FilesToDeleteJson);
                    if (fileIdsToDelete != null && fileIdsToDelete.Any())
                    {
                        var attachmentsToDelete = existingOrder.Attachments
                            .Where(a => fileIdsToDelete.Contains(a.Id))
                            .ToList();

                        if (attachmentsToDelete.Any())
                        {
                            string fileStorageBasePath = _env.WebRootPath ?? _env.ContentRootPath;
                            foreach (var attachment in attachmentsToDelete)
                            {
                                if (!string.IsNullOrEmpty(attachment.FilePath))
                                {
                                    var physicalPath = Path.Combine(fileStorageBasePath, attachment.FilePath);
                                    if (System.IO.File.Exists(physicalPath))
                                    {
                                        try { System.IO.File.Delete(physicalPath); }
                                        catch (IOException ex) { Console.Error.WriteLine($"Error deleting attachment file {physicalPath}: {ex.Message}"); }
                                    }
                                }
                                existingOrder.Attachments.Remove(attachment);
                            }
                            _context.SalesOrderAttachments.RemoveRange(attachmentsToDelete);
                        }
                    }
                }
                catch (JsonException ex)
                {
                    return BadRequest(new { message = $"Invalid FilesToDeleteJson format: {ex.Message}" });
                }
            }

            // Handle New Attachments Upload
            if (dto.UploadedFiles != null && dto.UploadedFiles.Any())
            {
                string uploadBasePath = _env.WebRootPath ?? _env.ContentRootPath;
                string uploadFolderRelative = "uploads";
                string fullUploadPath = Path.Combine(uploadBasePath, uploadFolderRelative);
                Directory.CreateDirectory(fullUploadPath);

                foreach (var file in dto.UploadedFiles)
                {
                    if (file.Length > 0)
                    {
                        // Use Path.GetFileName to get just the file name part from the client
                        var clientFileName = Path.GetFileName(file.FileName);
                        var uniqueFileName = $"{Guid.NewGuid()}_{clientFileName}"; // Create a unique name for storage
                        var physicalFilePath = Path.Combine(fullUploadPath, uniqueFileName);
                        var relativePathForDb = Path.Combine(uploadFolderRelative, uniqueFileName).Replace(Path.DirectorySeparatorChar, '/');

                        await using (var stream = new FileStream(physicalFilePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        existingOrder.Attachments.Add(new SalesOrderAttachment
                        {
                            FileName = uniqueFileName, // This stores the unique name (e.g., guid_clientFileName.ext)
                                                       // No OriginalFileName property is being set here
                            FilePath = relativePathForDb,
                            SalesOrderId = existingOrder.Id
                        });
                    }
                }
            }

            // Save Changes
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Sales order updated successfully!" });
            }
            catch (DbUpdateConcurrencyException dbEx)
            {
                Console.WriteLine($"❗ DbUpdateConcurrencyException during SalesOrder Update for ID {id}. Details: {dbEx.Message}");
                var conflictingEntry = dbEx.Entries.SingleOrDefault();
                if (conflictingEntry?.Entity is SalesOrder so && so.Id == id)
                {
                    var databaseValues = await conflictingEntry.GetDatabaseValuesAsync();
                    if (databaseValues == null)
                    {
                        Console.WriteLine($"SalesOrder ID {id} appears to have been deleted from the database before save.");
                    }
                    else
                    {
                        Console.WriteLine($"Detailed concurrency conflict for SalesOrder ID {id}:");
                        foreach (var prop in conflictingEntry.Properties)
                        {
                            Console.WriteLine($"  Property: {prop.Metadata.Name}, Original: '{conflictingEntry.OriginalValues[prop.Metadata.Name]}', Database: '{databaseValues[prop.Metadata.Name]}', Current: '{conflictingEntry.CurrentValues[prop.Metadata.Name]}'");
                        }
                    }
                }
                return Conflict(new { message = "The record was modified or deleted since it was loaded. Please refresh and try again." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ Generic Exception during SalesOrder Update for ID {id}. Details: {ex.ToString()}");
                return StatusCode(500, new { message = "An unexpected error occurred during the update: " + ex.Message });
            }
        }


        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.SalesOrders
                .Include(o => o.SalesItems)
                .Include(o => o.Attachments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (existing == null) return NotFound();

            _context.SalesOrderItems.RemoveRange(existing.SalesItems);
            _context.SalesOrderAttachments.RemoveRange(existing.Attachments);
            _context.SalesOrders.Remove(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
