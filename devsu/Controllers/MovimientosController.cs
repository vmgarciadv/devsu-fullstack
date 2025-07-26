using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using devsu.DTOs;
using devsu.Services;

namespace devsu.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class MovimientosController : ControllerBase
  {
    private readonly IMovimientoService _movimientoService;

    public MovimientosController(IMovimientoService movimientoService)
    {
      _movimientoService = movimientoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovimientoDto>>> GetMovimientos([FromQuery] MovimientoFilterDto filterDto)
    {
      bool hasFilters = !string.IsNullOrEmpty(filterDto.Q) ||
                        filterDto.Fecha.HasValue ||
                        !string.IsNullOrEmpty(filterDto.TipoMovimiento) ||
                        filterDto.Valor.HasValue ||
                        filterDto.Saldo.HasValue ||
                        filterDto.NumeroCuenta.HasValue ||
                        filterDto.Timezone != 0;

      bool isPaginated = filterDto.PageNumber != 1 || filterDto.PageSize != 10;

      if (hasFilters || isPaginated)
      {
        var result = await _movimientoService.GetMovimientosFilteredAsync(filterDto);
        return Ok(result);
      }
      else
      {
        var movimientos = await _movimientoService.GetAllMovimientosAsync();
        return Ok(movimientos);
      }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovimientoDto>> GetMovimiento(int id)
    {
      var movimiento = await _movimientoService.GetMovimientoByIdAsync(id);
      return Ok(movimiento);
    }

    [HttpPost]
    public async Task<ActionResult<MovimientoDto>> CreateMovimiento(CreateMovimientoDto createMovimientoDto)
    {
      var movimiento = await _movimientoService.CreateMovimientoAsync(createMovimientoDto);
      return Created("", movimiento);
    }
  }
}