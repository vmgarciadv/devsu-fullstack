using System;
using System.Threading.Tasks;
using devsu.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace devsu.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DevsuContext _context;
        private IDbContextTransaction _transaction;
        
        public IClienteRepository Clientes { get; private set; }
        public ICuentaRepository Cuentas { get; private set; }
        public IMovimientoRepository Movimientos { get; private set; }
        public IReporteRepository Reportes { get; private set; }
        
        public UnitOfWork(DevsuContext context)
        {
            _context = context;
            Clientes = new ClienteRepository(_context);
            Cuentas = new CuentaRepository(_context);
            Movimientos = new MovimientoRepository(_context);
            Reportes = new ReporteRepository(_context);
        }
        
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
        
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }
        
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }
        
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        
        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}