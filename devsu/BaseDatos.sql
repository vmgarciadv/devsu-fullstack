-- =============================================
-- Script de Base de Datos: devsu
-- Fecha: 2025-01-25
-- Descripción: Script completo para crear la base de datos, 
--              tablas, índices, procedimientos almacenados 
--              y datos de ejemplo
-- =============================================

-- Crear base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'devsu')
BEGIN
    CREATE DATABASE devsu;
END
GO

USE devsu;
GO

-- =============================================
-- ELIMINAR OBJETOS SI EXISTEN
-- =============================================

-- Eliminar procedimiento almacenado si existe
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GenerarReporteEstadoCuenta]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_GenerarReporteEstadoCuenta];
GO

-- Eliminar tablas en orden inverso de dependencias
IF OBJECT_ID('dbo.Movimientos', 'U') IS NOT NULL DROP TABLE dbo.Movimientos;
IF OBJECT_ID('dbo.Cuentas', 'U') IS NOT NULL DROP TABLE dbo.Cuentas;
IF OBJECT_ID('dbo.Personas', 'U') IS NOT NULL DROP TABLE dbo.Personas;
GO

-- =============================================
-- CREAR TABLAS
-- =============================================

-- Tabla Personas (incluye herencia TPH para Cliente)
CREATE TABLE Personas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Genero NVARCHAR(1) NOT NULL CHECK (Genero IN ('M', 'F')),
    Edad INT NOT NULL CHECK (Edad >= 0 AND Edad <= 110),
    Identificacion NVARCHAR(20) NOT NULL,
    Direccion NVARCHAR(200) NOT NULL,
    Telefono NVARCHAR(20) NOT NULL,
    PersonaType NVARCHAR(50) NOT NULL DEFAULT 'Persona',
    -- Campos específicos de Cliente
    Contrasena NVARCHAR(50) NULL,
    Estado BIT NULL
);
GO

-- Crear índice único para identificación
CREATE UNIQUE INDEX IX_Personas_Identificacion ON Personas(Identificacion) WHERE PersonaType = 'Cliente';
GO

-- Tabla Cuentas
CREATE TABLE Cuentas (
    CuentaId INT IDENTITY(1,1) PRIMARY KEY,
    NumeroCuenta INT NOT NULL,
    TipoCuenta NVARCHAR(20) NOT NULL CHECK (TipoCuenta IN ('Ahorro', 'Corriente')),
    SaldoInicial DECIMAL(18,2) NOT NULL,
    Estado BIT NOT NULL,
    ClienteId INT NOT NULL,
    CONSTRAINT FK_Cuentas_Clientes FOREIGN KEY (ClienteId) REFERENCES Personas(Id),
    CONSTRAINT UQ_NumeroCuenta UNIQUE (NumeroCuenta)
);
GO

-- Tabla Movimientos
CREATE TABLE Movimientos (
    MovimientoId INT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATETIME NOT NULL,
    TipoMovimiento NVARCHAR(20) NOT NULL CHECK (TipoMovimiento IN ('Deposito', 'Retiro')),
    Valor DECIMAL(18,2) NOT NULL,
    Saldo DECIMAL(18,2) NOT NULL,
    CuentaId INT NOT NULL,
    CONSTRAINT FK_Movimientos_Cuentas FOREIGN KEY (CuentaId) REFERENCES Cuentas(CuentaId)
);
GO

-- =============================================
-- CREAR ÍNDICES PARA OPTIMIZACIÓN
-- =============================================

CREATE INDEX IX_Personas_Nombre ON Personas(Nombre) INCLUDE (Id);
CREATE INDEX IX_Movimientos_CuentaId_Fecha ON Movimientos(CuentaId, Fecha) INCLUDE (Valor);
CREATE INDEX IX_Cuentas_ClienteId ON Cuentas(ClienteId) INCLUDE (NumeroCuenta, TipoCuenta, SaldoInicial, Estado);
GO

-- =============================================
-- CREAR PROCEDIMIENTO ALMACENADO
-- =============================================

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
    
    -- Solo retornar cuentas que tienen movimientos en el rango de fechas
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
GO

-- =============================================
-- INSERTAR DATOS DE EJEMPLO
-- =============================================

