using EmployeeService.API;
using EmployeeService.Contracts.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EmployeeService.IntegrationTests
{
    public class EmployeeApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public EmployeeApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/employees");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Create_ShouldReturnCreated()
        {
            var newEmployee = new CreateEmployeeRequest
            {
                FirstName = "Juan",
                LastName = "Perez",
                Email = "juan@empresa.com",
                BirthDate = new DateTime(1990, 1, 1),
                HireDate = DateTime.UtcNow,
                Role = Domain.Types.EmployeeRole.Staff,
                Salary = 5000
            };

            var response = await _client.PostAsJsonAsync("/api/employees", newEmployee);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var employee = await response.Content.ReadFromJsonAsync<EmployeeResponse>();
            employee.Should().NotBeNull();
            employee!.FirstName.Should().Be("Juan");
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
        {
            var fakeId = Guid.NewGuid();
            var response = await _client.GetAsync($"/api/employees/{fakeId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
