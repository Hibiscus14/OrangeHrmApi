using OrangeHrmApi.Models;

namespace OrangeHrmApi.Data
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByEmployeeIdAsync(string employeeId);
        Task<Employee?> GetByNameAsync(string firstName, string? middleName, string lastName);
        Task<Employee> AddAsync(Employee employee);
        Task<bool> ExistsAsync(string employeeId);
    }
}
