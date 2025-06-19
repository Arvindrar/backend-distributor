// Controllers/TaxDeclarationsController.cs
using backendDistributor.Models; // Your main models namespace
using backendDistributor.Models.Dtos; // Your DTOs namespace
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backendDistributor.Controllers // Ensure this namespace matches your project structure
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxDeclarationsController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public TaxDeclarationsController(CustomerDbContext context)
        {
            _context = context;
        }

        // Private helper for custom tax logic validation
        private bool ValidateTaxTypeLogic(decimal? cgst, decimal? sgst, decimal? igst, out string errorMessage)
        {
            errorMessage = string.Empty;
            // Treat 0 as a valid entry, but null as not entered for this specific logic
            bool cgstProvided = cgst.HasValue;
            bool sgstProvided = sgst.HasValue;
            bool igstProvided = igst.HasValue;

            if ((cgstProvided || sgstProvided) && igstProvided)
            {
                errorMessage = "Cannot provide (CGST or SGST) and IGST simultaneously. Clear one set.";
                return false;
            }

            if (cgstProvided && !sgstProvided)
            {
                errorMessage = "SGST is required when CGST is provided (or set SGST to 0 if applicable).";
                return false;
            }
            if (sgstProvided && !cgstProvided)
            {
                errorMessage = "CGST is required when SGST is provided (or set CGST to 0 if applicable).";
                return false;
            }

            if (!cgstProvided && !sgstProvided && !igstProvided)
            {
                errorMessage = "Either (CGST and SGST) or IGST must be provided.";
                return false;
            }
            return true;
        }


        // GET: api/TaxDeclarations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaxDeclarationDto>>> GetTaxDeclarations()
        {
            return await _context.TaxDeclarations
                .OrderBy(td => td.TaxCode) // Or td.Id, or any other preferred order
                .Select(td => new TaxDeclarationDto
                {
                    Id = td.Id,
                    TaxCode = td.TaxCode,
                    TaxDescription = td.TaxDescription,
                    ValidFrom = td.ValidFrom,
                    ValidTo = td.ValidTo,
                    CGST = td.CGST,
                    SGST = td.SGST,
                    IGST = td.IGST,
                    TotalPercentage = td.TotalPercentage,
                    IsActive = td.IsActive
                }).ToListAsync();
        }

        // GET: api/TaxDeclarations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaxDeclarationDto>> GetTaxDeclaration(int id)
        {
            var taxDeclaration = await _context.TaxDeclarations.FindAsync(id);

            if (taxDeclaration == null)
            {
                return NotFound($"Tax Declaration with ID {id} not found.");
            }

            return new TaxDeclarationDto
            {
                Id = taxDeclaration.Id,
                TaxCode = taxDeclaration.TaxCode,
                TaxDescription = taxDeclaration.TaxDescription,
                ValidFrom = taxDeclaration.ValidFrom,
                ValidTo = taxDeclaration.ValidTo,
                CGST = taxDeclaration.CGST,
                SGST = taxDeclaration.SGST,
                IGST = taxDeclaration.IGST,
                TotalPercentage = taxDeclaration.TotalPercentage,
                IsActive = taxDeclaration.IsActive
            };
        }

        // POST: api/TaxDeclarations
        [HttpPost]
        public async Task<ActionResult<TaxDeclarationDto>> PostTaxDeclaration(TaxDeclarationCreateDto createDto)
        {
            if (!ModelState.IsValid) // Basic DTO validation
            {
                return BadRequest(ModelState);
            }

            // Custom Validation for tax types
            if (!ValidateTaxTypeLogic(createDto.CGST, createDto.SGST, createDto.IGST, out var taxLogicError))
            {
                ModelState.AddModelError("TaxLogic", taxLogicError);
                return BadRequest(ModelState);
            }

            if (createDto.ValidTo < createDto.ValidFrom)
            {
                ModelState.AddModelError("ValidTo", "Valid To date cannot be earlier than Valid From date.");
                return BadRequest(ModelState);
            }

            // Check for duplicate TaxCode
            if (await _context.TaxDeclarations.AnyAsync(td => td.TaxCode == createDto.TaxCode))
            {
                ModelState.AddModelError("TaxCode", $"Tax Code '{createDto.TaxCode}' already exists.");
                return BadRequest(ModelState); // Using 400 for business rule violation is common
            }

            var taxDeclaration = new TaxDeclaration
            {
                TaxCode = createDto.TaxCode,
                TaxDescription = createDto.TaxDescription,
                ValidFrom = createDto.ValidFrom,
                ValidTo = createDto.ValidTo,
                CGST = createDto.CGST,
                SGST = createDto.SGST,
                IGST = createDto.IGST,
                TotalPercentage = createDto.TotalPercentage,
                IsActive = true // New entries are active by default
            };

            // Enforce tax exclusivity at entity level before saving
            if (taxDeclaration.CGST.HasValue || taxDeclaration.SGST.HasValue)
            {
                taxDeclaration.IGST = null; // Clear IGST if CGST/SGST is present
            }
            else if (taxDeclaration.IGST.HasValue)
            {
                taxDeclaration.CGST = null; // Clear CGST if IGST is present
                taxDeclaration.SGST = null; // Clear SGST if IGST is present
            }


            _context.TaxDeclarations.Add(taxDeclaration);
            await _context.SaveChangesAsync();

            // Map back to DTO for the response
            var resultDto = new TaxDeclarationDto
            {
                Id = taxDeclaration.Id,
                TaxCode = taxDeclaration.TaxCode,
                TaxDescription = taxDeclaration.TaxDescription,
                ValidFrom = taxDeclaration.ValidFrom,
                ValidTo = taxDeclaration.ValidTo,
                CGST = taxDeclaration.CGST,
                SGST = taxDeclaration.SGST,
                IGST = taxDeclaration.IGST,
                TotalPercentage = taxDeclaration.TotalPercentage,
                IsActive = taxDeclaration.IsActive
            };

            return CreatedAtAction(nameof(GetTaxDeclaration), new { id = taxDeclaration.Id }, resultDto);
        }

        // PUT: api/TaxDeclarations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTaxDeclaration(int id, TaxDeclarationUpdateDto updateDto)
        {
            if (!ModelState.IsValid) // Basic DTO validation
            {
                return BadRequest(ModelState);
            }

            var taxDeclaration = await _context.TaxDeclarations.FindAsync(id);
            if (taxDeclaration == null)
            {
                return NotFound($"Tax Declaration with ID {id} not found.");
            }

            // Custom Validation for tax types
            if (!ValidateTaxTypeLogic(updateDto.CGST, updateDto.SGST, updateDto.IGST, out var taxLogicError))
            {
                ModelState.AddModelError("TaxLogic", taxLogicError);
                return BadRequest(ModelState);
            }

            if (updateDto.ValidTo < updateDto.ValidFrom)
            {
                ModelState.AddModelError("ValidTo", "Valid To date cannot be earlier than Valid From date.");
                return BadRequest(ModelState);
            }

            // Check for duplicate TaxCode if it's being changed
            if (taxDeclaration.TaxCode != updateDto.TaxCode &&
                await _context.TaxDeclarations.AnyAsync(td => td.TaxCode == updateDto.TaxCode && td.Id != id))
            {
                ModelState.AddModelError("TaxCode", $"Tax Code '{updateDto.TaxCode}' already exists for another entry.");
                return BadRequest(ModelState);
            }

            // Update properties
            taxDeclaration.TaxCode = updateDto.TaxCode;
            taxDeclaration.TaxDescription = updateDto.TaxDescription;
            taxDeclaration.ValidFrom = updateDto.ValidFrom;
            taxDeclaration.ValidTo = updateDto.ValidTo;
            taxDeclaration.CGST = updateDto.CGST;
            taxDeclaration.SGST = updateDto.SGST;
            taxDeclaration.IGST = updateDto.IGST;
            taxDeclaration.TotalPercentage = updateDto.TotalPercentage;
            taxDeclaration.IsActive = updateDto.IsActive;

            // Enforce tax exclusivity at entity level before saving
            if (taxDeclaration.CGST.HasValue || taxDeclaration.SGST.HasValue)
            {
                taxDeclaration.IGST = null;
            }
            else if (taxDeclaration.IGST.HasValue)
            {
                taxDeclaration.CGST = null;
                taxDeclaration.SGST = null;
            }

            _context.Entry(taxDeclaration).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxDeclarationExists(id))
                {
                    return NotFound($"Tax Declaration with ID {id} not found during concurrency check.");
                }
                else
                {
                    // Log the concurrency exception or handle as per application policy
                    return Conflict("The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled.");
                }
            }
            catch (DbUpdateException ex) // Catch other potential DB update errors
            {
                Console.WriteLine($"DbUpdateException for TaxDeclaration ID {id}: {ex.InnerException?.Message ?? ex.Message}");
                // Log ex.InnerException for more details
                return StatusCode(500, "An error occurred while updating the database.");
            }


            return NoContent(); // Standard successful PUT response
        }

        // DELETE: api/TaxDeclarations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaxDeclaration(int id)
        {
            var taxDeclaration = await _context.TaxDeclarations.FindAsync(id);
            if (taxDeclaration == null)
            {
                return NotFound($"Tax Declaration with ID {id} not found.");
            }

            _context.TaxDeclarations.Remove(taxDeclaration);
            await _context.SaveChangesAsync();

            return NoContent(); // Standard successful DELETE response
        }

        private bool TaxDeclarationExists(int id)
        {
            return _context.TaxDeclarations.Any(e => e.Id == id);
        }
    }
}