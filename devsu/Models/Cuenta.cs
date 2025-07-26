using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using devsu.Validators;

namespace devsu.Models
{
    public class Cuenta
    {
        [Key]
        public int CuentaId { get; set; }
        
        [Required]
        [MaxLength(20)]
        public int NumeroCuenta { get; set; }
        
        [Required]
        [MaxLength(20)]
        [TipoCuenta]
        public string TipoCuenta { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoInicial { get; set; }
        
        [Required]
        public bool Estado { get; set; }
        
        // Foreign Key
        public int ClienteId { get; set; }
        
        // Navegación
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        public virtual ICollection<Movimiento> Movimientos { get; set; }
        
        public Cuenta()
        {
            Movimientos = new HashSet<Movimiento>();
        }
    }
}