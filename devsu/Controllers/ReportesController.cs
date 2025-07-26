using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using devsu.DTOs;
using devsu.Services;

namespace devsu.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reporteService;

        public ReportesController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [HttpGet]
        public async Task<IActionResult> GenerarReporte(
            [FromQuery] string cliente,
            [FromQuery] DateTime? fecha,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int timezone = 0)
        {
            ReporteRequestDto request;

            // Fecha
            if (fecha.HasValue)
            {
                request = new ReporteRequestDto
                {
                    Cliente = cliente,
                    FechaInicio = fecha.Value.Date,
                    FechaFin = fecha.Value.Date.AddDays(1).AddSeconds(-1),
                    TimezoneOffset = timezone
                };
            }
            // FechaInicio y FechaFin
            else if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                request = new ReporteRequestDto
                {
                    Cliente = cliente,
                    FechaInicio = fechaInicio.Value,
                    FechaFin = fechaFin.Value,
                    TimezoneOffset = timezone
                };
            }
            else
            {
                return BadRequest(new ErrorResponse
                {
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    Title = "Parámetros inválidos",
                    Status = 400,
                    Detail = "Debe proporcionar 'fecha' o tanto 'fechaInicio' como 'fechaFin'",
                    Instance = "/api/reportes"
                });
            }

            var response = await _reporteService.GenerarReporteEstadoCuentaAsync(request);
            return Ok(response);
        }
    }
}