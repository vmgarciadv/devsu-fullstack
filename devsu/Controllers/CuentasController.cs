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
    public class CuentasController : ControllerBase
    {
        private readonly ICuentaService _cuentaService;
        
        public CuentasController(ICuentaService cuentaService)
        {
            _cuentaService = cuentaService;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CuentaDto>>> GetCuentas([FromQuery] CuentaFilterDto filterDto)
        {
            bool hasFilters = !string.IsNullOrEmpty(filterDto.Q) ||
                            filterDto.NumeroCuenta.HasValue ||
                            !string.IsNullOrEmpty(filterDto.TipoCuenta) ||
                            filterDto.SaldoInicial.HasValue ||
                            filterDto.Estado.HasValue ||
                            !string.IsNullOrEmpty(filterDto.NombreCliente);

            bool isPaginated = filterDto.PageNumber != 1 || filterDto.PageSize != 10;

            if (hasFilters || isPaginated)
            {
                var result = await _cuentaService.GetCuentasFilteredAsync(filterDto);
                return Ok(result);
            }
            else
            {
                var cuentas = await _cuentaService.GetAllCuentasAsync();
                return Ok(cuentas);
            }
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<CuentaDto>> GetCuenta(int id)
        {
            var cuenta = await _cuentaService.GetCuentaByNumeroCuentaAsync(id);
            return Ok(cuenta);
        }
        
        [HttpPost]
        public async Task<ActionResult<CuentaDto>> CreateCuenta(CuentaDto cuentaDto)
        {
            var cuenta = await _cuentaService.CreateCuentaAsync(cuentaDto);
            return CreatedAtAction(nameof(GetCuenta), new { id = cuenta.NumeroCuenta }, cuenta);
        }

        [HttpPut("{numeroCuenta}")]
        public async Task<ActionResult<CuentaDto>> UpdateCuenta(int numeroCuenta, CuentaDto cuentaDto)
        {
            var cuenta = await _cuentaService.UpdateCuentaAsync(numeroCuenta, cuentaDto);
            return Ok(cuenta);
        }

        [HttpPatch("{numeroCuenta}")]
        public async Task<ActionResult<CuentaDto>> PatchCuenta(int numeroCuenta, [FromBody] CuentaPatchDto cuentaPatchDto)
        {
            var cuenta = await _cuentaService.PatchCuentaAsync(numeroCuenta, cuentaPatchDto);
            return Ok(cuenta);
        }

        [HttpDelete("{numeroCuenta}")]
        public async Task<ActionResult> DeleteCuenta(int numeroCuenta)
        {
            await _cuentaService.DeleteCuentaAsync(numeroCuenta);
            return Ok(new { mensaje = "Cuenta eliminada exitosamente" });
        }
    }
}