// Controllers/MedicalRecordsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicalRecordsController> _logger;

        public MedicalRecordsController(ApplicationDbContext context, ILogger<MedicalRecordsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/MedicalRecords
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicalRecordDto>>> GetMedicalRecords()
        {
            try
            {
                var medicalRecords = await _context.MedicalRecords
                    .Include(mr => mr.Pet)
                    .Include(mr => mr.MedicalConsultations)
                    .Select(mr => new MedicalRecordDto
                    {
                        MedicalRecordId = mr.MedicalRecordId,
                        PetId = mr.PetId,
                        PetName = mr.Pet.PetName,
                        RecordNumber = mr.RecordNumber,
                        CreationDate = mr.CreationDate,
                        GeneralNotes = mr.GeneralNotes,
                        Status = mr.Status,
                        ConsultationsCount = mr.MedicalConsultations.Count
                    })
                    .ToListAsync();

                return Ok(medicalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los registros médicos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/MedicalRecords/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalRecordDto>> GetMedicalRecord(int id)
        {
            try
            {
                var medicalRecord = await _context.MedicalRecords
                    .Include(mr => mr.Pet)
                    .Include(mr => mr.MedicalConsultations)
                    .FirstOrDefaultAsync(mr => mr.MedicalRecordId == id);

                if (medicalRecord == null)
                {
                    return NotFound();
                }

                var medicalRecordDto = new MedicalRecordDto
                {
                    MedicalRecordId = medicalRecord.MedicalRecordId,
                    PetId = medicalRecord.PetId,
                    PetName = medicalRecord.Pet.PetName,
                    RecordNumber = medicalRecord.RecordNumber,
                    CreationDate = medicalRecord.CreationDate,
                    GeneralNotes = medicalRecord.GeneralNotes,
                    Status = medicalRecord.Status,
                    ConsultationsCount = medicalRecord.MedicalConsultations.Count
                };

                return Ok(medicalRecordDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el registro médico con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/MedicalRecords
        [HttpPost]
        public async Task<ActionResult<MedicalRecordDto>> PostMedicalRecord(MedicalRecordFormDto medicalRecordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if pet exists
                if (!await _context.Pets.AnyAsync(p => p.PetId == medicalRecordDto.PetId && p.IsActive))
                {
                    return BadRequest("La mascota especificada no existe o no está activa");
                }

                // Check if record number is unique
                if (await _context.MedicalRecords.AnyAsync(mr => mr.RecordNumber == medicalRecordDto.RecordNumber))
                {
                    return Conflict("Ya existe un registro médico con este número");
                }

                var medicalRecord = new MedicalRecord
                {
                    PetId = medicalRecordDto.PetId,
                    RecordNumber = medicalRecordDto.RecordNumber,
                    CreationDate = DateTime.UtcNow,
                    GeneralNotes = medicalRecordDto.GeneralNotes,
                    Status = medicalRecordDto.Status
                };

                _context.MedicalRecords.Add(medicalRecord);
                await _context.SaveChangesAsync();

                // Reload with related data
                var createdMedicalRecord = await _context.MedicalRecords
                    .Include(mr => mr.Pet)
                    .FirstOrDefaultAsync(mr => mr.MedicalRecordId == medicalRecord.MedicalRecordId);

                var resultDto = new MedicalRecordDto
                {
                    MedicalRecordId = createdMedicalRecord!.MedicalRecordId,
                    PetId = createdMedicalRecord.PetId,
                    PetName = createdMedicalRecord.Pet.PetName,
                    RecordNumber = createdMedicalRecord.RecordNumber,
                    CreationDate = createdMedicalRecord.CreationDate,
                    GeneralNotes = createdMedicalRecord.GeneralNotes,
                    Status = createdMedicalRecord.Status,
                    ConsultationsCount = 0
                };

                return CreatedAtAction(nameof(GetMedicalRecord), new { id = medicalRecord.MedicalRecordId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear registro médico");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/MedicalRecords/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMedicalRecord(int id, MedicalRecordFormDto medicalRecordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de registro médico inválido");
                }

                var medicalRecord = await _context.MedicalRecords.FindAsync(id);
                if (medicalRecord == null)
                {
                    return NotFound();
                }

                // Check if changing pet
                if (medicalRecord.PetId != medicalRecordDto.PetId &&
                    !await _context.Pets.AnyAsync(p => p.PetId == medicalRecordDto.PetId && p.IsActive))
                {
                    return BadRequest("La nueva mascota especificada no existe o no está activa");
                }

                // Check if changing record number to one that already exists
                if (medicalRecord.RecordNumber != medicalRecordDto.RecordNumber &&
                    await _context.MedicalRecords.AnyAsync(mr => mr.RecordNumber == medicalRecordDto.RecordNumber && mr.MedicalRecordId != id))
                {
                    return Conflict("Ya existe otro registro médico con este número");
                }

                medicalRecord.PetId = medicalRecordDto.PetId;
                medicalRecord.RecordNumber = medicalRecordDto.RecordNumber;
                medicalRecord.GeneralNotes = medicalRecordDto.GeneralNotes;
                medicalRecord.Status = medicalRecordDto.Status;

                _context.Entry(medicalRecord).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicalRecordExists(id))
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
                _logger.LogError(ex, $"Error al actualizar registro médico con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/MedicalRecords/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicalRecord(int id)
        {
            try
            {
                var medicalRecord = await _context.MedicalRecords
                    .Include(mr => mr.MedicalConsultations)
                    .FirstOrDefaultAsync(mr => mr.MedicalRecordId == id);

                if (medicalRecord == null)
                {
                    return NotFound();
                }

                // Check if medical record has consultations
                if (medicalRecord.MedicalConsultations.Any())
                {
                    return BadRequest("No se puede eliminar un registro médico con consultas asociadas");
                }

                _context.MedicalRecords.Remove(medicalRecord);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar registro médico con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool MedicalRecordExists(int id)
        {
            return _context.MedicalRecords.Any(e => e.MedicalRecordId == id);
        }
    }
}