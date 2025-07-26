using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using devsu.DTOs;

namespace devsu.Repositories
{
    public interface IReporteRepository
    {
        Task<IEnumerable<ReporteDto>> GenerarReporteEstadoCuentaAsync(string clienteNombre, DateTime fechaInicio, DateTime fechaFin, int timezoneOffset = 0);
    }
}