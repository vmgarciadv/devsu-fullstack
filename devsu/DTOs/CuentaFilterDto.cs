namespace devsu.DTOs
{
    public class CuentaFilterDto : PaginationParameters
    {
        public string? Q { get; set; }
        public int? NumeroCuenta { get; set; }
        public string? TipoCuenta { get; set; }
        public decimal? SaldoInicial { get; set; }
        public bool? Estado { get; set; }
        public string? NombreCliente { get; set; }
    }
}