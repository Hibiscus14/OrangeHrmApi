using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrangeHrmApi.Configuration;
using OrangeHrmApi.Data;
using OrangeHrmApi.Models;
using OrangeHrmApi.Models.DTOs;
using OrangeHrmApi.Services;
namespace OrangeHrmApi.Tests
{
    public class OrangeHrmServiceTests
    {
        private readonly Mock<IEmployeeRepository> _mockRepository;
        private readonly Mock<ILogger<OrangeHrmService>> _mockLogger;
        private readonly OrangeHrmSettings _settings;
        private readonly OrangeHrmService _service;
        public OrangeHrmServiceTests()
        {
            _mockRepository = new Mock<IEmployeeRepository>();
            _mockLogger = new Mock<ILogger<OrangeHrmService>>();
            _settings = new OrangeHrmSettings
            {
                BaseUrl = "https://opensource-demo.orangehrmlive.com",
                Username = "Admin",
                Password = "admin123",
                TimeoutSeconds = 30,
                HeadlessMode = true,
                UseSqlite = true,
                AllowDuplicateEmployees = false,
                CaseSensitiveValidation = false
            };
            var mockOptions = new Mock<IOptions<OrangeHrmSettings>>();
            mockOptions.Setup(x => x.Value).Returns(_settings);
            _service = new OrangeHrmService(mockOptions.Object, _mockRepository.Object, _mockLogger.Object);
        }
        [Fact]
        public async Task AddEmployeeAsync_WithDuplicateName_ReturnsDuplicateError()
        {
            var request = new AddEmployeeRequest
            {
                FirstName = "Peter",
                LastName = "Griffin",
                Job = new JobInfo
                {
                    JobTitle = "Support Specialist",
                    JobCategory = "Professionals",
                    SubUnit = "Human Resources",
                    Location = "HQ - CA, USAD",
                    EmploymentStatus = "Full-Time Contract"
                }
            };
            var existingEmployee = new Employee
            {
                EmployeeId = "ABC123D",
                FirstName = "Peter",
                LastName = "Griffin"
            };
            _mockRepository.Setup(x => x.GetByNameAsync("Peter", null, "Griffin")).ReturnsAsync(existingEmployee);
        }
        [Fact]
        public async Task CreateClaimAsync_WithValidEmployeeId_ReturnsSuccess()
        {
            var request = new CreateClaimRequest
            {
                EmployeeId = "ABC123D",
                Event = "Medical Reimbursement",
                Currency = "United States Dollar",
                Remarks = "Peter falls and hurts his knee (Aaaah... Ssssss... Aaaah...) "
            }; var employee = new Employee
            {
                EmployeeId = "ABC123D",
                FirstName = "Peter",
                LastName = "Griffin"
            };
            _mockRepository.Setup(x => x.GetByEmployeeIdAsync("ABC123D")).ReturnsAsync(employee);
        }
    }
}