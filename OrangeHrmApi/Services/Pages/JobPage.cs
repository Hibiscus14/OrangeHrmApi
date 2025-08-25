using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OrangeHrmApi.Models.DTOs;

namespace OrangeHrmApi.Services.Pages
{
    public class JobPage
    {
        private readonly IWebDriver _driver;
        private readonly ILogger _logger;
        private readonly WebDriverWait _wait;

        private readonly By _jobTab = By.XPath("//a[text()='Job']");
        private readonly By _jobTitleDropdown = By.XPath("//label[text()='Job Title']/../..//div[@class='oxd-select-text-input']");
        private readonly By _jobCategoryDropdown = By.XPath("//label[text()='Job Category']/../..//div[@class='oxd-select-text-input']");
        private readonly By _subUnitDropdown = By.XPath("//label[text()='Sub Unit']/../..//div[@class='oxd-select-text-input']");
        private readonly By _locationDropdown = By.XPath("//label[text()='Location']/../..//div[@class='oxd-select-text-input']");
        private readonly By _employmentStatusDropdown = By.XPath("//label[text()='Employment Status']/../..//div[@class='oxd-select-text-input']");
        private readonly By _joinedDateField = By.XPath("//label[text()='Joined Date']/../..//input");
        private readonly By _saveButton = By.XPath("//button[@type='submit']");
        private readonly By _dropdownOptions = By.CssSelector(".oxd-select-option");

        public JobPage(IWebDriver driver, ILogger logger)
        {
            _driver = driver;
            _logger = logger;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        }

        public async Task NavigateToJobTabAsync()
        {
            _logger.LogInformation("Navigating to Job tab");

            var jobTab = _wait.Until(d => d.FindElement(_jobTab));
            jobTab.Click();
            await Task.Delay(1000);

            _logger.LogInformation("Successfully navigated to Job tab");
        }

        public async Task<(bool success, string? error)> ValidateAndFillJobDetailsAsync(JobInfo jobInfo, bool caseSensitive)
        {
            _logger.LogInformation("Validating and filling job details");

            try
            {
                if (!await SelectDropdownOptionAsync(_jobTitleDropdown, jobInfo.JobTitle, "Job Title", caseSensitive))
                    return (false, $"Job title '{jobInfo.JobTitle}' not found");

                await Task.Delay(500);

                if (!await SelectDropdownOptionAsync(_jobCategoryDropdown, jobInfo.JobCategory, "Job Category", caseSensitive))
                    return (false, $"Job category '{jobInfo.JobCategory}' not found");

                await Task.Delay(500);

                if (!await SelectDropdownOptionAsync(_subUnitDropdown, jobInfo.SubUnit, "Sub Unit", caseSensitive))
                    return (false, $"Sub unit '{jobInfo.SubUnit}' not found");

                await Task.Delay(500);

                if (!await SelectDropdownOptionAsync(_locationDropdown, jobInfo.Location, "Location", caseSensitive))
                    return (false, $"Location '{jobInfo.Location}' not found");

                await Task.Delay(500);

                if (!await SelectDropdownOptionAsync(_employmentStatusDropdown, jobInfo.EmploymentStatus, "Employment Status", caseSensitive))
                    return (false, $"Employment status '{jobInfo.EmploymentStatus}' not found");

                var joinedDateElement = _driver.FindElement(_joinedDateField);
                joinedDateElement.Clear();
                joinedDateElement.SendKeys(DateTime.Now.ToString("yyyy-MM-dd"));

                await Task.Delay(500);
                _logger.LogInformation("Job details filled successfully");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filling job details");
                return (false, "Error filling job details");
            }
        }

        public async Task SaveJobDetailsAsync()
        {
            _logger.LogInformation("Saving job details");

            var saveButton = _driver.FindElement(_saveButton);
            saveButton.Click();

            await Task.Delay(2000);      
            _logger.LogInformation("Job details saved successfully");
        }

        private async Task<bool> SelectDropdownOptionAsync(By dropdownLocator, string optionText, string fieldName, bool caseSensitive)
        {
            try
            {
                _logger.LogInformation("Selecting option '{Option}' for field '{Field}'", optionText, fieldName);

                var dropdown = _wait.Until(d => d.FindElement(dropdownLocator));
                dropdown.Click();
                await Task.Delay(500);

                var options = _wait.Until(d => d.FindElements(_dropdownOptions));

                foreach (var option in options)
                {
                    var optionTextValue = option.Text.Trim();
                    bool matches = caseSensitive
                        ? optionTextValue.Equals(optionText, StringComparison.Ordinal)
                        : optionTextValue.Equals(optionText, StringComparison.OrdinalIgnoreCase);

                    if (matches)
                    {
                        option.Click();
                        await Task.Delay(300);
                        _logger.LogInformation("Successfully selected option '{Option}' for field '{Field}'", optionText, fieldName);
                        return true;
                    }
                }

                _logger.LogWarning("Option '{Option}' not found for field '{Field}'. Available options: {Options}",
                    optionText, fieldName, string.Join(", ", options.Select(o => o.Text)));

                _driver.FindElement(By.TagName("body")).Click();
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting dropdown option for field '{Field}'", fieldName);
                return false;
            }
        }
    }
}
