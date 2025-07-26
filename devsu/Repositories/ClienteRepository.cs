using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using devsu.Data;
using devsu.Models;

namespace devsu.Repositories
{
    public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
    {
        public ClienteRepository(DevsuContext context) : base(context) { }
        
        public async Task<Cliente> GetByIdentificacionAsync(string identificacion)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.Identificacion == identificacion);
        }
        
        public async Task<Cliente> GetByNombreAsync(string nombre)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.Nombre == nombre);
        }
    }
}