using System.Collections.Generic;
using System.Threading.Tasks;
using devsu.DTOs;

namespace devsu.Services
{
    public interface IReporteService
    {
        Task<IEnumerable<ReporteDto>> GenerarReporteEstadoCuentaAsync(ReporteRequestDto request);
    }
}