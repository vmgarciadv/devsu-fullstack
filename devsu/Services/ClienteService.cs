using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using devsu.DTOs;
using devsu.Models;
using devsu.Repositories;
using devsu.Exceptions;

namespace devsu.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        
        public ClienteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ClienteDto> GetClienteByIdAsync(int id)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
            if (cliente == null)
            {
                throw new NotFoundException($"Cliente con ID {id} no encontrado");
            }
            return _mapper.Map<ClienteDto>(cliente);
        }
        
        public async Task<ClienteDto> CreateClienteAsync(ClienteDto clienteDto)
        {
            // Verificar si ya existe un cliente con la misma identificación
            var clienteExistente = await _unitOfWork.Clientes.GetByIdentificacionAsync(clienteDto.Identificacion);
            if (clienteExistente != null)
            {
                throw new BusinessException("Ya existe un cliente con esa Identificacion.");
            }
            
            var cliente = _mapper.Map<Cliente>(clienteDto);
            await _unitOfWork.Clientes.AddAsync(cliente);
            await _unitOfWork.CompleteAsync();
            
            return _mapper.Map<ClienteDto>(cliente);
        }

        public async Task<IEnumerable<ClienteDto>> GetAllClientesAsync()
        {
            var clientes = await _unitOfWork.Clientes.GetAllAsync();
            // Ordenar por Estado descendente (true primero) y luego por nombre
            var clientesOrdenados = clientes.OrderByDescending(c => c.Estado).ThenBy(c => c.Nombre);
            return _mapper.Map<IEnumerable<ClienteDto>>(clientesOrdenados);
        }

        public async Task<PaginatedResponse<ClienteDto>> GetAllClientesPaginatedAsync(PaginationParameters paginationParameters)
        {
            var clientes = await _unitOfWork.Clientes.GetAllAsync();
            
            // Ordenar por Estado descendente (true primero) y luego por nombre
            var clientesOrdenados = clientes
                .OrderByDescending(c => c.Estado)
                .ThenBy(c => c.Nombre)
                .ToList();

            var totalRecords = clientesOrdenados.Count;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)paginationParameters.PageSize);

            var clientesPaginados = clientesOrdenados
                .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
                .Take(paginationParameters.PageSize)
                .ToList();

            var clientesDto = _mapper.Map<IEnumerable<ClienteDto>>(clientesPaginados);

            return new PaginatedResponse<ClienteDto>
            {
                Data = clientesDto,
                PageNumber = paginationParameters.PageNumber,
                PageSize = paginationParameters.PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

        public async Task<ClienteDto> UpdateClienteAsync(int id, ClienteDto clienteDto)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
            if (cliente == null)
            {
                throw new NotFoundException("Cliente no encontrado");
            }

            // Verificar si la identificación ya está en uso por otro cliente
            if (clienteDto.Identificacion != cliente.Identificacion)
            {
                var clienteExistente = await _unitOfWork.Clientes.GetByIdentificacionAsync(clienteDto.Identificacion);
                if (clienteExistente != null && clienteExistente.Id != id)
                {
                    throw new BusinessException("Ya existe un cliente con esa Identificacion.");
                }
            }

            // Actualizar manualmente las propiedades para evitar problemas con el Id
            cliente.Nombre = clienteDto.Nombre;
            cliente.Genero = clienteDto.Genero;
            cliente.Edad = clienteDto.Edad;
            cliente.Identificacion = clienteDto.Identificacion;
            cliente.Direccion = clienteDto.Direccion;
            cliente.Telefono = clienteDto.Telefono;
            cliente.Contrasena = clienteDto.Contrasena;
            cliente.Estado = clienteDto.Estado;
            
            await _unitOfWork.CompleteAsync();
            
            return _mapper.Map<ClienteDto>(cliente);
        }

        public async Task<ClienteDto> PatchClienteAsync(int id, ClientePatchDto clientePatchDto)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
            if (cliente == null)
            {
                throw new NotFoundException("Cliente no encontrado");
            }

            // Validar que no exista otro cliente con la misma Identificación (en caso de ser necesario)
            if (clientePatchDto.Identificacion != null && clientePatchDto.Identificacion != cliente.Identificacion)
            {
                var clienteExistente = await _unitOfWork.Clientes.GetByIdentificacionAsync(clientePatchDto.Identificacion);
                if (clienteExistente != null && clienteExistente.Id != id)
                {
                    throw new InvalidOperationException("Ya existe un cliente con esa Identificacion");
                }
            }

            // Usar reflection para actualizar solo las propiedades no nulas
            var patchProperties = typeof(ClientePatchDto).GetProperties();
            var clienteType = typeof(Cliente);

            foreach (var patchProp in patchProperties)
            {
                var value = patchProp.GetValue(clientePatchDto);
                
                // Omitir valores nulos y cadenas vacías
                if (value == null || (value is string str && string.IsNullOrEmpty(str)))
                    continue;

                var clienteProp = clienteType.GetProperty(patchProp.Name);
                if (clienteProp != null && clienteProp.CanWrite)
                {
                    clienteProp.SetValue(cliente, value);
                }
            }

            await _unitOfWork.CompleteAsync();
            
            return _mapper.Map<ClienteDto>(cliente);
        }

        public async Task<PaginatedResponse<ClienteDto>> GetClientesFilteredAsync(ClienteFilterDto filterDto)
        {
            var clientes = await _unitOfWork.Clientes.GetAllAsync();
            var query = clientes.AsQueryable();

            if (!string.IsNullOrEmpty(filterDto.Q))
            {
                var searchTerm = filterDto.Q.ToLower();
                query = query.Where(c => 
                    c.Nombre.ToLower().Contains(searchTerm) ||
                    c.Genero.ToLower().Contains(searchTerm) ||
                    c.Edad.ToString().Contains(searchTerm) ||
                    c.Identificacion.ToLower().Contains(searchTerm) ||
                    c.Direccion.ToLower().Contains(searchTerm) ||
                    c.Telefono.ToLower().Contains(searchTerm) ||
                    (c.Estado ? "activo" : "inactivo").Contains(searchTerm) ||
                    (c.Estado ? "true" : "false").Contains(searchTerm)
                );
            }
            else
            {
                if (!string.IsNullOrEmpty(filterDto.Nombre))
                {
                    query = query.Where(c => c.Nombre.Contains(filterDto.Nombre, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(filterDto.Genero))
                {
                    query = query.Where(c => c.Genero.Equals(filterDto.Genero, StringComparison.OrdinalIgnoreCase));
                }

                if (filterDto.Edad.HasValue)
                {
                    query = query.Where(c => c.Edad == filterDto.Edad.Value);
                }

                if (!string.IsNullOrEmpty(filterDto.Identificacion))
                {
                    query = query.Where(c => c.Identificacion.Contains(filterDto.Identificacion));
                }

                if (!string.IsNullOrEmpty(filterDto.Direccion))
                {
                    query = query.Where(c => c.Direccion.Contains(filterDto.Direccion, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(filterDto.Telefono))
                {
                    query = query.Where(c => c.Telefono.Contains(filterDto.Telefono));
                }

                if (filterDto.Estado.HasValue)
                {
                    query = query.Where(c => c.Estado == filterDto.Estado.Value);
                }
            }

            var clientesOrdenados = query
                .OrderByDescending(c => c.Estado)
                .ThenBy(c => c.Nombre)
                .ToList();

            var totalRecords = clientesOrdenados.Count;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)filterDto.PageSize);

            var clientesPaginados = clientesOrdenados
                .Skip((filterDto.PageNumber - 1) * filterDto.PageSize)
                .Take(filterDto.PageSize)
                .ToList();

            var clientesDto = _mapper.Map<IEnumerable<ClienteDto>>(clientesPaginados);

            return new PaginatedResponse<ClienteDto>
            {
                Data = clientesDto,
                PageNumber = filterDto.PageNumber,
                PageSize = filterDto.PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

        public async Task<bool> DeleteClienteAsync(int id)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
            if (cliente == null)
            {
                throw new NotFoundException("Cliente no encontrado");
            }

            // Soft delete - cambiar Estado a false
            cliente.Estado = false;
            
            // Soft delete todas las cuentas asociadas
            var cuentasCliente = await _unitOfWork.Cuentas.GetCuentasByClienteAsync(cliente.Id);
            foreach (var cuenta in cuentasCliente)
            {
                cuenta.Estado = false;
            }
            
            await _unitOfWork.CompleteAsync();
            
            return true;
        }
    }
}