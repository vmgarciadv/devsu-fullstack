using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace devsu.Migrations
{
    /// <inheritdoc />
    public partial class AddReporteStoredProcedureAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create indexes for optimization
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Personas_Nombre' AND object_id = OBJECT_ID('Personas'))
                    CREATE INDEX IX_Personas_Nombre ON Personas(Nombre) INCLUDE (Id);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Movimientos_CuentaId_Fecha' AND object_id = OBJECT_ID('Movimientos'))
                    CREATE INDEX IX_Movimientos_CuentaId_Fecha ON Movimientos(CuentaId, Fecha) INCLUDE (Valor);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Cuentas_ClienteId' AND object_id = OBJECT_ID('Cuentas'))
                    CREATE INDEX IX_Cuentas_ClienteId ON Cuentas(ClienteId) INCLUDE (NumeroCuenta, TipoCuenta, SaldoInicial, Estado);
            ");

            // Create stored procedure
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop stored procedure
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GenerarReporteEstadoCuenta;");

            // Drop indexes
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Cuentas_ClienteId ON Cuentas;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Movimientos_CuentaId_Fecha ON Movimientos;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Personas_Nombre ON Personas;");
        }
    }
}
