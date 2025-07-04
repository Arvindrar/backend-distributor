// FILE: Controllers/PurchaseOrdersController.cs

using backendDistributor.DTOs;
using backendDistributor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace backendDistributor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly CustomerDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PurchaseOrdersController(CustomerDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/PurchaseOrders (For List View)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseOrderListDto>>> GetPurchaseOrders(
            [FromQuery] string? purchaseOrderNo,
            [FromQuery] string? vendorName)
        {
            var query = _context.PurchaseOrders.AsQueryable();

            if (!string.IsNullOrEmpty(purchaseOrderNo))
            {
                query = query.Where(o => o.PurchaseOrderNo.Contains(purchaseOrderNo));
            }

            if (!string.IsNullOrEmpty(vendorName))
            {
                query = query.Where(o => o.VendorName.Contains(vendorName));
            }

            var orders = await query
                .Include(o => o.PurchaseItems) // Included to calculate the total
                .Select(order => new PurchaseOrderListDto
                {
                    Id = order.Id,
                    PurchaseOrderNo = order.PurchaseOrderNo,
                    VendorCode = order.VendorCode,
                    VendorName = order.VendorName,
                    PODate = order.PODate,
                    PurchaseRemarks = order.PurchaseRemarks,
                    OrderTotal = order.PurchaseItems.Sum(i => i.Total)
                })
                .OrderByDescending(o => o.PODate)
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/PurchaseOrders/{id} (For Detail/Edit View)
        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetById(Guid id)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.PurchaseItems)
                .Include(po => po.Attachments)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null)
            {
                return NotFound(new { message = $"Purchase Order with ID {id} not found." });
            }

            var result = new
            {
                order.Id,
                order.PurchaseOrderNo,
                order.VendorCode,
                order.VendorName,
                order.PODate,
                order.DeliveryDate,
                order.VendorRefNumber,
                order.ShipToAddress,
                order.PurchaseRemarks,
                purchaseItems = order.PurchaseItems.Select(i => new
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
                    a.PurchaseOrderId
                }).ToList()
            };

            return Ok(result);
        }

        // POST: api/PurchaseOrders (Create)
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> Create([FromForm] PurchaseOrderCreateDto dto)
        {
            if (Request.Form.TryGetValue("PurchaseItemsJson", out var itemsJsonString) && !string.IsNullOrEmpty(itemsJsonString))
            {
                dto.PurchaseItems = JsonSerializer.Deserialize<List<PurchaseItemDto>>(itemsJsonString.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            if (dto.PurchaseItems == null || !dto.PurchaseItems.Any())
            {
                return BadRequest(new { message = "At least one purchase item is required." });
            }

            var tracker = await _context.PurchaseOrderNumberTrackers.FindAsync(1) ?? new PurchaseOrderNumberTracker { Id = 1, LastUsedNumber = 2000000 };
            if (_context.Entry(tracker).State == EntityState.Detached) _context.PurchaseOrderNumberTrackers.Add(tracker);

            tracker.LastUsedNumber++;
            string newPoNumber = $"PO-{tracker.LastUsedNumber}";

            var purchaseOrder = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                PurchaseOrderNo = newPoNumber,
                VendorCode = dto.VendorCode,
                VendorName = dto.VendorName,
                PODate = dto.PODate,
                DeliveryDate = dto.DeliveryDate,
                VendorRefNumber = dto.VendorRefNumber,
                ShipToAddress = dto.ShipToAddress,
                PurchaseRemarks = dto.PurchaseRemarks,
                Attachments = new List<PurchaseOrderAttachment>(),
                PurchaseItems = dto.PurchaseItems.Select(i => new PurchaseOrderItem { /* mapping... */ }).ToList()
            };

            purchaseOrder.PurchaseItems = dto.PurchaseItems.Select(i => new PurchaseOrderItem
            {
                ProductCode = i.ProductCode,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UOM = i.UOM,
                Price = i.Price,
                WarehouseLocation = i.WarehouseLocation,
                TaxCode = i.TaxCode,
                TaxPrice = i.TaxPrice,
                Total = i.Total
            }).ToList();


            if (dto.UploadedFiles != null)
            {
                string uploadBasePath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                string uploadFolder = Path.Combine(uploadBasePath, "uploads", "purchase_orders");
                Directory.CreateDirectory(uploadFolder);

                foreach (var file in dto.UploadedFiles)
                {
                    var clientFileName = Path.GetFileName(file.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{clientFileName}";
                    var physicalPath = Path.Combine(uploadFolder, uniqueFileName);
                    var relativePath = Path.Combine("uploads", "purchase_orders", uniqueFileName).Replace(Path.DirectorySeparatorChar, '/');

                    await using var stream = new FileStream(physicalPath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    purchaseOrder.Attachments.Add(new PurchaseOrderAttachment
                    {
                        FileName = clientFileName,
                        FilePath = relativePath,
                    });
                }
            }

            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Purchase Order {newPoNumber} created successfully!", id = purchaseOrder.Id, purchaseOrderNo = newPoNumber });
        }

        // PUT: api/PurchaseOrders/{id} (Update)
        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(Guid id, [FromForm] PurchaseOrderCreateDto dto)
        {
            var existingOrder = await _context.PurchaseOrders
                .Include(p => p.PurchaseItems)
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingOrder == null)
            {
                return NotFound(new { message = $"Purchase Order with ID {id} not found." });
            }

            // 1. Update Scalar Properties
            existingOrder.VendorCode = dto.VendorCode;
            existingOrder.VendorName = dto.VendorName;
            existingOrder.PODate = dto.PODate;
            existingOrder.DeliveryDate = dto.DeliveryDate;
            existingOrder.VendorRefNumber = dto.VendorRefNumber;
            existingOrder.ShipToAddress = dto.ShipToAddress;
            existingOrder.PurchaseRemarks = dto.PurchaseRemarks;

            // 2. Update Items (Remove and Replace strategy)
            if (!string.IsNullOrEmpty(dto.PurchaseItemsJson))
            {
                var newItemsDto = JsonSerializer.Deserialize<List<PurchaseItemDto>>(dto.PurchaseItemsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _context.PurchaseOrderItems.RemoveRange(existingOrder.PurchaseItems); // Remove old items
                existingOrder.PurchaseItems = newItemsDto.Select(i => new PurchaseOrderItem
                {
                    ProductCode = i.ProductCode,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UOM = i.UOM,
                    Price = i.Price,
                    WarehouseLocation = i.WarehouseLocation,
                    TaxCode = i.TaxCode,
                    TaxPrice = i.TaxPrice,
                    Total = i.Total
                }).ToList(); // Add new items
            }

            // 3. Handle Attachment Deletions
            if (!string.IsNullOrEmpty(dto.FilesToDeleteJson))
            {
                var fileIdsToDelete = JsonSerializer.Deserialize<List<Guid>>(dto.FilesToDeleteJson);
                var attachmentsToDelete = existingOrder.Attachments.Where(a => fileIdsToDelete.Contains(a.Id)).ToList();
                if (attachmentsToDelete.Any())
                {
                    string fileStorageBasePath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                    foreach (var att in attachmentsToDelete)
                    {
                        var physicalPath = Path.Combine(fileStorageBasePath, att.FilePath);
                        if (System.IO.File.Exists(physicalPath))
                        {
                            try { System.IO.File.Delete(physicalPath); }
                            catch (IOException ex) { Console.Error.WriteLine($"Error deleting file {physicalPath}: {ex.Message}"); }
                        }
                    }
                    _context.PurchaseOrderAttachments.RemoveRange(attachmentsToDelete);
                }
            }

            // 4. Handle New Attachment Uploads
            if (dto.UploadedFiles != null && dto.UploadedFiles.Any())
            {
                // This logic is identical to the Create method
                string uploadBasePath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                string uploadFolder = Path.Combine(uploadBasePath, "uploads", "purchase_orders");
                Directory.CreateDirectory(uploadFolder);

                foreach (var file in dto.UploadedFiles)
                {
                    var clientFileName = Path.GetFileName(file.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{clientFileName}";
                    var physicalPath = Path.Combine(uploadFolder, uniqueFileName);
                    var relativePath = Path.Combine("uploads", "purchase_orders", uniqueFileName).Replace(Path.DirectorySeparatorChar, '/');

                    await using var stream = new FileStream(physicalPath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    existingOrder.Attachments.Add(new PurchaseOrderAttachment
                    {
                        FileName = clientFileName,
                        FilePath = relativePath,
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Purchase order updated successfully!" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "The record was modified by another user. Please refresh and try again." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during the update: " + ex.Message });
            }
        }

        // DELETE: api/PurchaseOrders/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingOrder = await _context.PurchaseOrders
                .Include(p => p.PurchaseItems)
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingOrder == null)
            {
                return NotFound();
            }

            // Delete associated physical files first
            if (existingOrder.Attachments.Any())
            {
                string fileStorageBasePath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                foreach (var att in existingOrder.Attachments)
                {
                    var physicalPath = Path.Combine(fileStorageBasePath, att.FilePath);
                    if (System.IO.File.Exists(physicalPath))
                    {
                        try { System.IO.File.Delete(physicalPath); }
                        catch (IOException ex) { Console.Error.WriteLine($"Error deleting file on PO delete {physicalPath}: {ex.Message}"); }
                    }
                }
            }

            // EF Core will handle cascading deletes for child tables if configured,
            // but being explicit is safer.
            _context.PurchaseOrderItems.RemoveRange(existingOrder.PurchaseItems);
            _context.PurchaseOrderAttachments.RemoveRange(existingOrder.Attachments);
            _context.PurchaseOrders.Remove(existingOrder);

            await _context.SaveChangesAsync();

            return NoContent(); // Standard response for successful deletion
        }
    }
}