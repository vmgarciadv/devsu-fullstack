using System;
using System.Linq;
using System.Threading.Tasks;
using devsu.DTOs;
using devsu.Exceptions;
using devsu.Repositories;

namespace devsu.Services
{
    public class ReporteService : IReporteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReporteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ReporteDto>> GenerarReporteEstadoCuentaAsync(ReporteRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Cliente))
            {
                throw new BusinessException("El nombre del cliente es requerido");
            }

            if (request.FechaInicio > request.FechaFin)
            {
                throw new BusinessException("La fecha de inicio no puede ser mayor a la fecha fin");
            }

            var reportes = await _unitOfWork.Reportes.GenerarReporteEstadoCuentaAsync(
                request.Cliente, 
                request.FechaInicio, 
                request.FechaFin,
                request.TimezoneOffset
            );

            var reportesList = reportes.ToList();

            if (!reportesList.Any())
            {
                throw new NotFoundException($"No se encontraron movimientos para el cliente {request.Cliente} en el rango de fechas especificado");
            }

            return reportesList;
        }
    }
}