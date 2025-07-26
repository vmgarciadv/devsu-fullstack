using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace devsu.Models
{
    public class Cliente : Persona
    {
        [Required]
        [MaxLength(50)]
        public string Contrasena { get; set; }

        [Required]
        public bool Estado { get; set; }

        // Navegación
        public virtual ICollection<Cuenta> Cuentas { get; set; }
        
        public Cliente()
        {
            Cuentas = new HashSet<Cuenta>();
        }
    }
}