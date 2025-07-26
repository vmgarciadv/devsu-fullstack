using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Options;
using devsu.DTOs;
using devsu.Models;
using devsu.Repositories;
using devsu.Configuration;
using devsu.Exceptions;

namespace devsu.Services
{
    public class MovimientoService : IMovimientoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly BusinessRulesOptions _businessRules;

        public MovimientoService(IUnitOfWork unitOfWork, IMapper mapper, IOptions<BusinessRulesOptions> businessRulesOptions)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _businessRules = businessRulesOptions.Value;
        }

        public async Task<IEnumerable<MovimientoDto>> GetAllMovimientosAsync()
        {
            var movimientos = await _unitOfWork.Movimientos.GetAllWithCuentaAsync();
            // Ordenar por fecha descendente (más recientes primero)
            var movimientosOrdenados = movimientos.OrderByDescending(m => m.Fecha).ThenByDescending(m => m.MovimientoId);
            return _mapper.Map<IEnumerable<MovimientoDto>>(movimientosOrdenados);
        }

        public async Task<PaginatedResponse<MovimientoDto>> GetAllMovimientosPaginatedAsync(PaginationParameters paginationParameters)
        {
            var movimientos = await _unitOfWork.Movimientos.GetAllWithCuentaAsync();
            
            // Ordenar por fecha descendente (más recientes primero)
            var movimientosOrdenados = movimientos
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.MovimientoId)
                .ToList();

            var totalRecords = movimientosOrdenados.Count;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)paginationParameters.PageSize);

            var movimientosPaginados = movimientosOrdenados
                .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
                .Take(paginationParameters.PageSize)
                .ToList();

            var movimientosDto = _mapper.Map<IEnumerable<MovimientoDto>>(movimientosPaginados);

            return new PaginatedResponse<MovimientoDto>
            {
                Data = movimientosDto,
                PageNumber = paginationParameters.PageNumber,
                PageSize = paginationParameters.PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

        public async Task<MovimientoDto> GetMovimientoByIdAsync(int id)
        {
            var movimiento = await _unitOfWork.Movimientos.GetByIdAsync(id);
            if (movimiento == null)
            {
                throw new NotFoundException($"Movimiento con ID {id} no encontrado");
            }
            return _mapper.Map<MovimientoDto>(movimiento);
        }

        public async Task<PaginatedResponse<MovimientoDto>> GetMovimientosFilteredAsync(MovimientoFilterDto filterDto)
        {
            var movimientos = await _unitOfWork.Movimientos.GetAllWithCuentaAsync();
            var query = movimientos.AsQueryable();

            if (!string.IsNullOrEmpty(filterDto.Q))
            {
                var searchTerm = filterDto.Q.ToLower();
                query = query.Where(m => 
                    m.Fecha.ToString().Contains(searchTerm) ||
                    m.TipoMovimiento.ToLower().Contains(searchTerm) ||
                    m.Valor.ToString().Contains(searchTerm) ||
                    m.Saldo.ToString().Contains(searchTerm) ||
                    m.Cuenta.NumeroCuenta.ToString().Contains(searchTerm) ||
                    (m.Cuenta.Cliente != null && m.Cuenta.Cliente.Nombre.ToLower().Contains(searchTerm))
                );
            }
            else
            {
                if (filterDto.Fecha.HasValue)
                {
                    var localStartDate = filterDto.Fecha.Value.Date;
                    var localEndDate = localStartDate.AddDays(1);
                    
                    var utcStartDate = localStartDate.AddHours(-filterDto.Timezone);
                    var utcEndDate = localEndDate.AddHours(-filterDto.Timezone);
                    
                    query = query.Where(m => m.Fecha >= utcStartDate && m.Fecha < utcEndDate);
                }

                if (!string.IsNullOrEmpty(filterDto.TipoMovimiento))
                {
                    query = query.Where(m => m.TipoMovimiento.Equals(filterDto.TipoMovimiento, StringComparison.OrdinalIgnoreCase));
                }

                if (filterDto.Valor.HasValue)
                {
                    query = query.Where(m => m.Valor == filterDto.Valor.Value);
                }

                if (filterDto.Saldo.HasValue)
                {
                    query = query.Where(m => m.Saldo == filterDto.Saldo.Value);
                }

                if (filterDto.NumeroCuenta.HasValue)
                {
                    query = query.Where(m => m.Cuenta.NumeroCuenta == filterDto.NumeroCuenta.Value);
                }
            }

            var movimientosOrdenados = query
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.MovimientoId)
                .ToList();

            var totalRecords = movimientosOrdenados.Count;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)filterDto.PageSize);

            var movimientosPaginados = movimientosOrdenados
                .Skip((filterDto.PageNumber - 1) * filterDto.PageSize)
                .Take(filterDto.PageSize)
                .ToList();

            var movimientosDto = movimientosPaginados.Select(m => new MovimientoDto
            {
                Fecha = m.Fecha.AddHours(filterDto.Timezone),
                TipoMovimiento = m.TipoMovimiento,
                Valor = m.Valor,
                Saldo = m.Saldo,
                NumeroCuenta = m.Cuenta.NumeroCuenta
            });

            return new PaginatedResponse<MovimientoDto>
            {
                Data = movimientosDto,
                PageNumber = filterDto.PageNumber,
                PageSize = filterDto.PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

        public async Task<MovimientoDto> CreateMovimientoAsync(CreateMovimientoDto createMovimientoDto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Obtener la cuenta por número de cuenta
                var cuenta = await _unitOfWork.Cuentas.GetByNumeroCuentaAsync(createMovimientoDto.NumeroCuenta);
                if (cuenta == null)
                {
                    throw new NotFoundException($"Cuenta con número {createMovimientoDto.NumeroCuenta} no encontrada");
                }

                // Validar que la cuenta esté activa
                if (!cuenta.Estado)
                {
                    throw new BusinessException($"La cuenta {createMovimientoDto.NumeroCuenta} no está activa");
                }

                // Obtener el último movimiento para calcular el saldo actual
                var ultimoMovimiento = await _unitOfWork.Movimientos.GetLastMovimientoByCuentaAsync(cuenta.CuentaId);
                decimal saldoActual = ultimoMovimiento?.Saldo ?? cuenta.SaldoInicial;

                // Validar tipo de movimiento
                var tipoMovimientoNormalizado = createMovimientoDto.TipoMovimiento.ToLower();
                if (tipoMovimientoNormalizado != "debito" && tipoMovimientoNormalizado != "credito")
                {
                    throw new ArgumentException($"TipoMovimiento debe ser 'Debito' o 'Credito'. Valor recibido: '{createMovimientoDto.TipoMovimiento}'");
                }

                var esDebito = tipoMovimientoNormalizado == "debito";
                decimal valorMovimiento = esDebito ? -Math.Abs(createMovimientoDto.Valor) : Math.Abs(createMovimientoDto.Valor);

                // Validaciones para débitos
                if (esDebito)
                {
                    // Validar saldo disponible
                    if (saldoActual + valorMovimiento < 0)
                    {
                        throw new BusinessException("Saldo no disponible");
                    }
                    
                    // Validar límite diario de retiros (débitos)
                    var limiteDiario = _businessRules.LimiteDiarioRetiro;
                    var totalDebitosHoy = await _unitOfWork.Movimientos.GetTotalDebitosDiarioAsync(cuenta.CuentaId, DateTime.Now);
                    var nuevoTotalDebitos = totalDebitosHoy + Math.Abs(createMovimientoDto.Valor);
                    
                    if (nuevoTotalDebitos > limiteDiario)
                    {
                        throw new BusinessException($"Cupo diario excedido. Débitos hoy: ${totalDebitosHoy:F2}, Intento: ${Math.Abs(createMovimientoDto.Valor):F2}, Límite: ${limiteDiario:F2}");
                    }
                }

                // Crear el movimiento
                var movimiento = new Movimiento
                {
                    CuentaId = cuenta.CuentaId,
                    Fecha = DateTime.Now,
                    TipoMovimiento = esDebito ? "Debito" : "Credito",
                    Valor = valorMovimiento,
                    Saldo = saldoActual + valorMovimiento
                };

                await _unitOfWork.Movimientos.AddAsync(movimiento);

                // Commit transaction (this will also save changes)
                await _unitOfWork.CommitTransactionAsync();

                // Mapear después del commit
                var result = new MovimientoDto
                {
                    Fecha = movimiento.Fecha,
                    TipoMovimiento = movimiento.TipoMovimiento,
                    Valor = movimiento.Valor,
                    Saldo = movimiento.Saldo,
                    NumeroCuenta = cuenta.NumeroCuenta
                };

                return result;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }


    }
}