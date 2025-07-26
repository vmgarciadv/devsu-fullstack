using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using devsu.Data;
using devsu.Models;

namespace devsu.Repositories
{
    public class MovimientoRepository : GenericRepository<Movimiento>, IMovimientoRepository
    {
        public MovimientoRepository(DevsuContext context) : base(context) { }
        
        public async Task<IEnumerable<Movimiento>> GetMovimientosByFechaAsync(int cuentaId, DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Movimientos
                .Where(m => m.CuentaId == cuentaId && 
                           m.Fecha >= fechaInicio && 
                           m.Fecha <= fechaFin)
                .OrderBy(m => m.Fecha)
                .ToListAsync();
        }
        
        public async Task<decimal> GetTotalDebitosDiarioAsync(int cuentaId, DateTime fecha)
        {
            var inicioDia = fecha.Date;
            var finDia = fecha.Date.AddDays(1);
            
            var total = await _context.Movimientos
                .Where(m => m.CuentaId == cuentaId && 
                           m.TipoMovimiento.ToLower() == "debito" &&
                           m.Fecha >= inicioDia && 
                           m.Fecha < finDia)
                .SumAsync(m => Math.Abs(m.Valor));
                
            return total;
        }
        
        public async Task<Movimiento> GetLastMovimientoByCuentaAsync(int cuentaId)
        {
            return await _context.Movimientos
                .Where(m => m.CuentaId == cuentaId)
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.MovimientoId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Movimiento>> GetAllWithCuentaAsync()
        {
            return await _context.Movimientos
                .Include(m => m.Cuenta)
                .ToListAsync();
        }
    }
}