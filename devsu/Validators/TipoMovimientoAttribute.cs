using System;
using System.ComponentModel.DataAnnotations;

namespace devsu.Validators
{
    public class TipoMovimientoAttribute : ValidationAttribute
    {
        private readonly string[] _validTypes = { "Debito", "Credito" };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("TipoMovimiento es requerido");
            }

            string tipoMovimiento = value.ToString().Trim();
            
            // Convertir a minúsculas para comparación
            string normalizedValue = tipoMovimiento.ToLower();
            
            // Solo aceptar exactamente "debito" o "credito"
            if (normalizedValue != "debito" && normalizedValue != "credito")
            {
                return new ValidationResult($"TipoMovimiento debe ser exactamente 'Debito' o 'Credito'. Valor recibido: '{tipoMovimiento}'");
            }

            return ValidationResult.Success;
        }
    }
}