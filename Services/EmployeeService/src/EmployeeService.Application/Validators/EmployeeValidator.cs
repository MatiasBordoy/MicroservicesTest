using EmployeeService.Domain.Entities;
using EmployeeService.Application.Exceptions;

namespace EmployeeService.Application.Validators
{
    public class EmployeeValidator
    {
        public void Validate(Employee employee)
        {
            var errors = new Dictionary<string, string[]>();

            if (employee.UserId == Guid.Empty)
                errors["FirstName"] = new[] { "First name is required." };

            if (string.IsNullOrWhiteSpace(employee.FirstName))
                errors["FirstName"] = new[] { "First name is required." };

            if (string.IsNullOrWhiteSpace(employee.LastName))
                errors["LastName"] = new[] { "Last name is required." };

            if (string.IsNullOrWhiteSpace(employee.Email))
                errors["Email"] = new[] { "Email is required." };

            if (employee.Salary < 0)
                errors["Salary"] = new[] { "Salary cannot be negative." };

            if (employee.HireDate < employee.BirthDate)
                errors["HireDate"] = new[] { "Hire date cannot be earlier than birth date." };

            if (errors.Any())
                throw new ValidationException(errors);
        }
    }
}
