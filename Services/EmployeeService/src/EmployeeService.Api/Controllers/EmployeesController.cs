using EmployeeService.Application.Interfaces;
using EmployeeService.Contracts.DTOs;
using EmployeeService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<EmployeeResponse>>> GetAll()
        {
            try
            {
                var employees = await _employeeService.GetAllAsync();

                var response = employees.Select(e => new EmployeeResponse
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    BirthDate = e.BirthDate,
                    HireDate = e.HireDate,
                    Role = e.Role,
                    Salary = e.Salary
                }).ToList();

                return Ok(response);
            }
            catch
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<EmployeeResponse>> GetById(Guid id)
        {
            try
            {
                var employee = await _employeeService.GetByIdAsync(id);
                if (employee == null)
                    return NotFound();

                var response = new EmployeeResponse
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    BirthDate = employee.BirthDate,
                    HireDate = employee.HireDate,
                    Role = employee.Role,
                    Salary = employee.Salary
                };

                return Ok(response);
            }
            catch
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EmployeeResponse>> Create([FromBody] CreateEmployeeRequest request)
        {
            try
            {
                var newEmployee = new Employee
                {
                    Id = Guid.NewGuid(),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    BirthDate = request.BirthDate,
                    HireDate = request.HireDate,
                    Role = request.Role,
                    Salary = request.Salary
                };

                var created = await _employeeService.CreateAsync(newEmployee);

                var response = new EmployeeResponse
                {
                    Id = created.Id,
                    FirstName = created.FirstName,
                    LastName = created.LastName,
                    Email = created.Email,
                    BirthDate = created.BirthDate,
                    HireDate = created.HireDate,
                    Role = created.Role,
                    Salary = created.Salary
                };

                return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
            }
            catch
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EmployeeResponse>> Update(Guid id, [FromBody] UpdateEmployeeRequest request)
        {
            try
            {
                var existing = await _employeeService.GetByIdAsync(id);
                if (existing == null)
                    return NotFound();

                existing.FirstName = request.FirstName;
                existing.LastName = request.LastName;
                existing.Email = request.Email;
                existing.BirthDate = request.BirthDate;
                existing.HireDate = request.HireDate;
                existing.Role = request.Role;
                existing.Salary = request.Salary;

                var updated = await _employeeService.UpdateAsync(existing);

                var response = new EmployeeResponse
                {
                    Id = updated.Id,
                    FirstName = updated.FirstName,
                    LastName = updated.LastName,
                    Email = updated.Email,
                    BirthDate = updated.BirthDate,
                    HireDate = updated.HireDate,
                    Role = updated.Role,
                    Salary = updated.Salary
                };

                return Ok(response);
            }
            catch
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _employeeService.DeleteAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
