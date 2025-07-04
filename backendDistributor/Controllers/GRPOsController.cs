using backendDistributor.DTOs;
using backendDistributor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace backendDistributor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GRPOsController : ControllerBase
    {
        private readonly CustomerDbContext _context;
        private readonly IWebHostEnvironment _env;

        public GRPOsController(CustomerDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/GRPOs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GRPOListDto>>> GetGRPOs(
            [FromQuery] string? grpoNo,
            [FromQuery] string? vendorName,
            [FromQuery] string? purchaseOrderNo) // Added for future use
        {
            var query = _context.GRPOs.AsQueryable();

            // Apply filter for GRPO Number if provided
            if (!string.IsNullOrEmpty(grpoNo))
            {
                query = query.Where(g => g.GRPONo.Contains(grpoNo));
            }

            // Apply filter for Vendor Name if provided
            if (!string.IsNullOrEmpty(vendorName))
            {
                query = query.Where(g => g.VendorName.Contains(vendorName));
            }

            // Apply filter for original Purchase Order Number if provided
            if (!string.IsNullOrEmpty(purchaseOrderNo))
            {
                query = query.Where(g => g.PurchaseOrderNo.Contains(purchaseOrderNo));
            }

            var grpos = await query
                .Include(g => g.GRPOItems)
                .Select(grpo => new GRPOListDto
                {
                    Id = grpo.Id,
                    GRPONo = grpo.GRPONo,
                    PurchaseOrderNo = grpo.PurchaseOrderNo,
                    VendorName = grpo.VendorName,
                    VendorCode = grpo.VendorCode,
                    GRPODate = grpo.GRPODate,
                    GRPORemarks = grpo.GRPORemarks,
                    GRPOTotal = grpo.GRPOItems.Sum(i => i.Total)
                })
                .OrderByDescending(g => g.GRPODate)
                .ToListAsync();

            return Ok(grpos);
        }
        // GET: api/GRPOs/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetGRPOById(Guid id)
        {
            var grpo = await _context.GRPOs
                .Include(g => g.GRPOItems)
                .Include(g => g.Attachments)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grpo == null)
            {
                return NotFound(new { message = $"GRPO with ID {id} not found." });
            }

            // Return a detailed object, similar to the Purchase Order GET by ID
            return Ok(grpo);
        }

        // POST: api/GRPOs
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> CreateGRPO([FromForm] GRPOCreateDto dto)
        {
            if (Request.Form.TryGetValue("GRPOItemsJson", out var itemsJsonString) && !string.IsNullOrEmpty(itemsJsonString))
            {
                dto.GRPOItems = JsonSerializer.Deserialize<List<GRPOItemDto>>(itemsJsonString.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            if (dto.GRPOItems == null || !dto.GRPOItems.Any())
            {
                return BadRequest(new { message = "At least one GRPO item is required." });
            }

            // Use the GRPO-specific number tracker
            var tracker = await _context.GRPONumberTrackers.FindAsync(2) ?? new GRPONumberTracker { Id = 2, LastUsedNumber = 1000000 };
            if (_context.Entry(tracker).State == EntityState.Detached) _context.GRPONumberTrackers.Add(tracker);

            tracker.LastUsedNumber++;
            string newGrpoNumber = $"GRPO-{tracker.LastUsedNumber}";

            var grpo = new GRPO
            {
                Id = Guid.NewGuid(),
                GRPONo = newGrpoNumber,
                PurchaseOrderNo = dto.PurchaseOrderNo,
                VendorCode = dto.VendorCode,
                VendorName = dto.VendorName,
                GRPODate = dto.GRPODate,
                DeliveryDate = dto.DeliveryDate,
                VendorRefNumber = dto.VendorRefNumber,
                ShipToAddress = dto.ShipToAddress,
                GRPORemarks = dto.GRPORemarks,
                Attachments = new List<GRPOAttachment>(),
                GRPOItems = dto.GRPOItems.Select(i => new GRPOItem
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
                }).ToList()
            };

            if (dto.UploadedFiles != null)
            {
                // Use a GRPO-specific upload folder
                string uploadBasePath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                string uploadFolder = Path.Combine(uploadBasePath, "uploads", "grpos");
                Directory.CreateDirectory(uploadFolder);

                foreach (var file in dto.UploadedFiles)
                {
                    var clientFileName = Path.GetFileName(file.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{clientFileName}";
                    var physicalPath = Path.Combine(uploadFolder, uniqueFileName);
                    var relativePath = Path.Combine("uploads", "grpos", uniqueFileName).Replace(Path.DirectorySeparatorChar, '/');

                    await using var stream = new FileStream(physicalPath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    grpo.Attachments.Add(new GRPOAttachment
                    {
                        FileName = clientFileName,
                        FilePath = relativePath,
                    });
                }
            }

            _context.GRPOs.Add(grpo);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"GRPO {newGrpoNumber} created successfully!", id = grpo.Id, grpoNo = newGrpoNumber });
        }

        // PUT: api/GRPOs/{id}
        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateGRPO(Guid id, [FromForm] GRPOCreateDto dto)
        {
            var existingGRPO = await _context.GRPOs
                .Include(p => p.GRPOItems)
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingGRPO == null)
            {
                return NotFound(new { message = $"GRPO with ID {id} not found." });
            }

            // Update scalar properties
            existingGRPO.PurchaseOrderNo = dto.PurchaseOrderNo;
            existingGRPO.VendorCode = dto.VendorCode;
            existingGRPO.VendorName = dto.VendorName;
            existingGRPO.GRPODate = dto.GRPODate;
            existingGRPO.DeliveryDate = dto.DeliveryDate;
            existingGRPO.VendorRefNumber = dto.VendorRefNumber;
            existingGRPO.ShipToAddress = dto.ShipToAddress;
            existingGRPO.GRPORemarks = dto.GRPORemarks;

            // Update items
            if (!string.IsNullOrEmpty(dto.GRPOItemsJson))
            {
                var newItemsDto = JsonSerializer.Deserialize<List<GRPOItemDto>>(dto.GRPOItemsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _context.GRPOItems.RemoveRange(existingGRPO.GRPOItems);
                existingGRPO.GRPOItems = newItemsDto.Select(i => new GRPOItem
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
            }

            // Handle file deletions and uploads (similar logic to Purchase Order update)
            // ...
            // 3. Handle Attachment Deletions
            if (!string.IsNullOrEmpty(dto.FilesToDeleteJson))
            {
                var fileIdsToDelete = JsonSerializer.Deserialize<List<Guid>>(dto.FilesToDeleteJson);
                var attachmentsToDelete = existingGRPO.Attachments.Where(a => fileIdsToDelete.Contains(a.Id)).ToList();

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
                    _context.GRPOAttachments.RemoveRange(attachmentsToDelete);
                }
            }

            // 4. Handle New Attachment Uploads
            if (dto.UploadedFiles != null && dto.UploadedFiles.Any())
            {
                string uploadBasePath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                string uploadFolder = Path.Combine(uploadBasePath, "uploads", "grpos");
                Directory.CreateDirectory(uploadFolder);

                foreach (var file in dto.UploadedFiles)
                {
                    var clientFileName = Path.GetFileName(file.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{clientFileName}";
                    var physicalPath = Path.Combine(uploadFolder, uniqueFileName);
                    var relativePath = Path.Combine("uploads", "grpos", uniqueFileName).Replace(Path.DirectorySeparatorChar, '/');

                    await using var stream = new FileStream(physicalPath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    existingGRPO.Attachments.Add(new GRPOAttachment
                    {
                        FileName = clientFileName,
                        FilePath = relativePath,
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "GRPO updated successfully!" });
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

        // DELETE: api/GRPOs/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteGRPO(Guid id)
        {
            var existingGRPO = await _context.GRPOs
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingGRPO == null)
            {
                return NotFound();
            }

            // Delete physical files
            if (existingGRPO.Attachments.Any())
            {
                string fileStorageBasePath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                foreach (var att in existingGRPO.Attachments)
                {
                    if (!string.IsNullOrEmpty(att.FilePath))
                    {
                        var physicalPath = Path.Combine(fileStorageBasePath, att.FilePath);
                        if (System.IO.File.Exists(physicalPath))
                        {
                            try { System.IO.File.Delete(physicalPath); }
                            catch (IOException ex) { Console.Error.WriteLine($"Error deleting file on GRPO delete {physicalPath}: {ex.Message}"); }
                        }
                    }
                }
            }

            _context.GRPOs.Remove(existingGRPO);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}