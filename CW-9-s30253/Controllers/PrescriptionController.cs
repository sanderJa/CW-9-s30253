using CW_9_s30253.DTOs;
using CW_9_s30253.Services;
using Microsoft.AspNetCore.Mvc;

namespace CW_9_s30253.Controllers;

[ApiController]
[Route("/[controller]")]
public class PrescriptionController : ControllerBase
{
    private readonly IPrescriptionService _service;

    public PrescriptionController(IPrescriptionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> AddPrescription([FromBody] PrescriptionRequestDto dto)
    {
        try
        {
            await _service.AddPrescriptionAsync(dto);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{idPatient}")]
    public async Task<IActionResult> GetPatientDetails(int idPatient)
    {
        try
        {
            var result = await _service.GetPatientDetailsAsync(idPatient);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
}