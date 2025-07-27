// Controllers/PrescriptionsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PrescriptionsController> _logger;

        public PrescriptionsController(ApplicationDbContext context, ILogger<PrescriptionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Prescriptions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrescriptionDto>>> GetPrescriptions()
        {
            try
            {
                var prescriptions = await _context.Prescriptions
                    .Include(p => p.Consultation)
                    .Include(p => p.Medication)
                    .Select(p => new PrescriptionDto
                    {
                        PrescriptionId = p.PrescriptionId,
                        ConsultationId = p.ConsultationId,
                        ConsultationType = p.Consultation.ConsultationType,
                        MedicationId = p.MedicationId,
                        MedicationName = p.Medication.MedicationName,
                        Dosage = p.Dosage,
                        Frequency = p.Frequency,
                        Duration = p.Duration,
                        Instructions = p.Instructions,
                        Quantity = p.Quantity,
                        Refills = p.Refills,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        Status = p.Status,
                        CreatedDate = p.CreatedDate
                    })
                    .ToListAsync();

                return Ok(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las prescripciones");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Prescriptions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PrescriptionDto>> GetPrescription(int id)
        {
            try
            {
                var prescription = await _context.Prescriptions
                    .Include(p => p.Consultation)
                    .Include(p => p.Medication)
                    .FirstOrDefaultAsync(p => p.PrescriptionId == id);

                if (prescription == null)
                {
                    return NotFound();
                }

                var prescriptionDto = new PrescriptionDto
                {
                    PrescriptionId = prescription.PrescriptionId,
                    ConsultationId = prescription.ConsultationId,
                    ConsultationType = prescription.Consultation.ConsultationType,
                    MedicationId = prescription.MedicationId,
                    MedicationName = prescription.Medication.MedicationName,
                    Dosage = prescription.Dosage,
                    Frequency = prescription.Frequency,
                    Duration = prescription.Duration,
                    Instructions = prescription.Instructions,
                    Quantity = prescription.Quantity,
                    Refills = prescription.Refills,
                    StartDate = prescription.StartDate,
                    EndDate = prescription.EndDate,
                    Status = prescription.Status,
                    CreatedDate = prescription.CreatedDate
                };

                return Ok(prescriptionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener la prescripción con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Prescriptions
        [HttpPost]
        public async Task<ActionResult<PrescriptionDto>> PostPrescription(PrescriptionFormDto prescriptionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if consultation exists
                if (!await _context.MedicalConsultations.AnyAsync(c => c.ConsultationId == prescriptionDto.ConsultationId))
                {
                    return BadRequest("La consulta médica especificada no existe");
                }

                // Check if medication exists
                if (!await _context.Medications.AnyAsync(m => m.MedicationId == prescriptionDto.MedicationId))
                {
                    return BadRequest("El medicamento especificado no existe");
                }

                var prescription = new Prescription
                {
                    ConsultationId = prescriptionDto.ConsultationId,
                    MedicationId = prescriptionDto.MedicationId,
                    Dosage = prescriptionDto.Dosage,
                    Frequency = prescriptionDto.Frequency,
                    Duration = prescriptionDto.Duration,
                    Instructions = prescriptionDto.Instructions,
                    Quantity = prescriptionDto.Quantity,
                    Refills = prescriptionDto.Refills,
                    StartDate = prescriptionDto.StartDate,
                    EndDate = prescriptionDto.EndDate,
                    Status = prescriptionDto.Status,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                // Reload with related data
                var createdPrescription = await _context.Prescriptions
                    .Include(p => p.Consultation)
                    .Include(p => p.Medication)
                    .FirstOrDefaultAsync(p => p.PrescriptionId == prescription.PrescriptionId);

                var resultDto = new PrescriptionDto
                {
                    PrescriptionId = createdPrescription!.PrescriptionId,
                    ConsultationId = createdPrescription.ConsultationId,
                    ConsultationType = createdPrescription.Consultation.ConsultationType,
                    MedicationId = createdPrescription.MedicationId,
                    MedicationName = createdPrescription.Medication.MedicationName,
                    Dosage = createdPrescription.Dosage,
                    Frequency = createdPrescription.Frequency,
                    Duration = createdPrescription.Duration,
                    Instructions = createdPrescription.Instructions,
                    Quantity = createdPrescription.Quantity,
                    Refills = createdPrescription.Refills,
                    StartDate = createdPrescription.StartDate,
                    EndDate = createdPrescription.EndDate,
                    Status = createdPrescription.Status,
                    CreatedDate = createdPrescription.CreatedDate
                };

                return CreatedAtAction(nameof(GetPrescription), new { id = prescription.PrescriptionId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear prescripción");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Prescriptions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPrescription(int id, PrescriptionFormDto prescriptionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de prescripción inválido");
                }

                var prescription = await _context.Prescriptions.FindAsync(id);
                if (prescription == null)
                {
                    return NotFound();
                }

                // Check if changing consultation
                if (prescription.ConsultationId != prescriptionDto.ConsultationId &&
                    !await _context.MedicalConsultations.AnyAsync(c => c.ConsultationId == prescriptionDto.ConsultationId))
                {
                    return BadRequest("La nueva consulta médica especificada no existe");
                }

                // Check if changing medication
                if (prescription.MedicationId != prescriptionDto.MedicationId &&
                    !await _context.Medications.AnyAsync(m => m.MedicationId == prescriptionDto.MedicationId))
                {
                    return BadRequest("El nuevo medicamento especificado no existe");
                }

                prescription.ConsultationId = prescriptionDto.ConsultationId;
                prescription.MedicationId = prescriptionDto.MedicationId;
                prescription.Dosage = prescriptionDto.Dosage;
                prescription.Frequency = prescriptionDto.Frequency;
                prescription.Duration = prescriptionDto.Duration;
                prescription.Instructions = prescriptionDto.Instructions;
                prescription.Quantity = prescriptionDto.Quantity;
                prescription.Refills = prescriptionDto.Refills;
                prescription.StartDate = prescriptionDto.StartDate;
                prescription.EndDate = prescriptionDto.EndDate;
                prescription.Status = prescriptionDto.Status;

                _context.Entry(prescription).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrescriptionExists(id))
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
                _logger.LogError(ex, $"Error al actualizar prescripción con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Prescriptions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            try
            {
                var prescription = await _context.Prescriptions.FindAsync(id);
                if (prescription == null)
                {
                    return NotFound();
                }

                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar prescripción con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool PrescriptionExists(int id)
        {
            return _context.Prescriptions.Any(e => e.PrescriptionId == id);
        }
    }
}