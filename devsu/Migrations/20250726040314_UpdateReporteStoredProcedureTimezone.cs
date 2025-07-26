using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace devsu.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReporteStoredProcedureTimezone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing stored procedure
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GenerarReporteEstadoCuenta;");

            // Create updated stored procedure with timezone parameter
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_GenerarReporteEstadoCuenta
                    @ClienteNombre NVARCHAR(100),
                    @FechaInicio DATETIME,
                    @FechaFin DATETIME,
                    @TimezoneOffset INT = 0
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    -- Ajustar las fechas con el timezone offset
                    DECLARE @FechaInicioAjustada DATETIME = DATEADD(HOUR, @TimezoneOffset, @FechaInicio);
                    DECLARE @FechaFinAjustada DATETIME = DATEADD(HOUR, @TimezoneOffset, @FechaFin);
                    
                    -- Only return accounts that have movements in the date range
                    SELECT 
                        MAX(m.Fecha) as Fecha,
                        p.Nombre as Cliente,
                        cu.NumeroCuenta,
                        cu.TipoCuenta,
                        cu.SaldoInicial,
                        cu.Estado,
                        SUM(m.Valor) as TotalMovimientos,
                        cu.SaldoInicial + SUM(m.Valor) as SaldoDisponible
                    FROM Cuentas cu
                    INNER JOIN Personas p ON cu.ClienteId = p.Id
                    INNER JOIN Movimientos m ON cu.CuentaId = m.CuentaId 
                    WHERE p.Nombre = @ClienteNombre 
                        AND p.PersonaType = 'Cliente'
                        AND m.Fecha >= @FechaInicioAjustada 
                        AND m.Fecha <= @FechaFinAjustada
                    GROUP BY p.Id, p.Nombre, cu.CuentaId, cu.NumeroCuenta, 
                             cu.TipoCuenta, cu.SaldoInicial, cu.Estado
                    ORDER BY cu.NumeroCuenta;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the updated stored procedure
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GenerarReporteEstadoCuenta;");

            // Recreate the original stored procedure without timezone
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_GenerarReporteEstadoCuenta
                    @ClienteNombre NVARCHAR(100),
                    @FechaInicio DATETIME,
                    @FechaFin DATETIME
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    -- Only return accounts that have movements in the date range
                    SELECT 
                        MAX(m.Fecha) as Fecha,
                        p.Nombre as Cliente,
                        cu.NumeroCuenta,
                        cu.TipoCuenta,
                        cu.SaldoInicial,
                        cu.Estado,
                        SUM(m.Valor) as TotalMovimientos,
                        cu.SaldoInicial + SUM(m.Valor) as SaldoDisponible
                    FROM Cuentas cu
                    INNER JOIN Personas p ON cu.ClienteId = p.Id
                    INNER JOIN Movimientos m ON cu.CuentaId = m.CuentaId 
                    WHERE p.Nombre = @ClienteNombre 
                        AND p.PersonaType = 'Cliente'
                        AND m.Fecha >= @FechaInicio 
                        AND m.Fecha <= @FechaFin
                    GROUP BY p.Id, p.Nombre, cu.CuentaId, cu.NumeroCuenta, 
                             cu.TipoCuenta, cu.SaldoInicial, cu.Estado
                    ORDER BY cu.NumeroCuenta;
                END
            ");
        }
    }
}
