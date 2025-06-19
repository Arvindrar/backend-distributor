using backendDistributor.DTOs;
using backendDistributor.Models;
using Microsoft.AspNetCore.Http; // For IFormFile, IWebHostEnvironment
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting; // For IWebHostEnvironment in .NET 6+
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace backendDistributor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesOrdersController : ControllerBase
    {
        private readonly CustomerDbContext _context;
        private readonly IWebHostEnvironment _environment; // For file uploads

        public SalesOrdersController(CustomerDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // POST: api/SalesOrders
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<SalesOrderViewDto>> CreateSalesOrder([FromForm] SalesOrderCreateDto salesOrderCreateDto)
        {
            if (string.IsNullOrEmpty(salesOrderCreateDto.SalesItemsJson))
            {
                return BadRequest(new { message = "Sales items JSON is required." });
            }

            try
            {
                var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                salesOrderCreateDto.ParsedSalesItems = JsonSerializer.Deserialize<List<SalesOrderItemDto>>(salesOrderCreateDto.SalesItemsJson, jsonSerializerOptions);

                if (salesOrderCreateDto.ParsedSalesItems == null || !salesOrderCreateDto.ParsedSalesItems.Any())
                {
                    return BadRequest(new { message = "Sales items list cannot be empty after parsing." });
                }
            }
            catch (JsonException ex)
            {
                return BadRequest(new { message = "Invalid format for sales items JSON.", details = ex.Message });
            }

            var salesOrder = new SalesOrder // This is your EF Model instance
            {
                Id = Guid.NewGuid(),
                SalesOrderNo = salesOrderCreateDto.SalesOrderNo,
                CustomerCode = salesOrderCreateDto.CustomerCode,
                CustomerName = salesOrderCreateDto.CustomerName,
                CustomerRefNumber = salesOrderCreateDto.CustomerRefNumber,
                ShipToAddress = salesOrderCreateDto.ShipToAddress,
                SalesRemarks = salesOrderCreateDto.SalesRemarks,
                SalesEmployee = salesOrderCreateDto.SalesEmployee,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            if (DateTime.TryParse(salesOrderCreateDto.SODate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var soDate))
                salesOrder.SODate = soDate;
            if (DateTime.TryParse(salesOrderCreateDto.DeliveryDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var deliveryDate))
                salesOrder.DeliveryDate = deliveryDate;

            if (!string.IsNullOrEmpty(salesOrder.CustomerCode))
            {
                var customer = await _context.Customer.FirstOrDefaultAsync(c => c.Code == salesOrder.CustomerCode);
                if (customer != null) salesOrder.CustomerId = customer.Id;
            }

            foreach (var itemDto in salesOrderCreateDto.ParsedSalesItems)
            {
                salesOrder.SalesOrderItems.Add(new SalesOrderItem // This is your EF Model SalesOrderItem
                {
                    Id = Guid.NewGuid(),
                    SalesOrderId = salesOrder.Id,
                    ProductCode = itemDto.ProductCode,
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    UOM = itemDto.UOM,
                    Price = itemDto.Price,
                    WarehouseLocation = itemDto.WarehouseLocation,
                    TaxCode = itemDto.TaxCode,
                    TaxPrice = itemDto.TaxPrice,
                    Total = itemDto.Total // This is item.Quantity * item.Price + item.TaxPrice
                });
            }

            await ProcessFileUploads(salesOrderCreateDto.UploadedFiles, salesOrder);

            _context.SalesOrders.Add(salesOrder);
            await _context.SaveChangesAsync();

            // After saving, 'salesOrder' (EF model) has its items.
            // Now, map this EF model 'salesOrder' to 'SalesOrderViewDto' for the response.
            var viewDto = MapSalesOrderToViewDto(salesOrder);
            return CreatedAtAction(nameof(GetSalesOrder), new { id = salesOrder.Id }, viewDto);
        }

        // GET: api/SalesOrders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesOrderViewDto>>> GetSalesOrders(
            [FromQuery] string? salesOrderNo,
            [FromQuery] string? customerName)
        {
            var query = _context.SalesOrders
                .Include(so => so.SalesOrderItems) // Crucial: Include items to calculate total
                .Include(so => so.Attachments)
                .AsQueryable();

            if (!string.IsNullOrEmpty(salesOrderNo))
            {
                query = query.Where(so => so.SalesOrderNo != null && so.SalesOrderNo.ToLower().Contains(salesOrderNo.ToLower()));
            }
            if (!string.IsNullOrEmpty(customerName))
            {
                query = query.Where(so => so.CustomerName != null && so.CustomerName.ToLower().Contains(customerName.ToLower()));
            }

            // The .Select() will project each 'SalesOrder' entity to a 'SalesOrderViewDto'
            // using our MapSalesOrderToViewDto method.
            var salesOrders = await query
                .OrderByDescending(so => so.CreatedDate)
                .Select(so => MapSalesOrderToViewDto(so)) // MapSalesOrderToViewDto is called for each 'so'
                .ToListAsync();

            return Ok(salesOrders);
        }

        // GET: api/SalesOrders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SalesOrderViewDto>> GetSalesOrder(Guid id)
        {
            // Use .Select directly to map to DTO, or fetch entity then map
            var salesOrderEntity = await _context.SalesOrders
                .Include(so => so.SalesOrderItems) // Crucial: Include items to calculate total
                .Include(so => so.Attachments)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (salesOrderEntity == null)
            {
                return NotFound();
            }

            var viewDto = MapSalesOrderToViewDto(salesOrderEntity); // Map the fetched entity
            return Ok(viewDto);
        }

        // PUT: api/SalesOrders/{id}
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateSalesOrder(Guid id, [FromForm] SalesOrderUpdateDto salesOrderUpdateDto)
        {
            var salesOrderToUpdate = await _context.SalesOrders
                                .Include(so => so.SalesOrderItems)
                                .Include(so => so.Attachments)
                                .FirstOrDefaultAsync(so => so.Id == id);

            if (salesOrderToUpdate == null)
            {
                return NotFound(new { message = $"Sales Order with ID {id} not found." });
            }

            salesOrderToUpdate.SalesOrderNo = salesOrderUpdateDto.SalesOrderNo ?? salesOrderToUpdate.SalesOrderNo;
            salesOrderToUpdate.CustomerCode = salesOrderUpdateDto.CustomerCode ?? salesOrderToUpdate.CustomerCode;
            salesOrderToUpdate.CustomerName = salesOrderUpdateDto.CustomerName ?? salesOrderToUpdate.CustomerName;
            salesOrderToUpdate.CustomerRefNumber = salesOrderUpdateDto.CustomerRefNumber ?? salesOrderToUpdate.CustomerRefNumber;
            salesOrderToUpdate.ShipToAddress = salesOrderUpdateDto.ShipToAddress ?? salesOrderToUpdate.ShipToAddress;
            salesOrderToUpdate.SalesRemarks = salesOrderUpdateDto.SalesRemarks ?? salesOrderToUpdate.SalesRemarks;
            salesOrderToUpdate.SalesEmployee = salesOrderUpdateDto.SalesEmployee ?? salesOrderToUpdate.SalesEmployee;
            salesOrderToUpdate.ModifiedDate = DateTime.UtcNow;

            if (DateTime.TryParse(salesOrderUpdateDto.SODate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var soDate))
                salesOrderToUpdate.SODate = soDate;
            if (DateTime.TryParse(salesOrderUpdateDto.DeliveryDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var deliveryDate))
                salesOrderToUpdate.DeliveryDate = deliveryDate;

            if (!string.IsNullOrEmpty(salesOrderUpdateDto.CustomerCode) && salesOrderUpdateDto.CustomerCode != salesOrderToUpdate.CustomerCode)
            {
                var customer = await _context.Customer.FirstOrDefaultAsync(c => c.Code == salesOrderUpdateDto.CustomerCode);
                salesOrderToUpdate.CustomerId = customer?.Id;
                if (customer != null && string.IsNullOrEmpty(salesOrderUpdateDto.CustomerName))
                {
                    salesOrderToUpdate.CustomerName = customer.Name;
                }
            }

            if (!string.IsNullOrEmpty(salesOrderUpdateDto.SalesItemsJson))
            {
                try
                {
                    var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    salesOrderUpdateDto.ParsedSalesItems = JsonSerializer.Deserialize<List<SalesOrderItemDto>>(salesOrderUpdateDto.SalesItemsJson, jsonSerializerOptions);

                    if (salesOrderUpdateDto.ParsedSalesItems != null)
                    {
                        _context.SalesOrderItems.RemoveRange(salesOrderToUpdate.SalesOrderItems);
                        salesOrderToUpdate.SalesOrderItems.Clear();

                        foreach (var itemDto in salesOrderUpdateDto.ParsedSalesItems)
                        {
                            salesOrderToUpdate.SalesOrderItems.Add(new SalesOrderItem
                            {
                                Id = Guid.NewGuid(), // Or handle existing item IDs if you want to update items
                                SalesOrderId = salesOrderToUpdate.Id,
                                ProductCode = itemDto.ProductCode,
                                ProductName = itemDto.ProductName,
                                Quantity = itemDto.Quantity,
                                UOM = itemDto.UOM,
                                Price = itemDto.Price,
                                WarehouseLocation = itemDto.WarehouseLocation,
                                TaxCode = itemDto.TaxCode,
                                TaxPrice = itemDto.TaxPrice,
                                Total = itemDto.Total
                            });
                        }
                    }
                }
                catch (JsonException ex)
                {
                    return BadRequest(new { message = "Invalid format for sales items JSON.", details = ex.Message });
                }
            }

            if (salesOrderUpdateDto.FilesToDelete != null && salesOrderUpdateDto.FilesToDelete.Any())
            {
                var attachmentsToDelete = salesOrderToUpdate.Attachments
                                            .Where(att => salesOrderUpdateDto.FilesToDelete.Contains(att.Id))
                                            .ToList();
                foreach (var att in attachmentsToDelete)
                {
                    if (!string.IsNullOrEmpty(att.StoredFileName))
                    {
                        var filePath = Path.Combine(_environment.ContentRootPath, "Uploads", "SalesAttachments", att.StoredFileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    _context.SalesOrderAttachments.Remove(att);
                }
            }
            await ProcessFileUploads(salesOrderUpdateDto.UploadedFiles, salesOrderToUpdate);

            _context.Entry(salesOrderToUpdate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalesOrderExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        // DELETE: api/SalesOrders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSalesOrder(Guid id)
        {
            var salesOrder = await _context.SalesOrders
                                    .Include(so => so.Attachments) // Include attachments to delete files
                                    .FirstOrDefaultAsync(so => so.Id == id);
            if (salesOrder == null)
            {
                return NotFound(new { message = $"Sales Order with ID {id} not found." });
            }

            foreach (var att in salesOrder.Attachments)
            {
                if (!string.IsNullOrEmpty(att.StoredFileName))
                {
                    var filePath = Path.Combine(_environment.ContentRootPath, "Uploads", "SalesAttachments", att.StoredFileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            // EF Core cascade delete should handle SalesOrderItems and SalesOrderAttachment records
            // if configured correctly in your DbContext relationship.
            // If not, you might need to explicitly remove them:
            // _context.SalesOrderItems.RemoveRange(salesOrder.SalesOrderItems);
            // _context.SalesOrderAttachments.RemoveRange(salesOrder.Attachments);
            _context.SalesOrders.Remove(salesOrder);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Sales Order with ID {id} and its items/attachments have been deleted." });
        }

        // GET: api/SalesOrders/attachment/{attachmentId}
        [HttpGet("attachment/{attachmentId}")]
        public async Task<IActionResult> DownloadAttachment(Guid attachmentId)
        {
            var attachment = await _context.SalesOrderAttachments.FindAsync(attachmentId);
            if (attachment == null) return NotFound("Attachment not found.");

            var contentType = attachment.ContentType ?? "application/octet-stream";
            var clientFileName = attachment.FileName ?? "downloadedFile";

            if (string.IsNullOrEmpty(attachment.StoredFileName))
            {
                return NotFound("Attachment metadata is incomplete (missing stored file name).");
            }

            var uploadsFolderPath = Path.Combine(_environment.ContentRootPath, "Uploads", "SalesAttachments");
            var filePath = Path.Combine(uploadsFolderPath, attachment.StoredFileName);

            if (!System.IO.File.Exists(filePath)) return NotFound("File not found on server.");

            var memory = new MemoryStream();
            await using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, contentType, clientFileName);
        }

        private bool SalesOrderExists(Guid id)
        {
            return _context.SalesOrders.Any(e => e.Id == id);
        }

        private async Task ProcessFileUploads(List<IFormFile>? files, SalesOrder salesOrder)
        {
            if (files != null && files.Any())
            {
                var uploadsFolderPath = Path.Combine(_environment.ContentRootPath, "Uploads", "SalesAttachments");
                if (!Directory.Exists(uploadsFolderPath))
                {
                    Directory.CreateDirectory(uploadsFolderPath);
                }

                foreach (var file in files)
                {
                    if (file.Length > 0 && !string.IsNullOrEmpty(file.FileName))
                    {
                        var originalFileName = file.FileName;
                        var extension = Path.GetExtension(originalFileName);
                        var uniqueStoredFileName = Guid.NewGuid().ToString() + (string.IsNullOrEmpty(extension) ? "" : extension);
                        var filePath = Path.Combine(uploadsFolderPath, uniqueStoredFileName);

                        await using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var attachment = new SalesOrderAttachment
                        {
                            Id = Guid.NewGuid(),
                            SalesOrder = salesOrder,
                            FileName = originalFileName,
                            StoredFileName = uniqueStoredFileName,
                            ContentType = file.ContentType ?? "application/octet-stream",
                            FileSize = file.Length,
                            UploadedDate = DateTime.UtcNow
                        };
                        salesOrder.Attachments.Add(attachment);
                    }
                }
            }
        }

        // --- THIS IS THE MODIFIED METHOD ---
        // Made static to resolve client projection issue
        private static SalesOrderViewDto MapSalesOrderToViewDto(SalesOrder so) // 'so' is your EF Model 'SalesOrder'
        {
            // 1. Map the SalesOrderItems (from the EF Model 'so') to SalesOrderItemViewDto
            var mappedItems = so.SalesOrderItems?.Select(item => new SalesOrderItemViewDto
            {
                Id = item.Id,
                ProductCode = item.ProductCode,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UOM = item.UOM,
                Price = item.Price,
                WarehouseLocation = item.WarehouseLocation,
                TaxCode = item.TaxCode,
                TaxPrice = item.TaxPrice,
                Total = item.Total // This 'Total' is from the SalesOrderItem EF Model
            }).ToList() ?? new List<SalesOrderItemViewDto>(); // Ensure mappedItems is never null

            // 2. Create the SalesOrderViewDto and populate its properties
            return new SalesOrderViewDto
            {
                Id = so.Id,
                SalesOrderNo = so.SalesOrderNo,
                CustomerCode = so.CustomerCode,
                CustomerName = so.CustomerName,
                SODate = so.SODate,
                DeliveryDate = so.DeliveryDate,
                CustomerRefNumber = so.CustomerRefNumber,
                ShipToAddress = so.ShipToAddress,
                SalesRemarks = so.SalesRemarks,
                SalesEmployee = so.SalesEmployee,
                CreatedDate = so.CreatedDate,

                // Calculate OrderTotal by summing the 'Total' of all 'mappedItems'
                OrderTotal = mappedItems.Sum(item => item.Total),

                SalesOrderItems = mappedItems, // Assign the list of already mapped items

                Attachments = so.Attachments?.Select(att => new SalesOrderAttachmentViewDto
                {
                    Id = att.Id,
                    FileName = att.FileName,
                    ContentType = att.ContentType,
                    FileSize = att.FileSize,
                    DownloadUrl = $"/api/SalesOrders/attachment/{att.Id}"
                }).ToList() ?? new List<SalesOrderAttachmentViewDto>()
            };
        }
        // --- END OF MODIFIED METHOD ---
    }
}