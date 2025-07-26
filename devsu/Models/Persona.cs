using System.ComponentModel.DataAnnotations;

namespace devsu.Models
{
    public class Persona
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(1)]
        public string Genero { get; set; }

        [Required]
        [Range(0, 110)]
        public int Edad { get; set; }

        [Required]
        [MaxLength(20)]
        public string Identificacion { get; set; }

        [Required]
        [MaxLength(200)]
        public string Direccion { get; set; }

        [Required]
        [MaxLength(20)]
        public string Telefono { get; set; }
    }
}