-- Insertar 25 Clientes
INSERT INTO Personas (Nombre, Genero, Edad, Identificacion, Direccion, Telefono, PersonaType, Contrasena, Estado) VALUES
('Juan Carlos Pérez', 'M', 28, '1234567890', 'Av. Principal 123, Quito', '0991234567', 'Cliente', 'password123', 1),
('María Elena González', 'F', 35, '0987654321', 'Calle Secundaria 456, Guayaquil', '0998765432', 'Cliente', 'pass456', 1),
('Roberto Sánchez López', 'M', 42, '1122334455', 'Av. Los Alamos 789, Cuenca', '0991122334', 'Cliente', 'rob123', 1),
('Ana Patricia Martínez', 'F', 29, '2233445566', 'Calle Las Flores 321, Loja', '0992233445', 'Cliente', 'ana789', 1),
('Carlos Alberto Rodríguez', 'M', 51, '3344556677', 'Av. Amazonas 654, Quito', '0993344556', 'Cliente', 'car456', 1),
('Laura Sofía Jiménez', 'F', 26, '4455667788', 'Calle El Sol 987, Guayaquil', '0994455667', 'Cliente', 'lau123', 1),
('Diego Fernando Vargas', 'M', 38, '5566778899', 'Av. La Paz 147, Ambato', '0995566778', 'Cliente', 'die789', 1),
('Gabriela Isabel Torres', 'F', 45, '6677889900', 'Calle Luna 258, Manta', '0996677889', 'Cliente', 'gab456', 1),
('Miguel Ángel Flores', 'M', 33, '7788990011', 'Av. Central 369, Portoviejo', '0997788990', 'Cliente', 'mig123', 1),
('Patricia Andrea Ramos', 'F', 31, '8899001122', 'Calle Norte 741, Machala', '0998899001', 'Cliente', 'pat789', 1),
('José Luis Mendoza', 'M', 47, '9900112233', 'Av. Sur 852, Esmeraldas', '0999900112', 'Cliente', 'jos456', 1),
('Claudia Marcela Castro', 'F', 39, '1010101010', 'Calle Este 963, Ibarra', '0991010101', 'Cliente', 'cla123', 1),
('Ricardo Andrés Morales', 'M', 24, '1111111111', 'Av. Oeste 159, Santo Domingo', '0991111111', 'Cliente', 'ric789', 1),
('Mónica Elizabeth Gutiérrez', 'F', 36, '1212121212', 'Calle Principal 753, Riobamba', '0991212121', 'Cliente', 'mon456', 1),
('Fernando José Herrera', 'M', 44, '1313131313', 'Av. Simón Bolívar 357, Latacunga', '0991313131', 'Cliente', 'fer123', 1),
('Verónica Alexandra Díaz', 'F', 27, '1414141414', 'Calle Comercial 951, Tulcán', '0991414141', 'Cliente', 'ver789', 1),
('Alejandro David Silva', 'M', 40, '1515151515', 'Av. Kennedy 753, Quito', '0991515151', 'Cliente', 'ale456', 1),
('Carolina Andrea Romero', 'F', 34, '1616161616', 'Calle Industrial 159, Guayaquil', '0991616161', 'Cliente', 'car123', 1),
('Pablo Esteban Navarro', 'M', 30, '1717171717', 'Av. Universitaria 357, Cuenca', '0991717171', 'Cliente', 'pab789', 1),
('Sandra Milena Aguilar', 'F', 43, '1818181818', 'Calle Residencial 753, Loja', '0991818181', 'Cliente', 'san456', 1),
('Luis Eduardo Vega', 'M', 37, '1919191919', 'Av. Libertad 951, Ambato', '0991919191', 'Cliente', 'lui123', 1),
('Teresa Isabel Medina', 'F', 32, '2020202020', 'Calle Victoria 147, Manta', '0992020202', 'Cliente', 'ter789', 1),
('Andrés Felipe Guerrero', 'M', 25, '2121212121', 'Av. República 258, Portoviejo', '0992121212', 'Cliente', 'and456', 1),
('Daniela Victoria Cruz', 'F', 41, '2222222222', 'Calle Colón 369, Machala', '0992222222', 'Cliente', 'dan123', 1),
('Santiago Nicolás Peña', 'M', 46, '2323232323', 'Av. Malecón 741, Esmeraldas', '0992323232', 'Cliente', 'san789', 1),
('Victor', 'M', 30, '2424242424', 'Av. Test 123, Quito', '0992424242', 'Cliente', 'vic123', 1);
GO

-- Insertar 25 Cuentas (al menos una por cliente)
INSERT INTO Cuentas (NumeroCuenta, TipoCuenta, SaldoInicial, Estado, ClienteId) VALUES
(100001, 'Ahorro', 1500.00, 1, 1),
(100002, 'Corriente', 2500.00, 1, 2),
(100003, 'Ahorro', 3000.00, 1, 3),
(100004, 'Corriente', 1800.00, 1, 4),
(100005, 'Ahorro', 5000.00, 1, 5),
(100006, 'Corriente', 2200.00, 1, 6),
(100007, 'Ahorro', 3500.00, 1, 7),
(100008, 'Corriente', 4100.00, 1, 8),
(100009, 'Ahorro', 1200.00, 1, 9),
(100010, 'Corriente', 2800.00, 1, 10),
(100011, 'Ahorro', 3300.00, 1, 11),
(100012, 'Corriente', 1900.00, 1, 12),
(100013, 'Ahorro', 4500.00, 1, 13),
(100014, 'Corriente', 2600.00, 1, 14),
(100015, 'Ahorro', 3700.00, 1, 15),
(100016, 'Corriente', 4300.00, 1, 16),
(100017, 'Ahorro', 1600.00, 1, 17),
(100018, 'Corriente', 2900.00, 1, 18),
(100019, 'Ahorro', 3400.00, 1, 19),
(100020, 'Corriente', 2000.00, 1, 20),
(100021, 'Ahorro', 4800.00, 1, 21),
(100022, 'Corriente', 2700.00, 1, 22),
(100023, 'Ahorro', 3800.00, 1, 23),
(100024, 'Corriente', 4400.00, 1, 24),
(100025, 'Ahorro', 1700.00, 1, 25),
(706630, 'Corriente', 500.00, 1, 26);  -- Cuenta para Victor
GO

