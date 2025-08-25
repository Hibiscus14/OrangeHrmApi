using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OrangeHrmApi.Configuration;
using OrangeHrmApi.Data;
using OrangeHrmApi.Models;
using OrangeHrmApi.Models.DTOs;
using OrangeHrmApi.Services.Pages;
using System.Text;

namespace OrangeHrmApi.Services
{
    public class OrangeHrmService : IOrangeHrmService
    {
        private readonly OrangeHrmSettings _settings;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<OrangeHrmService> _logger;

        public OrangeHrmService(
            IOptions<OrangeHrmSettings> settings,
            IEmployeeRepository employeeRepository,
            ILogger<OrangeHrmService> logger)
        {
            _settings = settings.Value;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public async Task<(bool success, string? result, string? error)> AddEmployeeAsync(AddEmployeeRequest request)
        {
            IWebDriver? driver = null;
            try
            {
                _logger.LogInformation("Starting employee addition process for {FirstName} {LastName}",
                    request.FirstName, request.LastName);

                if (!_settings.AllowDuplicateEmployees && _settings.UseSqlite)
                {
                    var existingEmployee = await _employeeRepository.GetByNameAsync(
                        request.FirstName, request.MiddleName, request.LastName);

                    if (existingEmployee != null)
                    {
                        _logger.LogWarning("Duplicate employee found: {EmployeeId}", existingEmployee.EmployeeId);
                        return (false, null, "Employee with the same name already exists");
                    }
                }

                driver = CreateWebDriver();
                var loginPage = new LoginPage(driver, _logger);
                var addEmployeePage = new EmployeePage(driver, _logger);
                var jobPage = new JobPage(driver, _logger);

                await loginPage.LoginAsync(_settings.Username, _settings.Password);
                _logger.LogInformation("Successfully logged in");

                await addEmployeePage.NavigateToAddEmployeeAsync();

                string employeeId = GenerateEmployeeId();

                await addEmployeePage.FillEmployeeDetailsAsync(
                    request.FirstName, request.MiddleName, request.LastName, employeeId);

                await addEmployeePage.SaveEmployeeAsync();

                string finalEmployeeId = await addEmployeePage.GetEmployeeIdAsync();
                _logger.LogInformation("Employee saved with ID: {EmployeeId}", finalEmployeeId);

                await jobPage.NavigateToJobTabAsync();

                var validationResult = await jobPage.ValidateAndFillJobDetailsAsync(request.Job, _settings.CaseSensitiveValidation);
                if (!validationResult.success)
                {
                    return (false, null, validationResult.error);
                }

                await jobPage.SaveJobDetailsAsync();
                _logger.LogInformation("Job details saved successfully");

                if (_settings.UseSqlite)
                {
                    var employee = new Employee
                    {
                        EmployeeId = finalEmployeeId,
                        FirstName = request.FirstName,
                        MiddleName = request.MiddleName,
                        LastName = request.LastName
                    };
                    await _employeeRepository.AddAsync(employee);
                }

                return (true, finalEmployeeId, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding employee");
                return (false, null, "Internal server error");
            }
            finally
            {
                driver?.Quit();
                driver?.Dispose();
            }
        }

        public async Task<(bool success, string? result, string? error)> CreateClaimAsync(CreateClaimRequest request)
        {
            IWebDriver? driver = null;
            try
            {
                _logger.LogInformation("Starting claim creation process");

                string employeeName;

                if (_settings.UseSqlite && !string.IsNullOrEmpty(request.EmployeeId))
                {
                    var employee = await _employeeRepository.GetByEmployeeIdAsync(request.EmployeeId);
                    if (employee == null)
                    {
                        return (false, null, $"Employee with ID '{request.EmployeeId}' not found");
                    }
                    employeeName = employee.FullName;
                    _logger.LogInformation("Found employee by ID: {EmployeeName}", employeeName);
                }
                else if (!string.IsNullOrEmpty(request.EmployeeName))
                {
                    employeeName = request.EmployeeName;
                }
                else
                {
                    return (false, null, "Either employeeId or employeeName must be provided");
                }

                driver = CreateWebDriver();
                var loginPage = new LoginPage(driver, _logger);
                var claimPage = new AssignClaimPage(driver, _logger);

                await loginPage.LoginAsync(_settings.Username, _settings.Password);
                _logger.LogInformation("Successfully logged in");

                await claimPage.NavigateToAssignClaimAsync();

                var result = await claimPage.CreateClaimAsync(
                    employeeName, request.Event, request.Currency, request.Remarks, _settings.CaseSensitiveValidation);

                if (!result.success)
                {
                    return (false, null, result.error);
                }

                _logger.LogInformation("Claim created successfully with reference ID: {ReferenceId}", result.referenceId);
                return (true, result.referenceId, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating claim");
                return (false, null, "Internal server error");
            }
            finally
            {
                driver?.Quit();
                driver?.Dispose();
            }
        }

        private IWebDriver CreateWebDriver()
        {
            var options = new ChromeOptions();
            if (_settings.HeadlessMode)
            {
                options.AddArgument("--headless");
            }
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--enable-javascript");

            var driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

            return driver;
        }

        private static string GenerateEmployeeId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new StringBuilder(7);

            for (int i = 0; i < 7; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }
    }
}
