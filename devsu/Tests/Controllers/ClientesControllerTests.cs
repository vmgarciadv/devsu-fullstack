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
    public class ClientesControllerTests
    {
        private readonly Mock<IClienteService> _mockClienteService;
        private readonly ClientesController _controller;

        public ClientesControllerTests()
        {
            _mockClienteService = new Mock<IClienteService>();
            _controller = new ClientesController(_mockClienteService.Object);
        }

        #region GetClientes Tests

        [Fact]
        public async Task GetClientes_ReturnsOkResult_WithListOfClientes()
        {
            var clientes = new List<ClienteDto>
            {
                new() { Nombre = "Juan Pérez", Estado = true },
                new() { Nombre = "María García", Estado = true }
            };
            _mockClienteService.Setup(service => service.GetAllClientesAsync())
                .ReturnsAsync(clientes);

            var result = await _controller.GetClientes();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedClientes = okResult.Value.Should().BeAssignableTo<IEnumerable<ClienteDto>>().Subject;
            returnedClientes.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetClientes_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockClienteService.Setup(service => service.GetAllClientesAsync())
                .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.GetClientes();

            var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
            statusResult.Value.Should().BeOfType<string>().Which.Should().Contain("Error interno");
        }

        #endregion

        #region GetCliente Tests

        [Fact]
        public async Task GetCliente_ReturnsOkResult_WhenClienteExists()
        {
            var cliente = new ClienteDto { Nombre = "Juan Pérez", Estado = true };
            _mockClienteService.Setup(service => service.GetClienteByIdAsync(1))
                .ReturnsAsync(cliente);

            var result = await _controller.GetCliente(1);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedCliente = okResult.Value.Should().BeOfType<ClienteDto>().Subject;
            returnedCliente.Nombre.Should().Be("Juan Pérez");
        }

        [Fact]
        public async Task GetCliente_ReturnsNotFound_WhenClienteDoesNotExist()
        {
            _mockClienteService.Setup(service => service.GetClienteByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.GetCliente(999);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region CreateCliente Tests

        [Fact]
        public async Task CreateCliente_ReturnsCreated_WhenSuccessful()
        {
            var clienteDto = new ClienteDto 
            { 
                Nombre = "Nuevo Cliente",
                Genero = "M",
                Edad = 30,
                Identificacion = "1234567890",
                Direccion = "Calle 123",
                Telefono = "555-1234",
                Contrasena = "password123",
                Estado = true
            };
            var createdCliente = new ClienteDto { Nombre = "Nuevo Cliente", Estado = true };
            _mockClienteService.Setup(service => service.CreateClienteAsync(It.IsAny<ClienteDto>()))
                .ReturnsAsync(createdCliente);

            var result = await _controller.CreateCliente(clienteDto);

            var createdResult = result.Result.Should().BeOfType<CreatedResult>().Subject;
            var returnedCliente = createdResult.Value.Should().BeOfType<ClienteDto>().Subject;
            returnedCliente.Nombre.Should().Be("Nuevo Cliente");
        }

        [Fact]
        public async Task CreateCliente_ReturnsBadRequest_WhenInvalidOperationException()
        {
            var clienteDto = new ClienteDto();
            _mockClienteService.Setup(service => service.CreateClienteAsync(It.IsAny<ClienteDto>()))
                .ThrowsAsync(new InvalidOperationException("Cliente ya existe"));

            var result = await _controller.CreateCliente(clienteDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Cliente ya existe");
        }

        #endregion

        #region UpdateCliente Tests

        [Fact]
        public async Task UpdateCliente_ReturnsOkResult_WhenSuccessful()
        {
            var clienteDto = new ClienteDto { Nombre = "Cliente Actualizado", Estado = true };
            _mockClienteService.Setup(service => service.UpdateClienteAsync(1, It.IsAny<ClienteDto>()))
                .ReturnsAsync(clienteDto);

            var result = await _controller.UpdateCliente(1, clienteDto);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedCliente = okResult.Value.Should().BeOfType<ClienteDto>().Subject;
            returnedCliente.Nombre.Should().Be("Cliente Actualizado");
        }

        [Fact]
        public async Task UpdateCliente_ReturnsNotFound_WhenClienteDoesNotExist()
        {
            var clienteDto = new ClienteDto();
            _mockClienteService.Setup(service => service.UpdateClienteAsync(It.IsAny<int>(), It.IsAny<ClienteDto>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.UpdateCliente(999, clienteDto);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region PatchCliente Tests

        [Fact]
        public async Task PatchCliente_ReturnsOkResult_WhenSuccessful()
        {
            var patchDto = new ClientePatchDto { Estado = false };
            var updatedCliente = new ClienteDto { Nombre = "Cliente", Estado = false };
            _mockClienteService.Setup(service => service.PatchClienteAsync(1, It.IsAny<ClientePatchDto>()))
                .ReturnsAsync(updatedCliente);

            var result = await _controller.PatchCliente(1, patchDto);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedCliente = okResult.Value.Should().BeOfType<ClienteDto>().Subject;
            returnedCliente.Estado.Should().BeFalse();
        }

        [Fact]
        public async Task PatchCliente_ReturnsNotFound_WhenClienteDoesNotExist()
        {
            var patchDto = new ClientePatchDto();
            _mockClienteService.Setup(service => service.PatchClienteAsync(It.IsAny<int>(), It.IsAny<ClientePatchDto>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.PatchCliente(999, patchDto);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region DeleteCliente Tests

        [Fact]
        public async Task DeleteCliente_ReturnsOkWithMessage_WhenSuccessful()
        {
            _mockClienteService.Setup(service => service.DeleteClienteAsync(1))
                .ReturnsAsync(true);

            var result = await _controller.DeleteCliente(1);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value;
            response.Should().BeEquivalentTo(new { mensaje = "Cliente eliminado exitosamente" });
            _mockClienteService.Verify(service => service.DeleteClienteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteCliente_ReturnsNotFound_WhenClienteDoesNotExist()
        {
            _mockClienteService.Setup(service => service.DeleteClienteAsync(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.DeleteCliente(999);

            result.Should().BeOfType<NotFoundResult>();
        }

        #endregion
    }
}