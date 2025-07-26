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
    public class CuentasControllerTests
    {
        private readonly Mock<ICuentaService> _mockCuentaService;
        private readonly CuentasController _controller;

        public CuentasControllerTests()
        {
            _mockCuentaService = new Mock<ICuentaService>();
            _controller = new CuentasController(_mockCuentaService.Object);
        }

        #region GetCuentas Tests

        [Fact]
        public async Task GetCuentas_ReturnsOkResult_WithListOfCuentas()
        {
            var cuentas = new List<CuentaDto>
            {
                new CuentaDto { NumeroCuenta = 123456, TipoCuenta = "Ahorro", Estado = true, NombreCliente = "Juan Pérez" },
                new CuentaDto { NumeroCuenta = 789012, TipoCuenta = "Corriente", Estado = true, NombreCliente = "María García" }
            };
            _mockCuentaService.Setup(service => service.GetAllCuentasAsync())
                .ReturnsAsync(cuentas);

            var result = await _controller.GetCuentas();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedCuentas = okResult.Value.Should().BeAssignableTo<IEnumerable<CuentaDto>>().Subject;
            returnedCuentas.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetCuentas_ReturnsOkResult_WithEmptyList_WhenNoCuentasExist()
        {
            var cuentas = new List<CuentaDto>();
            _mockCuentaService.Setup(service => service.GetAllCuentasAsync())
                .ReturnsAsync(cuentas);

            var result = await _controller.GetCuentas();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedCuentas = okResult.Value.Should().BeAssignableTo<IEnumerable<CuentaDto>>().Subject;
            returnedCuentas.Should().BeEmpty();
        }

        #endregion

        #region GetCuenta Tests

        [Fact]
        public async Task GetCuenta_ReturnsOkResult_WhenCuentaExists()
        {
            var cuenta = new CuentaDto 
            { 
                NumeroCuenta = 123456, 
                TipoCuenta = "Ahorro", 
                SaldoInicial = 1000,
                Estado = true,
                NombreCliente = "Juan Pérez"
            };
            _mockCuentaService.Setup(service => service.GetCuentaByNumeroCuentaAsync(123456))
                .ReturnsAsync(cuenta);

            var result = await _controller.GetCuenta(123456);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedCuenta = okResult.Value.Should().BeOfType<CuentaDto>().Subject;
            returnedCuenta.NumeroCuenta.Should().Be(123456);
            returnedCuenta.TipoCuenta.Should().Be("Ahorro");
        }

        [Fact]
        public async Task GetCuenta_ReturnsNotFound_WhenCuentaDoesNotExist()
        {
            _mockCuentaService.Setup(service => service.GetCuentaByNumeroCuentaAsync(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.GetCuenta(999999);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region CreateCuenta Tests

        [Fact]
        public async Task CreateCuenta_ReturnsCreatedAtAction_WhenSuccessful()
        {
            var cuentaDto = new CuentaDto 
            { 
                NumeroCuenta = 123456,
                TipoCuenta = "Ahorro",
                SaldoInicial = 1000,
                Estado = true,
                NombreCliente = "Juan Pérez"
            };
            var createdCuenta = new CuentaDto 
            { 
                NumeroCuenta = 123456,
                TipoCuenta = "Ahorro",
                SaldoInicial = 1000,
                Estado = true,
                NombreCliente = "Juan Pérez"
            };
            _mockCuentaService.Setup(service => service.CreateCuentaAsync(It.IsAny<CuentaDto>()))
                .ReturnsAsync(createdCuenta);

            var result = await _controller.CreateCuenta(cuentaDto);

            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(CuentasController.GetCuenta));
            createdResult.RouteValues["id"].Should().Be(123456);
            var returnedCuenta = createdResult.Value.Should().BeOfType<CuentaDto>().Subject;
            returnedCuenta.NumeroCuenta.Should().Be(123456);
        }

        [Fact]
        public async Task CreateCuenta_ReturnsBadRequest_WhenClienteNotFound()
        {
            var cuentaDto = new CuentaDto();
            _mockCuentaService.Setup(service => service.CreateCuentaAsync(It.IsAny<CuentaDto>()))
                .ThrowsAsync(new KeyNotFoundException("Cliente no encontrado"));

            var result = await _controller.CreateCuenta(cuentaDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Cliente no encontrado");
        }

        #endregion

        #region UpdateCuenta Tests

        [Fact]
        public async Task UpdateCuenta_ReturnsOkResult_WhenSuccessful()
        {
            var cuentaDto = new CuentaDto 
            { 
                NumeroCuenta = 123456,
                TipoCuenta = "Corriente",
                Estado = true 
            };
            var updatedCuenta = new CuentaDto 
            { 
                NumeroCuenta = 123456,
                TipoCuenta = "Corriente",
                Estado = true,
                NombreCliente = "Juan Pérez"
            };
            _mockCuentaService.Setup(service => service.UpdateCuentaAsync(123456, It.IsAny<CuentaDto>()))
                .ReturnsAsync(updatedCuenta);

            var result = await _controller.UpdateCuenta(123456, cuentaDto);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedCuenta = okResult.Value.Should().BeOfType<CuentaDto>().Subject;
            returnedCuenta.TipoCuenta.Should().Be("Corriente");
        }

        [Fact]
        public async Task UpdateCuenta_ReturnsNotFound_WhenCuentaDoesNotExist()
        {
            var cuentaDto = new CuentaDto();
            _mockCuentaService.Setup(service => service.UpdateCuentaAsync(It.IsAny<int>(), It.IsAny<CuentaDto>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.UpdateCuenta(999999, cuentaDto);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region PatchCuenta Tests

        [Fact]
        public async Task PatchCuenta_ReturnsOkResult_WhenSuccessful()
        {
            var patchDto = new CuentaPatchDto { Estado = false };
            var updatedCuenta = new CuentaDto 
            { 
                NumeroCuenta = 123456,
                Estado = false,
                TipoCuenta = "Ahorro",
                SaldoInicial = 1000,
                NombreCliente = "Juan Pérez"
            };
            _mockCuentaService.Setup(service => service.PatchCuentaAsync(123456, It.IsAny<CuentaPatchDto>()))
                .ReturnsAsync(updatedCuenta);

            var result = await _controller.PatchCuenta(123456, patchDto);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedCuenta = okResult.Value.Should().BeOfType<CuentaDto>().Subject;
            returnedCuenta.Estado.Should().BeFalse();
        }

        [Fact]
        public async Task PatchCuenta_ReturnsBadRequest_WhenInvalidOperationException()
        {
            var patchDto = new CuentaPatchDto();
            _mockCuentaService.Setup(service => service.PatchCuentaAsync(It.IsAny<int>(), It.IsAny<CuentaPatchDto>()))
                .ThrowsAsync(new InvalidOperationException("Operación inválida"));

            var result = await _controller.PatchCuenta(123456, patchDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Operación inválida");
        }

        #endregion

        #region DeleteCuenta Tests

        [Fact]
        public async Task DeleteCuenta_ReturnsOkWithMessage_WhenSuccessful()
        {
            _mockCuentaService.Setup(service => service.DeleteCuentaAsync(123456))
                .ReturnsAsync(true);

            var result = await _controller.DeleteCuenta(123456);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value;
            response.Should().BeEquivalentTo(new { mensaje = "Cuenta eliminada exitosamente" });
            _mockCuentaService.Verify(service => service.DeleteCuentaAsync(123456), Times.Once);
        }

        [Fact]
        public async Task DeleteCuenta_ReturnsNotFound_WhenCuentaDoesNotExist()
        {
            _mockCuentaService.Setup(service => service.DeleteCuentaAsync(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.DeleteCuenta(999999);

            result.Should().BeOfType<NotFoundResult>();
        }

        #endregion
    }
}