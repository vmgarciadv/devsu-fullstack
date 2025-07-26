using System;
using System.Threading.Tasks;

namespace devsu.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IClienteRepository Clientes { get; }
        ICuentaRepository Cuentas { get; }
        IMovimientoRepository Movimientos { get; }
        IReporteRepository Reportes { get; }
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}