using EmployeeService.Domain.Types;

namespace EmployeeService.Domain.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public EmployeeRole Role { get; set; }
        public Decimal Salary { get; set; }
    }
}
