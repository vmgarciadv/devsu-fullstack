using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace devsu.Models
{
    public class Movimiento
    {
        [Key]
        public int MovimientoId { get; set; }
        
        [Required]
        public DateTime Fecha { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string TipoMovimiento { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Saldo { get; set; }
        
        // Foreign Key
        public int CuentaId { get; set; }
        
        // Navegación
        [ForeignKey("CuentaId")]
        public virtual Cuenta Cuenta { get; set; }
    }
}