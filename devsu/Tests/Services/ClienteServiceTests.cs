using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using AutoMapper;
using devsu.Models;
using devsu.Services;
using devsu.Repositories;
using devsu.DTOs;

namespace Tests.Services
{
    public class ClienteServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IClienteRepository> _mockClienteRepository;
        private readonly Mock<ICuentaRepository> _mockCuentaRepository;
        private readonly ClienteService _service;

        public ClienteServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockClienteRepository = new Mock<IClienteRepository>();
            _mockCuentaRepository = new Mock<ICuentaRepository>();
            
            _mockUnitOfWork.Setup(uow => uow.Clientes).Returns(_mockClienteRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.Cuentas).Returns(_mockCuentaRepository.Object);
            
            _service = new ClienteService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task DeleteClienteAsync_ShouldSoftDeleteClienteAndAllAssociatedCuentas()
        {
            // Arrange
            var clienteId = 1;
            var cliente = new Cliente 
            { 
                Id = clienteId, 
                Nombre = "Test Cliente", 
                Estado = true 
            };
            
            var cuentas = new List<Cuenta>
            {
                new Cuenta { CuentaId = 1, ClienteId = clienteId, Estado = true, NumeroCuenta = 123456 },
                new Cuenta { CuentaId = 2, ClienteId = clienteId, Estado = true, NumeroCuenta = 789012 }
            };

            _mockClienteRepository.Setup(repo => repo.GetByIdAsync(clienteId))
                .ReturnsAsync(cliente);
            
            _mockCuentaRepository.Setup(repo => repo.GetCuentasByClienteAsync(clienteId))
                .ReturnsAsync(cuentas);
            
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.DeleteClienteAsync(clienteId);

            // Assert
            result.Should().BeTrue();
            cliente.Estado.Should().BeFalse();
            cuentas.Should().AllSatisfy(cuenta => cuenta.Estado.Should().BeFalse());
            
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteClienteAsync_ShouldThrowKeyNotFoundException_WhenClienteDoesNotExist()
        {
            // Arrange
            var clienteId = 999;
            _mockClienteRepository.Setup(repo => repo.GetByIdAsync(clienteId))
                .ReturnsAsync((Cliente)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteClienteAsync(clienteId));
            
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteClienteAsync_ShouldHandleClienteWithNoCuentas()
        {
            // Arrange
            var clienteId = 1;
            var cliente = new Cliente 
            { 
                Id = clienteId, 
                Nombre = "Test Cliente", 
                Estado = true 
            };
            
            var cuentas = new List<Cuenta>(); // No cuentas

            _mockClienteRepository.Setup(repo => repo.GetByIdAsync(clienteId))
                .ReturnsAsync(cliente);
            
            _mockCuentaRepository.Setup(repo => repo.GetCuentasByClienteAsync(clienteId))
                .ReturnsAsync(cuentas);
            
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.DeleteClienteAsync(clienteId);

            // Assert
            result.Should().BeTrue();
            cliente.Estado.Should().BeFalse();
            
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }
    }
}