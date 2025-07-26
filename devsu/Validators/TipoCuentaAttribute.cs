using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace devsu.Validators
{
    public class TipoCuentaAttribute : ValidationAttribute
    {
        private readonly string[] _validTypes = { "Ahorro", "Corriente" };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("TipoCuenta es requerido");
            }

            string tipoCuenta = value.ToString().Trim();
            
            // Convertir a minúsculas para comparación
            string normalizedValue = tipoCuenta.ToLower();
            
            // Solo aceptar exactamente "ahorro" o "corriente" (sin 's' al final)
            if (normalizedValue != "ahorro" && normalizedValue != "corriente")
            {
                return new ValidationResult($"TipoCuenta debe ser exactamente 'Ahorro' o 'Corriente'. Valor recibido: '{tipoCuenta}'");
            }

            return ValidationResult.Success;
        }
    }
}