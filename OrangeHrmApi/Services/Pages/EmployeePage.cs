using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OrangeHrmApi.Models.DTOs;
using SeleniumExtras.WaitHelpers;


namespace OrangeHrmApi.Services.Pages
{
    public class EmployeePage
    {
        private readonly IWebDriver _driver;
        private readonly ILogger _logger;
        private readonly WebDriverWait _wait;

        private readonly By _pimMenu = By.XPath("//span[text()='PIM']");
        private readonly By _addEmployeeMenu = By.XPath("//a[text()='Add Employee']");
        private readonly By _firstNameField = By.Name("firstName");
        private readonly By _middleNameField = By.Name("middleName");
        private readonly By _lastNameField = By.Name("lastName");
        private readonly By _employeeIdField = By.XPath("//label[text()='Employee Id']/../..//input");
        private readonly By _saveButton = By.XPath("//button[@type='submit']");

        public EmployeePage(IWebDriver driver, ILogger logger)
        {
            _driver = driver;
            _logger = logger;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        }

        public async Task NavigateToAddEmployeeAsync()
        {
            _logger.LogInformation("Navigating to Add Employee page");

            _wait.Until(d => d.FindElement(_pimMenu)).Click();
            await Task.Delay(500);

            _wait.Until(d => d.FindElement(_addEmployeeMenu)).Click();
            await Task.Delay(1000);

            _logger.LogInformation("Successfully navigated to Add Employee page");
        }

        public async Task FillEmployeeDetailsAsync(string firstName, string? middleName, string lastName, string employeeId)
        {
            _logger.LogInformation("Filling employee details");

            var firstNameElement = _wait.Until(d => d.FindElement(_firstNameField));
            firstNameElement.Clear();
            firstNameElement.SendKeys(firstName);

            if (!string.IsNullOrEmpty(middleName))
            {
                var middleNameElement = _wait.Until(d => d.FindElement(_middleNameField));
                middleNameElement.Clear();
                middleNameElement.SendKeys(middleName);
            }

            var lastNameElement = _wait.Until(d => d.FindElement(_lastNameField));
            lastNameElement.Clear();
            lastNameElement.SendKeys(lastName);

            var employeeIdElement = _wait.Until(ExpectedConditions.ElementToBeClickable(_employeeIdField));
            _logger.LogInformation($"Initial Employee ID value: {employeeIdElement.GetAttribute("value")}");

            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                js.ExecuteScript($"arguments[0].value = '{employeeId}';", employeeIdElement);
                await Task.Delay(500);     

                string valueAfterSet = employeeIdElement.GetAttribute("value");
                if (valueAfterSet != employeeId)
                {
                    _logger.LogError($"Failed to set Employee ID to {employeeId}, current value: {valueAfterSet}");
                    throw new WebDriverException($"Employee ID mismatch: expected {employeeId}, got {valueAfterSet}");
                }
                _logger.LogInformation($"Set Employee ID to: {valueAfterSet}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Employee ID field");
                throw;
            }

            await Task.Delay(500);
            _logger.LogInformation("Employee details filled successfully");
        }

        public async Task SaveEmployeeAsync()
        {
            _logger.LogInformation("Saving employee");
            var saveButton = _wait.Until(ExpectedConditions.ElementToBeClickable(_saveButton));
            saveButton.Click();

            _wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".oxd-loading-spinner")));

            bool successToastAppeared = false;
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".oxd-toast.oxd-toast--success")));
                _logger.LogInformation("Success toast appeared");
                successToastAppeared = true;
            }
            catch (WebDriverTimeoutException)
            {
                _logger.LogWarning("No success toast, checking for wizard or profile");
            }

           
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h6[normalize-space()='Personal Details']")));
                _logger.LogInformation("Navigation to Personal Details page successful");
            }
            catch (WebDriverTimeoutException ex)
            {
                _logger.LogError(ex, "Failed to find Personal Details header, capturing page state");
                ((ITakesScreenshot)_driver).GetScreenshot().SaveAsFile("save_employee_error.png");
                _logger.LogInformation($"Page source: {_driver.PageSource}");
                throw;       
            }

            await Task.Delay(500);
        }


        public ValueTask<string> GetEmployeeIdAsync()
        {
            try
            {
                var url = _driver.Url;
                var empIdMatch = System.Text.RegularExpressions.Regex.Match(url, @"empNumber=(\d+)");
                if (empIdMatch.Success)
                {
                    return new ValueTask<string>(empIdMatch.Groups[1].Value);
                }

                var employeeIdElement = _wait.Until(d => d.FindElement(_employeeIdField));
                return new ValueTask<string>(employeeIdElement.GetAttribute("value") ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get EmployeeId");
                return new ValueTask<string>(string.Empty);
            }
        }

    }
}
