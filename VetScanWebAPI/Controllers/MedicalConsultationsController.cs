// Controllers/MedicalConsultationsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalConsultationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicalConsultationsController> _logger;

        public MedicalConsultationsController(ApplicationDbContext context, ILogger<MedicalConsultationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/MedicalConsultations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicalConsultationDto>>> GetMedicalConsultations()
        {
            try
            {
                var consultations = await _context.MedicalConsultations
                    .Include(c => c.MedicalRecord)
                    .Include(c => c.AttendingVeterinarian)
                    .ThenInclude(v => v.User)
                    .Include(c => c.VitalSigns)
                    .Include(c => c.Prescriptions)
                    .Select(c => new MedicalConsultationDto
                    {
                        ConsultationId = c.ConsultationId,
                        MedicalRecordId = c.MedicalRecordId,
                        RecordNumber = c.MedicalRecord.RecordNumber,
                        VeterinarianId = c.VeterinarianId,
                        VeterinarianName = $"{c.AttendingVeterinarian.User.FirstName} {c.AttendingVeterinarian.User.LastName}",
                        ConsultationDate = c.ConsultationDate,
                        ConsultationType = c.ConsultationType,
                        Diagnosis = c.Diagnosis,
                        Treatment = c.Treatment,
                        NextAppointmentRecommended = c.NextAppointmentRecommended,
                        Status = c.Status,
                        VitalSignsCount = c.VitalSigns.Count,
                        PrescriptionsCount = c.Prescriptions.Count
                    })
                    .ToListAsync();

                return Ok(consultations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las consultas médicas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/MedicalConsultations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalConsultationDto>> GetMedicalConsultation(int id)
        {
            try
            {
                var consultation = await _context.MedicalConsultations
                    .Include(c => c.MedicalRecord)
                    .Include(c => c.AttendingVeterinarian)
                    .ThenInclude(v => v.User)
                    .Include(c => c.VitalSigns)
                    .Include(c => c.Prescriptions)
                    .FirstOrDefaultAsync(c => c.ConsultationId == id);

                if (consultation == null)
                {
                    return NotFound();
                }

                var consultationDto = new MedicalConsultationDto
                {
                    ConsultationId = consultation.ConsultationId,
                    MedicalRecordId = consultation.MedicalRecordId,
                    RecordNumber = consultation.MedicalRecord.RecordNumber,
                    VeterinarianId = consultation.VeterinarianId,
                    VeterinarianName = $"{consultation.AttendingVeterinarian.User.FirstName} {consultation.AttendingVeterinarian.User.LastName}",
                    ConsultationDate = consultation.ConsultationDate,
                    ConsultationType = consultation.ConsultationType,
                    Diagnosis = consultation.Diagnosis,
                    Treatment = consultation.Treatment,
                    NextAppointmentRecommended = consultation.NextAppointmentRecommended,
                    Status = consultation.Status,
                    VitalSignsCount = consultation.VitalSigns.Count,
                    PrescriptionsCount = consultation.Prescriptions.Count
                };

                return Ok(consultationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener la consulta médica con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/MedicalConsultations
        [HttpPost]
        public async Task<ActionResult<MedicalConsultationDto>> PostMedicalConsultation(MedicalConsultationFormDto consultationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if medical record exists
                if (!await _context.MedicalRecords.AnyAsync(mr => mr.MedicalRecordId == consultationDto.MedicalRecordId))
                {
                    return BadRequest("El registro médico especificado no existe");
                }

                // Check if veterinarian exists
                if (!await _context.Veterinarians.AnyAsync(v => v.VeterinarianId == consultationDto.VeterinarianId))
                {
                    return BadRequest("El veterinario especificado no existe");
                }

                var consultation = new MedicalConsultation
                {
                    MedicalRecordId = consultationDto.MedicalRecordId,
                    VeterinarianId = consultationDto.VeterinarianId,
                    ConsultationDate = consultationDto.ConsultationDate,
                    ConsultationType = consultationDto.ConsultationType,
                    Diagnosis = consultationDto.Diagnosis,
                    Treatment = consultationDto.Treatment,
                    NextAppointmentRecommended = consultationDto.NextAppointmentRecommended,
                    Status = consultationDto.Status
                };

                _context.MedicalConsultations.Add(consultation);
                await _context.SaveChangesAsync();

                // Reload with related data
                var createdConsultation = await _context.MedicalConsultations
                    .Include(c => c.MedicalRecord)
                    .Include(c => c.AttendingVeterinarian)
                    .ThenInclude(v => v.User)
                    .FirstOrDefaultAsync(c => c.ConsultationId == consultation.ConsultationId);

                var resultDto = new MedicalConsultationDto
                {
                    ConsultationId = createdConsultation!.ConsultationId,
                    MedicalRecordId = createdConsultation.MedicalRecordId,
                    RecordNumber = createdConsultation.MedicalRecord.RecordNumber,
                    VeterinarianId = createdConsultation.VeterinarianId,
                    VeterinarianName = $"{createdConsultation.AttendingVeterinarian.User.FirstName} {createdConsultation.AttendingVeterinarian.User.LastName}",
                    ConsultationDate = createdConsultation.ConsultationDate,
                    ConsultationType = createdConsultation.ConsultationType,
                    Diagnosis = createdConsultation.Diagnosis,
                    Treatment = createdConsultation.Treatment,
                    NextAppointmentRecommended = createdConsultation.NextAppointmentRecommended,
                    Status = createdConsultation.Status,
                    VitalSignsCount = 0,
                    PrescriptionsCount = 0
                };

                return CreatedAtAction(nameof(GetMedicalConsultation), new { id = consultation.ConsultationId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear consulta médica");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/MedicalConsultations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMedicalConsultation(int id, MedicalConsultationFormDto consultationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de consulta médica inválido");
                }

                var consultation = await _context.MedicalConsultations.FindAsync(id);
                if (consultation == null)
                {
                    return NotFound();
                }

                // Check if medical record exists
                if (!await _context.MedicalRecords.AnyAsync(mr => mr.MedicalRecordId == consultationDto.MedicalRecordId))
                {
                    return BadRequest("El nuevo registro médico especificado no existe");
                }

                // Check if veterinarian exists
                if (!await _context.Veterinarians.AnyAsync(v => v.VeterinarianId == consultationDto.VeterinarianId))
                {
                    return BadRequest("El nuevo veterinario especificado no existe");
                }

                consultation.MedicalRecordId = consultationDto.MedicalRecordId;
                consultation.VeterinarianId = consultationDto.VeterinarianId;
                consultation.ConsultationDate = consultationDto.ConsultationDate;
                consultation.ConsultationType = consultationDto.ConsultationType;
                consultation.Diagnosis = consultationDto.Diagnosis;
                consultation.Treatment = consultationDto.Treatment;
                consultation.NextAppointmentRecommended = consultationDto.NextAppointmentRecommended;
                consultation.Status = consultationDto.Status;

                _context.Entry(consultation).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicalConsultationExists(id))
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
                _logger.LogError(ex, $"Error al actualizar consulta médica con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/MedicalConsultations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicalConsultation(int id)
        {
            try
            {
                var consultation = await _context.MedicalConsultations
                    .Include(c => c.VitalSigns)
                    .Include(c => c.Prescriptions)
                    .FirstOrDefaultAsync(c => c.ConsultationId == id);

                if (consultation == null)
                {
                    return NotFound();
                }

                // Check if consultation has related records
                if (consultation.VitalSigns.Any() || consultation.Prescriptions.Any())
                {
                    return BadRequest("No se puede eliminar una consulta médica con signos vitales o prescripciones asociadas");
                }

                _context.MedicalConsultations.Remove(consultation);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar consulta médica con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool MedicalConsultationExists(int id)
        {
            return _context.MedicalConsultations.Any(e => e.ConsultationId == id);
        }
    }
}