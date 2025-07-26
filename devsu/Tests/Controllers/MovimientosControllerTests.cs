using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using devsu.Controllers;
using devsu.DTOs;
using devsu.Services;

namespace Tests.Controllers
{
    public class MovimientosControllerTests
    {
        private readonly Mock<IMovimientoService> _mockMovimientoService;
        private readonly MovimientosController _controller;

        public MovimientosControllerTests()
        {
            _mockMovimientoService = new Mock<IMovimientoService>();
            _controller = new MovimientosController(_mockMovimientoService.Object);
        }

        #region GetMovimientos Tests

        [Fact]
        public async Task GetMovimientos_ReturnsOkResult_WithListOfMovimientos()
        {
            var movimientos = new List<MovimientoDto>
            {
                new() { 
                    Fecha = DateTime.Now,
                    TipoMovimiento = "Credito",
                    Valor = 1000,
                    Saldo = 2000,
                    NumeroCuenta = 123456
                },
                new() { 
                    Fecha = DateTime.Now,
                    TipoMovimiento = "Debito",
                    Valor = -500,
                    Saldo = 1500,
                    NumeroCuenta = 123456
                }
            };
            _mockMovimientoService.Setup(service => service.GetAllMovimientosAsync())
                .ReturnsAsync(movimientos);

            var result = await _controller.GetMovimientos();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedMovimientos = okResult.Value.Should().BeAssignableTo<IEnumerable<MovimientoDto>>().Subject;
            returnedMovimientos.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetMovimientos_ReturnsOkResult_WithEmptyList_WhenNoMovimientosExist()
        {
            var movimientos = new List<MovimientoDto>();
            _mockMovimientoService.Setup(service => service.GetAllMovimientosAsync())
                .ReturnsAsync(movimientos);

            var result = await _controller.GetMovimientos();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedMovimientos = okResult.Value.Should().BeAssignableTo<IEnumerable<MovimientoDto>>().Subject;
            returnedMovimientos.Should().BeEmpty();
        }

        #endregion

        #region GetMovimiento Tests

        [Fact]
        public async Task GetMovimiento_ReturnsOkResult_WhenMovimientoExists()
        {
            var movimiento = new MovimientoDto 
            { 
                Fecha = DateTime.Now,
                TipoMovimiento = "Credito",
                Valor = 1000,
                Saldo = 2000,
                NumeroCuenta = 123456
            };
            _mockMovimientoService.Setup(service => service.GetMovimientoByIdAsync(1))
                .ReturnsAsync(movimiento);

            var result = await _controller.GetMovimiento(1);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedMovimiento = okResult.Value.Should().BeOfType<MovimientoDto>().Subject;
            returnedMovimiento.TipoMovimiento.Should().Be("Credito");
            returnedMovimiento.Valor.Should().Be(1000);
        }

        [Fact]
        public async Task GetMovimiento_ReturnsNotFound_WhenMovimientoDoesNotExist()
        {
            _mockMovimientoService.Setup(service => service.GetMovimientoByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.GetMovimiento(999);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region CreateMovimiento Tests

        [Fact]
        public async Task CreateMovimiento_ReturnsCreated_WhenSuccessful()
        {
            var createMovimientoDto = new CreateMovimientoDto 
            { 
                TipoMovimiento = "Credito",
                Valor = 1000,
                NumeroCuenta = 123456
            };
            var createdMovimiento = new MovimientoDto 
            { 
                Fecha = DateTime.Now,
                TipoMovimiento = "Credito",
                Valor = 1000,
                Saldo = 2000,
                NumeroCuenta = 123456
            };
            _mockMovimientoService.Setup(service => service.CreateMovimientoAsync(It.IsAny<CreateMovimientoDto>()))
                .ReturnsAsync(createdMovimiento);

            var result = await _controller.CreateMovimiento(createMovimientoDto);

            var createdResult = result.Result.Should().BeOfType<CreatedResult>().Subject;
            var returnedMovimiento = createdResult.Value.Should().BeOfType<MovimientoDto>().Subject;
            returnedMovimiento.TipoMovimiento.Should().Be("Credito");
            returnedMovimiento.Valor.Should().Be(1000);
        }

        [Fact]
        public async Task CreateMovimiento_ReturnsBadRequest_WhenCuentaNotFound()
        {
            var createMovimientoDto = new CreateMovimientoDto 
            { 
                TipoMovimiento = "Credito",
                Valor = 1000,
                NumeroCuenta = 999999
            };
            _mockMovimientoService.Setup(service => service.CreateMovimientoAsync(It.IsAny<CreateMovimientoDto>()))
                .ThrowsAsync(new KeyNotFoundException("Cuenta con número 999999 no encontrada"));

            var result = await _controller.CreateMovimiento(createMovimientoDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Cuenta con número 999999 no encontrada");
        }

        [Fact]
        public async Task CreateMovimiento_ReturnsBadRequest_WhenSaldoInsuficiente()
        {
            var createMovimientoDto = new CreateMovimientoDto 
            { 
                TipoMovimiento = "Debito",
                Valor = 5000,
                NumeroCuenta = 123456
            };
            _mockMovimientoService.Setup(service => service.CreateMovimientoAsync(It.IsAny<CreateMovimientoDto>()))
                .ThrowsAsync(new InvalidOperationException("Saldo no disponible"));

            var result = await _controller.CreateMovimiento(createMovimientoDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Saldo no disponible");
        }

        [Fact]
        public async Task CreateMovimiento_ReturnsBadRequest_WhenLimiteDiarioExcedido()
        {
            var createMovimientoDto = new CreateMovimientoDto 
            { 
                TipoMovimiento = "Debito",
                Valor = 500,
                NumeroCuenta = 123456
            };
            _mockMovimientoService.Setup(service => service.CreateMovimientoAsync(It.IsAny<CreateMovimientoDto>()))
                .ThrowsAsync(new InvalidOperationException("Cupo diario excedido. Débitos hoy: $800.00, Intento: $500.00, Límite: $1000.00"));

            var result = await _controller.CreateMovimiento(createMovimientoDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.ToString().Should().Contain("Cupo diario excedido");
        }

        [Fact]
        public async Task CreateMovimiento_ReturnsBadRequest_WhenTipoMovimientoInvalido()
        {
            var createMovimientoDto = new CreateMovimientoDto 
            { 
                TipoMovimiento = "Transferencia",
                Valor = 1000,
                NumeroCuenta = 123456
            };
            _mockMovimientoService.Setup(service => service.CreateMovimientoAsync(It.IsAny<CreateMovimientoDto>()))
                .ThrowsAsync(new ArgumentException("TipoMovimiento debe ser 'Debito' o 'Credito'. Valor recibido: 'Transferencia'"));

            var result = await _controller.CreateMovimiento(createMovimientoDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.ToString().Should().Contain("TipoMovimiento debe ser");
        }

        #endregion
    }
}