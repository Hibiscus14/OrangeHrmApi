using Microsoft.EntityFrameworkCore;
using OrangeHrmApi.Models;

namespace OrangeHrmApi.Data
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EmployeeContext _context;
        private readonly ILogger<EmployeeRepository> _logger;

        public EmployeeRepository(EmployeeContext context, ILogger<EmployeeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Employee?> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<Employee?> GetByNameAsync(string firstName, string? middleName, string lastName)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.FirstName == firstName &&
                                        e.MiddleName == middleName &&
                                        e.LastName == lastName);
        }

        public async Task<Employee> AddAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Employee saved to database: {EmployeeId}", employee.EmployeeId);
            return employee;
        }

        public async Task<bool> ExistsAsync(string employeeId)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
        }
    }
}