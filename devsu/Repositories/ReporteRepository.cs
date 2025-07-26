using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using devsu.Data;
using devsu.DTOs;

namespace devsu.Repositories
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly DevsuContext _context;

        public ReporteRepository(DevsuContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReporteDto>> GenerarReporteEstadoCuentaAsync(string clienteNombre, DateTime fechaInicio, DateTime fechaFin, int timezoneOffset = 0)
        {
            var reportes = new List<ReporteDto>();

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "sp_GenerarReporteEstadoCuenta";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@ClienteNombre", clienteNombre));
            command.Parameters.Add(new SqlParameter("@FechaInicio", fechaInicio));
            command.Parameters.Add(new SqlParameter("@FechaFin", fechaFin));
            command.Parameters.Add(new SqlParameter("@TimezoneOffset", timezoneOffset));

            await _context.Database.OpenConnectionAsync();

            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                reportes.Add(new ReporteDto
                {
                    Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                    Cliente = reader.GetString(reader.GetOrdinal("Cliente")),
                    NumeroCuenta = reader.GetInt32(reader.GetOrdinal("NumeroCuenta")),
                    TipoCuenta = reader.GetString(reader.GetOrdinal("TipoCuenta")),
                    SaldoInicial = reader.GetDecimal(reader.GetOrdinal("SaldoInicial")),
                    Estado = reader.GetBoolean(reader.GetOrdinal("Estado")),
                    TotalMovimientos = reader.GetDecimal(reader.GetOrdinal("TotalMovimientos")),
                    SaldoDisponible = reader.GetDecimal(reader.GetOrdinal("SaldoDisponible"))
                });
            }

            await _context.Database.CloseConnectionAsync();

            return reportes;
        }
    }
}