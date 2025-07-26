using System;

namespace devsu.DTOs
{
    public class MovimientoFilterDto : PaginationParameters
    {
        public string? Q { get; set; }
        public DateTime? Fecha { get; set; }
        public string? TipoMovimiento { get; set; }
        public decimal? Valor { get; set; }
        public decimal? Saldo { get; set; }
        public int? NumeroCuenta { get; set; }
        public int Timezone { get; set; } = 0;
    }
}