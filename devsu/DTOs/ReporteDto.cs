using System;

namespace devsu.DTOs
{
    public class ReporteDto
    {
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public int NumeroCuenta { get; set; }
        public string TipoCuenta { get; set; } = string.Empty;
        public decimal SaldoInicial { get; set; }
        public bool Estado { get; set; }
        public decimal TotalMovimientos { get; set; }
        public decimal SaldoDisponible { get; set; }
    }

    public class ReporteRequestDto
    {
        public string Cliente { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TimezoneOffset { get; set; } = 0;
    }
}