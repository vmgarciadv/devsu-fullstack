using System.Collections.Generic;
using System.Threading.Tasks;
using devsu.Models;

namespace devsu.Repositories
{
    public interface ICuentaRepository : IGenericRepository<Cuenta>
    {
        Task<IEnumerable<Cuenta>> GetCuentasByClienteAsync(int clienteId);
        Task<Cuenta> GetByNumeroCuentaAsync(int numeroCuenta);
        Task<IEnumerable<Cuenta>> GetAllWithClienteAsync();
        Task<Cuenta> GetCuentaWithMovimientosAsync(int id);
    }
}