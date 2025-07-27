// Controllers/VitalSignsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VitalSignsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VitalSignsController> _logger;

        public VitalSignsController(ApplicationDbContext context, ILogger<VitalSignsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/VitalSigns
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VitalSignDto>>> GetVitalSigns()
        {
            try
            {
                var vitalSigns = await _context.VitalSigns
                    .Include(v => v.Consultation)
                    .Select(v => new VitalSignDto
                    {
                        VitalSignId = v.VitalSignId,
                        ConsultationId = v.ConsultationId,
                        ConsultationType = v.Consultation.ConsultationType,
                        Temperature = v.Temperature,
                        HeartRate = v.HeartRate,
                        RespiratoryRate = v.RespiratoryRate,
                        Weight = v.Weight,
                        BloodPressureSystolic = v.BloodPressureSystolic,
                        BloodPressureDiastolic = v.BloodPressureDiastolic,
                        RecordedDate = v.RecordedDate,
                        Notes = v.Notes
                    })
                    .ToListAsync();

                return Ok(vitalSigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los signos vitales");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/VitalSigns/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VitalSignDto>> GetVitalSign(int id)
        {
            try
            {
                var vitalSign = await _context.VitalSigns
                    .Include(v => v.Consultation)
                    .FirstOrDefaultAsync(v => v.VitalSignId == id);

                if (vitalSign == null)
                {
                    return NotFound();
                }

                var vitalSignDto = new VitalSignDto
                {
                    VitalSignId = vitalSign.VitalSignId,
                    ConsultationId = vitalSign.ConsultationId,
                    ConsultationType = vitalSign.Consultation.ConsultationType,
                    Temperature = vitalSign.Temperature,
                    HeartRate = vitalSign.HeartRate,
                    RespiratoryRate = vitalSign.RespiratoryRate,
                    Weight = vitalSign.Weight,
                    BloodPressureSystolic = vitalSign.BloodPressureSystolic,
                    BloodPressureDiastolic = vitalSign.BloodPressureDiastolic,
                    RecordedDate = vitalSign.RecordedDate,
                    Notes = vitalSign.Notes
                };

                return Ok(vitalSignDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el signo vital con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/VitalSigns
        [HttpPost]
        public async Task<ActionResult<VitalSignDto>> PostVitalSign(VitalSignFormDto vitalSignDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if consultation exists
                if (!await _context.MedicalConsultations.AnyAsync(c => c.ConsultationId == vitalSignDto.ConsultationId))
                {
                    return BadRequest("La consulta médica especificada no existe");
                }

                var vitalSign = new VitalSign
                {
                    ConsultationId = vitalSignDto.ConsultationId,
                    Temperature = vitalSignDto.Temperature,
                    HeartRate = vitalSignDto.HeartRate,
                    RespiratoryRate = vitalSignDto.RespiratoryRate,
                    Weight = vitalSignDto.Weight,
                    BloodPressureSystolic = vitalSignDto.BloodPressureSystolic,
                    BloodPressureDiastolic = vitalSignDto.BloodPressureDiastolic,
                    RecordedDate = DateTime.Now,
                    Notes = vitalSignDto.Notes
                };

                _context.VitalSigns.Add(vitalSign);
                await _context.SaveChangesAsync();

                // Reload with related data
                var createdVitalSign = await _context.VitalSigns
                    .Include(v => v.Consultation)
                    .FirstOrDefaultAsync(v => v.VitalSignId == vitalSign.VitalSignId);

                var resultDto = new VitalSignDto
                {
                    VitalSignId = createdVitalSign!.VitalSignId,
                    ConsultationId = createdVitalSign.ConsultationId,
                    ConsultationType = createdVitalSign.Consultation.ConsultationType,
                    Temperature = createdVitalSign.Temperature,
                    HeartRate = createdVitalSign.HeartRate,
                    RespiratoryRate = createdVitalSign.RespiratoryRate,
                    Weight = createdVitalSign.Weight,
                    BloodPressureSystolic = createdVitalSign.BloodPressureSystolic,
                    BloodPressureDiastolic = createdVitalSign.BloodPressureDiastolic,
                    RecordedDate = createdVitalSign.RecordedDate,
                    Notes = createdVitalSign.Notes
                };

                return CreatedAtAction(nameof(GetVitalSign), new { id = vitalSign.VitalSignId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear signo vital");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/VitalSigns/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVitalSign(int id, VitalSignFormDto vitalSignDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de signo vital inválido");
                }

                var vitalSign = await _context.VitalSigns.FindAsync(id);
                if (vitalSign == null)
                {
                    return NotFound();
                }

                // Check if changing consultation
                if (vitalSign.ConsultationId != vitalSignDto.ConsultationId &&
                    !await _context.MedicalConsultations.AnyAsync(c => c.ConsultationId == vitalSignDto.ConsultationId))
                {
                    return BadRequest("La nueva consulta médica especificada no existe");
                }

                vitalSign.ConsultationId = vitalSignDto.ConsultationId;
                vitalSign.Temperature = vitalSignDto.Temperature;
                vitalSign.HeartRate = vitalSignDto.HeartRate;
                vitalSign.RespiratoryRate = vitalSignDto.RespiratoryRate;
                vitalSign.Weight = vitalSignDto.Weight;
                vitalSign.BloodPressureSystolic = vitalSignDto.BloodPressureSystolic;
                vitalSign.BloodPressureDiastolic = vitalSignDto.BloodPressureDiastolic;
                vitalSign.Notes = vitalSignDto.Notes;

                _context.Entry(vitalSign).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VitalSignExists(id))
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
                _logger.LogError(ex, $"Error al actualizar signo vital con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/VitalSigns/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVitalSign(int id)
        {
            try
            {
                var vitalSign = await _context.VitalSigns.FindAsync(id);
                if (vitalSign == null)
                {
                    return NotFound();
                }

                _context.VitalSigns.Remove(vitalSign);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar signo vital con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool VitalSignExists(int id)
        {
            return _context.VitalSigns.Any(e => e.VitalSignId == id);
        }
    }
}