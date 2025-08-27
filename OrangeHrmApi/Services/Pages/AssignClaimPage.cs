using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace OrangeHrmApi.Services.Pages
{
    public class AssignClaimPage
    {
        private readonly IWebDriver _driver;
        private readonly ILogger _logger;
        private readonly WebDriverWait _wait;

        private readonly By _claimMenu = By.XPath("//span[text()='Claim']");
        private readonly By _assignClaimMenu = By.XPath("//a[text()='Assign Claim']");
        private readonly By _employeeNameField = By.XPath("//label[text()='Employee Name']/../..//input");
        private readonly By _employeeSuggestions = By.CssSelector(".oxd-autocomplete-option");
        private readonly By _eventDropdown = By.XPath("//label[text()='Event']/../..//div[@class='oxd-select-text-input']");
        private readonly By _currencyDropdown = By.XPath("//label[text()='Currency']/../..//div[@class='oxd-select-text-input']");
        private readonly By _remarksField = By.XPath("//label[text()='Remarks']/../..//textarea");
        private readonly By _createButton = By.CssSelector("button[type='submit']");
        private readonly By _dropdownOptions = By.CssSelector(".oxd-select-option");

        public AssignClaimPage(IWebDriver driver, ILogger logger)
        {
            _driver = driver;
            _logger = logger;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        }

        public async Task NavigateToAssignClaimAsync()
        {
            _logger.LogInformation("Navigating to Assign Claim page");

            var claimMenu = _wait.Until(d => d.FindElement(_claimMenu));
            claimMenu.Click();
            await Task.Delay(500);

            var assignClaimMenu = _wait.Until(d => d.FindElement(_assignClaimMenu));
            assignClaimMenu.Click();
            await Task.Delay(1000);

            _logger.LogInformation("Successfully navigated to Assign Claim page");
        }

        public async Task<(bool success, string? referenceId, string? error)> CreateClaimAsync(
    string employeeName, string eventName, string currency, string? remarks, bool caseSensitive)
        {
            try
            {
                _logger.LogInformation("Creating claim for employee: {EmployeeName}", employeeName);

                var employeeNameElement = _wait.Until(d => d.FindElement(_employeeNameField));
                employeeNameElement.Clear();
                employeeNameElement.SendKeys(employeeName);
                await Task.Delay(2000);

                var suggestions = _wait.Until(d => d.FindElements(_employeeSuggestions));
                if (suggestions.Count > 0)
                {
                    suggestions.First().Click();
                    await Task.Delay(500);
                }
                else
                {
                    return (false, null, $"Employee '{employeeName}' not found in suggestions");
                }

                if (!await SelectDropdownOptionAsync(_eventDropdown, eventName, "Event", caseSensitive))
                    return (false, null, $"Event '{eventName}' not found");

                await Task.Delay(500);

                if (!await SelectDropdownOptionAsync(_currencyDropdown, currency, "Currency", caseSensitive))
                    return (false, null, $"Currency '{currency}' not found");

                await Task.Delay(500);

                if (!string.IsNullOrEmpty(remarks))
                {
                    var remarksElement = _driver.FindElement(_remarksField);
                    remarksElement.Clear();
                    remarksElement.SendKeys(remarks);
                }

                var createButton = _wait.Until(ExpectedConditions.ElementToBeClickable(_createButton));
                createButton.Click();

                bool successToastAppeared = false;
                bool errorToastAppeared = false;
                try
                {
                    _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".oxd-toast.oxd-toast--success")));
                    successToastAppeared = true;
                    _logger.LogInformation("Success toast appeared");
                }
                catch (WebDriverTimeoutException)
                {
                    try
                    {
                        _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".oxd-toast.oxd-toast--error")));
                        errorToastAppeared = true;
                        _logger.LogWarning("Error toast appeared");
                        return (false, null, "Claim creation failed due to an error");
                    }
                    catch (WebDriverTimeoutException)
                    {
                        _logger.LogWarning("No success or error toast, checking for page update");
                    }
                }

                try
                {
                    _wait.Until(ExpectedConditions.ElementExists(By.CssSelector(".oxd-dialog")));
                    _logger.LogInformation("Modal detected, waiting for content");
                    await Task.Delay(2000);
                }
                catch (WebDriverTimeoutException)
                {
                    _logger.LogInformation($"No modal detected, current URL: {_driver.Url}");
                }

                try
                {
                    _wait.Until(ExpectedConditions.UrlMatches(@"https://opensource-demo\.orangehrmlive\.com/web/index\.php/claim/assignClaim/id/\d+"));
                    _logger.LogInformation($"Successfully navigated to claim details page: {_driver.Url}");
                }
                catch (WebDriverTimeoutException ex)
                {
                    _logger.LogError(ex, "Failed to navigate to claim details page, capturing page state");
                    ((ITakesScreenshot)_driver).GetScreenshot().SaveAsFile($"claim_navigation_error_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                    _logger.LogInformation($"Page source: {_driver.PageSource.Substring(0, Math.Min(2000, _driver.PageSource.Length))}...");
                    return (false, null, "Failed to navigate to claim details page");
                }

                await Task.Delay(500);

                var referenceId = await ExtractReferenceIdAsync();
                if (string.IsNullOrEmpty(referenceId))
                {
                    return (false, null, "Could not extract reference ID from confirmation page");
                }

                _logger.LogInformation("Claim created successfully with reference ID: {ReferenceId}", referenceId);
                return (true, referenceId, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating claim");
                return (false, null, "Error creating claim");
            }
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

                _logger.LogInformation(optionText, fieldName, string.Join(", ", options.Select(o => o.Text)));

                _driver.FindElement(By.TagName("body")).Click();
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting dropdown option for field '{Field}'", fieldName);
                return false;
            }
        }

        private async Task<string> ExtractReferenceIdAsync()
        {
            try
            {
                var referenceIdElement = _wait.Until(ExpectedConditions.ElementExists(
                    By.XPath("//label[text()='Reference Id']/ancestor::div[contains(@class, 'oxd-input-group')]//input[contains(@class, 'oxd-input') and @disabled]")));
                var referenceId = referenceIdElement.GetAttribute("value")?.Trim() ?? string.Empty;

                if (!string.IsNullOrEmpty(referenceId))
                {
                    _logger.LogInformation("Found reference ID: {ReferenceId}", referenceId);
                    return referenceId;
                }

                _logger.LogWarning("Reference ID is empty or not found in input");
                return string.Empty;
            }
            catch (NoSuchElementException ex)
            {
                _logger.LogError(ex, "Failed to find Reference Id element");
                ((ITakesScreenshot)_driver).GetScreenshot().SaveAsFile($"reference_id_error_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                _logger.LogInformation($"Page source snippet: {_driver.PageSource.Substring(0, Math.Min(2000, _driver.PageSource.Length))}...");
                return string.Empty;
            }
        }
    }
}
