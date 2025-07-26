using System;
using System.ComponentModel.DataAnnotations;
using devsu.Validators;

namespace devsu.DTOs
{
    public class MovimientoDto
    {
        public DateTime Fecha { get; set; }
        
        [Required]
        [TipoMovimiento]
        public string TipoMovimiento { get; set; }
        
        [Required]
        public decimal Valor { get; set; }
        
        public decimal Saldo { get; set; }
        
        public int NumeroCuenta { get; set; }
    }

    public class CreateMovimientoDto
    {
        [Required]
        [TipoMovimiento]
        public string TipoMovimiento { get; set; }
        
        [Required]
        public decimal Valor { get; set; }
        
        [Required]
        public int NumeroCuenta { get; set; }
    }
}