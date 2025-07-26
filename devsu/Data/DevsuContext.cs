using Microsoft.EntityFrameworkCore;
using devsu.Models;

namespace devsu.Data
{
    public class DevsuContext : DbContext
    {
        public DevsuContext(DbContextOptions<DevsuContext> options) : base(options) { }
        
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<Movimiento> Movimientos { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar herencia TPH (Table Per Hierarchy)
            modelBuilder.Entity<Persona>()
                .HasDiscriminator<string>("PersonaType")
                .HasValue<Persona>("Persona")
                .HasValue<Cliente>("Cliente");
            
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Identificacion)
                .IsUnique();

            modelBuilder.Entity<Cuenta>()
                .HasIndex(c => c.NumeroCuenta)
                .IsUnique();

            modelBuilder.Entity<Cuenta>()
                .HasOne(c => c.Cliente)
                .WithMany(cl => cl.Cuentas)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Movimiento>()
                .HasOne(m => m.Cuenta)
                .WithMany(c => c.Movimientos)
                .HasForeignKey(m => m.CuentaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}