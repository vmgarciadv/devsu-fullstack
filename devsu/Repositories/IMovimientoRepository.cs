using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using devsu.Models;

namespace devsu.Repositories
{
    public interface IMovimientoRepository : IGenericRepository<Movimiento>
    {
        Task<IEnumerable<Movimiento>> GetMovimientosByFechaAsync(int cuentaId, DateTime fechaInicio, DateTime fechaFin);
        Task<decimal> GetTotalDebitosDiarioAsync(int cuentaId, DateTime fecha);
        Task<Movimiento> GetLastMovimientoByCuentaAsync(int cuentaId);
        Task<IEnumerable<Movimiento>> GetAllWithCuentaAsync();
    }
}