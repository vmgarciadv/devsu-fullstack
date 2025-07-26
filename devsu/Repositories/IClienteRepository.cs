using System.Threading.Tasks;
using devsu.Models;

namespace devsu.Repositories
{
    public interface IClienteRepository : IGenericRepository<Cliente>
    {
        Task<Cliente> GetByIdentificacionAsync(string identificacion);
        Task<Cliente> GetByNombreAsync(string nombre);
    }
}