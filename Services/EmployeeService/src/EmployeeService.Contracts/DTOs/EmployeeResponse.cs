using EmployeeService.Domain.Types;

namespace EmployeeService.Contracts.DTOs
{
    public class EmployeeResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public EmployeeRole Role { get; set; }
        public decimal Salary { get; set; }
    }
}
