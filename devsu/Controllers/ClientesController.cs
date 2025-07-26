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
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        
        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes([FromQuery] ClienteFilterDto filterDto)
        {
            bool hasFilters = !string.IsNullOrEmpty(filterDto.Q) ||
                            !string.IsNullOrEmpty(filterDto.Nombre) ||
                            !string.IsNullOrEmpty(filterDto.Genero) ||
                            filterDto.Edad.HasValue ||
                            !string.IsNullOrEmpty(filterDto.Identificacion) ||
                            !string.IsNullOrEmpty(filterDto.Direccion) ||
                            !string.IsNullOrEmpty(filterDto.Telefono) ||
                            filterDto.Estado.HasValue;

            bool isPaginated = filterDto.PageNumber != 1 || filterDto.PageSize != 10;

            if (hasFilters || isPaginated)
            {
                var result = await _clienteService.GetClientesFilteredAsync(filterDto);
                return Ok(result);
            }
            else
            {
                var clientes = await _clienteService.GetAllClientesAsync();
                return Ok(clientes);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var cliente = await _clienteService.GetClienteByIdAsync(id);
            return Ok(cliente);
        }

        [HttpPost]
        public async Task<ActionResult<ClienteDto>> CreateCliente(ClienteDto clienteDto)
        {
            var cliente = await _clienteService.CreateClienteAsync(clienteDto);
            return Created("", cliente);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ClienteDto>> UpdateCliente(int id, ClienteDto clienteDto)
        {
            var cliente = await _clienteService.UpdateClienteAsync(id, clienteDto);
            return Ok(cliente);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ClienteDto>> PatchCliente(int id, [FromBody] ClientePatchDto clientePatchDto)
        {
            var cliente = await _clienteService.PatchClienteAsync(id, clientePatchDto);
            return Ok(cliente);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCliente(int id)
        {
            await _clienteService.DeleteClienteAsync(id);
            return Ok(new { mensaje = "Cliente eliminado exitosamente" });
        }
    }
}