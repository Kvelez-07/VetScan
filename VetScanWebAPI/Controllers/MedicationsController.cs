// Controllers/MedicationsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicationsController> _logger;

        public MedicationsController(ApplicationDbContext context, ILogger<MedicationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Medications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicationDto>>> GetMedications()
        {
            try
            {
                var medications = await _context.Medications
                    .Where(m => m.IsActive)
                    .Select(m => new MedicationDto
                    {
                        MedicationId = m.MedicationId,
                        MedicationName = m.MedicationName,
                        GenericName = m.GenericName,
                        Manufacturer = m.Manufacturer,
                        Concentration = m.Concentration,
                        Category = m.Category,
                        IsActive = m.IsActive,
                        CreatedDate = m.CreatedDate
                    })
                    .ToListAsync();

                return Ok(medications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener medicamentos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Medications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicationDto>> GetMedication(int id)
        {
            try
            {
                var medication = await _context.Medications
                    .FirstOrDefaultAsync(m => m.MedicationId == id && m.IsActive);

                if (medication == null)
                {
                    return NotFound();
                }

                var medicationDto = new MedicationDto
                {
                    MedicationId = medication.MedicationId,
                    MedicationName = medication.MedicationName,
                    GenericName = medication.GenericName,
                    Manufacturer = medication.Manufacturer,
                    Concentration = medication.Concentration,
                    Category = medication.Category,
                    IsActive = medication.IsActive,
                    CreatedDate = medication.CreatedDate
                };

                return Ok(medicationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener medicamento con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Medications
        [HttpPost]
        public async Task<ActionResult<MedicationDto>> PostMedication(MedicationFormDto medicationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if medication with same name already exists
                if (await _context.Medications.AnyAsync(m =>
                    m.MedicationName == medicationDto.MedicationName))
                {
                    return Conflict("Ya existe un medicamento con ese nombre");
                }

                var medication = new Medication
                {
                    MedicationName = medicationDto.MedicationName,
                    GenericName = medicationDto.GenericName,
                    Manufacturer = medicationDto.Manufacturer,
                    Concentration = medicationDto.Concentration,
                    Category = medicationDto.Category,
                    IsActive = medicationDto.IsActive,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Medications.Add(medication);
                await _context.SaveChangesAsync();

                var resultDto = new MedicationDto
                {
                    MedicationId = medication.MedicationId,
                    MedicationName = medication.MedicationName,
                    GenericName = medication.GenericName,
                    Manufacturer = medication.Manufacturer,
                    Concentration = medication.Concentration,
                    Category = medication.Category,
                    IsActive = medication.IsActive,
                    CreatedDate = medication.CreatedDate
                };

                return CreatedAtAction(nameof(GetMedication), new { id = medication.MedicationId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear medicamento");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Medications/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMedication(int id, MedicationFormDto medicationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de medicamento inválido");
                }

                var medication = await _context.Medications.FindAsync(id);
                if (medication == null)
                {
                    return NotFound();
                }

                // Check if another medication has the same name
                if (await _context.Medications.AnyAsync(m =>
                    m.MedicationName == medicationDto.MedicationName &&
                    m.MedicationId != id))
                {
                    return Conflict("Ya existe otro medicamento con ese nombre");
                }

                medication.MedicationName = medicationDto.MedicationName;
                medication.GenericName = medicationDto.GenericName;
                medication.Manufacturer = medicationDto.Manufacturer;
                medication.Concentration = medicationDto.Concentration;
                medication.Category = medicationDto.Category;
                medication.IsActive = medicationDto.IsActive;

                _context.Entry(medication).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicationExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar medicamento con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Medications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedication(int id)
        {
            try
            {
                var medication = await _context.Medications.FindAsync(id);
                if (medication == null)
                {
                    return NotFound();
                }

                // Check if there are associated prescriptions
                var hasPrescriptions = await _context.Prescriptions
                    .AnyAsync(p => p.MedicationId == id);

                if (hasPrescriptions)
                {
                    return BadRequest("No se puede eliminar el medicamento porque tiene prescripciones asociadas");
                }

                // Soft delete (mark as inactive)
                medication.IsActive = false;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar medicamento con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool MedicationExists(int id)
        {
            return _context.Medications.Any(e => e.MedicationId == id);
        }
    }
}