-- Insertar 25+ Movimientos (múltiples por cuenta)
INSERT INTO Movimientos (Fecha, TipoMovimiento, Valor, Saldo, CuentaId) VALUES
-- Movimientos para cuenta 1
('2025-01-15 10:30:00', 'Deposito', 500.00, 2000.00, 1),
('2025-01-16 14:20:00', 'Retiro', -200.00, 1800.00, 1),
('2025-01-20 09:15:00', 'Deposito', 300.00, 2100.00, 1),

-- Movimientos para cuenta 2
('2025-01-10 11:45:00', 'Retiro', -500.00, 2000.00, 2),
('2025-01-18 16:30:00', 'Deposito', 1000.00, 3000.00, 2),
('2025-01-22 13:20:00', 'Retiro', -300.00, 2700.00, 2),

-- Movimientos para cuenta 3
('2025-01-12 08:00:00', 'Deposito', 800.00, 3800.00, 3),
('2025-01-19 15:40:00', 'Retiro', -400.00, 3400.00, 3),

-- Movimientos para cuenta 4
('2025-01-14 10:10:00', 'Retiro', -300.00, 1500.00, 4),
('2025-01-21 12:50:00', 'Deposito', 600.00, 2100.00, 4),

-- Movimientos para cuenta 5
('2025-01-11 09:30:00', 'Deposito', 1500.00, 6500.00, 5),
('2025-01-17 14:15:00', 'Retiro', -1000.00, 5500.00, 5),
('2025-01-23 11:00:00', 'Deposito', 500.00, 6000.00, 5),

-- Movimientos para cuenta 6
('2025-01-13 13:25:00', 'Retiro', -200.00, 2000.00, 6),
('2025-01-20 16:45:00', 'Deposito', 400.00, 2400.00, 6),

-- Movimientos para cuenta 7
('2025-01-15 11:20:00', 'Deposito', 700.00, 4200.00, 7),
('2025-01-22 09:40:00', 'Retiro', -500.00, 3700.00, 7),

-- Movimientos para cuenta 8
('2025-01-16 15:00:00', 'Retiro', -600.00, 3500.00, 8),
('2025-01-24 10:30:00', 'Deposito', 900.00, 4400.00, 8),

-- Movimientos para cuenta 9
('2025-01-12 12:10:00', 'Deposito', 200.00, 1400.00, 9),
('2025-01-18 14:50:00', 'Retiro', -150.00, 1250.00, 9),

-- Movimientos para cuenta 10
('2025-01-14 08:45:00', 'Retiro', -400.00, 2400.00, 10),
('2025-01-21 11:30:00', 'Deposito', 600.00, 3000.00, 10),

-- Movimientos para cuenta 11
('2025-01-17 13:15:00', 'Deposito', 300.00, 3600.00, 11),

-- Movimientos para cuenta 12
('2025-01-19 10:00:00', 'Retiro', -100.00, 1800.00, 12),

-- Movimientos para cuenta 13
('2025-01-15 16:20:00', 'Deposito', 1000.00, 5500.00, 13),

-- Movimientos adicionales
('2025-01-20 12:30:00', 'Retiro', -250.00, 2350.00, 14),
('2025-01-22 15:10:00', 'Deposito', 450.00, 4150.00, 15),
('2025-01-23 09:50:00', 'Retiro', -300.00, 4000.00, 16),
('2025-01-24 14:30:00', 'Deposito', 200.00, 1800.00, 17),
('2025-01-25 11:15:00', 'Retiro', -400.00, 2500.00, 18),

-- Movimientos para Victor (cuenta 26)
('2025-07-25 10:30:00', 'Deposito', 100.00, 600.00, 26),
('2025-07-25 14:45:00', 'Retiro', -50.00, 550.00, 26),
('2025-07-26 08:15:00', 'Retiro', -200.00, 350.00, 26),
('2025-07-26 16:20:00', 'Deposito', 150.00, 500.00, 26);
GO

-- =============================================
-- VERIFICAR DATOS INSERTADOS
-- =============================================

-- Contar registros insertados
SELECT 'Clientes' as Tabla, COUNT(*) as Total FROM Personas WHERE PersonaType = 'Cliente'
UNION ALL
SELECT 'Cuentas', COUNT(*) FROM Cuentas
UNION ALL
SELECT 'Movimientos', COUNT(*) FROM Movimientos;
GO

-- Ejemplo de uso del procedimiento almacenado
-- EXEC sp_GenerarReporteEstadoCuenta 'Juan Carlos Pérez', '2025-01-01', '2025-01-31', 0;
-- EXEC sp_GenerarReporteEstadoCuenta 'Juan Carlos Pérez', '2025-01-01', '2025-01-31', -4; -- Con timezone offset de -4 horas