using System.ComponentModel.DataAnnotations;
using devsu.Validators;

namespace devsu.DTOs
{
    public class CuentaDto
    {
        public int NumeroCuenta { get; set; }
        
        [Required]
        [TipoCuenta]
        public string TipoCuenta { get; set; }
        
        [Required]
        public decimal SaldoInicial { get; set; }
        
        public bool Estado { get; set; } = true;
        
        [Required]
        public string NombreCliente { get; set; }
    }
    
    // DTO para operaciones PATCH - permite valores nulos
    public class CuentaPatchDto
    {
        public int? NumeroCuenta { get; set; }
        
        [TipoCuenta]
        public string? TipoCuenta { get; set; }
        
        public decimal? SaldoInicial { get; set; }
        public bool? Estado { get; set; }
    }
}