using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeService.API.Controllers;
using EmployeeService.Application.Interfaces;
using EmployeeService.Contracts.DTOs;
using EmployeeService.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EmployeeService.UnitTests.Controllers
{
    public class EmployeesControllerTests
    {
        private readonly Mock<IEmployeeService> _serviceMock;
        private readonly EmployeesController _controller;

        public EmployeesControllerTests()
        {
            _serviceMock = new Mock<IEmployeeService>();
            _controller = new EmployeesController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithEmployeeList()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee { Id = Guid.NewGuid(), FirstName = "Juan", LastName = "Pérez" },
                new Employee { Id = Guid.NewGuid(), FirstName = "Ana", LastName = "García" }
            };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(employees);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult!.Value as IEnumerable<EmployeeResponse>;
            response.Should().NotBeNull();
            response!.Count().Should().Be(2);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenEmployeeExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var employee = new Employee { Id = id, FirstName = "Juan", LastName = "Pérez" };
            _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(employee);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult!.Value as EmployeeResponse;
            response.Should().NotBeNull();
            response!.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

            var result = await _controller.GetById(id);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Create_ShouldReturnCreated_WhenEmployeeIsValid()
        {
            var request = new CreateEmployeeRequest
            {
                FirstName = "Juan",
                LastName = "Perez",
                Email = "juan@empresa.com",
                HireDate = DateTime.UtcNow,
                BirthDate = new DateTime(1990, 1, 1),
                Role = Domain.Types.EmployeeRole.Staff,
                Salary = 1000
            };

            var created = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Role = request.Role,
                Salary = request.Salary
            };

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Employee>())).ReturnsAsync(created);

            var result = await _controller.Create(request);

            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            var response = createdResult!.Value as EmployeeResponse;
            response!.Id.Should().Be(created.Id);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

            var request = new UpdateEmployeeRequest { FirstName = "New" };
            var result = await _controller.Update(id, request);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenEmployeeDeleted()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

            var result = await _controller.Delete(id);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenEmployeeNotFound()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(false);

            var result = await _controller.Delete(id);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
