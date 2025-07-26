using System.ComponentModel.DataAnnotations;

namespace devsu.DTOs
{
    public class ClienteDto
    {
        public int? ClienteId { get; set; }
        
        [Required]
        public string Nombre { get; set; }
        
        [Required]
        public string Genero { get; set; }
        
        [Required]
        public int Edad { get; set; }
        
        [Required]
        public string Identificacion { get; set; }
        
        public string Direccion { get; set; }
        
        public string Telefono { get; set; }
        
        [Required]
        public string Contrasena { get; set; }
        
        public bool Estado { get; set; } = true; // Por defecto, el cliente está activo
    }

    public class ClientePatchDto
    {
        public string? Nombre { get; set; }
        public string? Genero { get; set; }
        public int? Edad { get; set; }
        public string? Identificacion { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Contrasena { get; set; }
        public bool? Estado { get; set; }
    }
}