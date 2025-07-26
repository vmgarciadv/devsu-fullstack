using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using devsu.Data;
using devsu.Models;

namespace devsu.Repositories
{
    public class CuentaRepository : GenericRepository<Cuenta>, ICuentaRepository
    {
        public CuentaRepository(DevsuContext context) : base(context) { }
        
        public async Task<IEnumerable<Cuenta>> GetCuentasByClienteAsync(int clienteId)
        {
            return await _context.Cuentas
                .Where(c => c.ClienteId == clienteId)
                .Include(c => c.Cliente)
                .ToListAsync();
        }
        
        public async Task<Cuenta> GetByNumeroCuentaAsync(int numeroCuenta)
        {
            return await _context.Cuentas
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta);
        }
        
        public async Task<IEnumerable<Cuenta>> GetAllWithClienteAsync()
        {
            return await _context.Cuentas
                .Include(c => c.Cliente)
                .ToListAsync();
        }

        public async Task<Cuenta> GetCuentaWithMovimientosAsync(int id)
        {
            return await _context.Cuentas
                .Include(c => c.Movimientos)
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.CuentaId == id);
        }
    }
}