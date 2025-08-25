using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace OrangeHrmApi.Services.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly ILogger _logger;
        private readonly WebDriverWait _wait;

        private readonly By _usernameField = By.Name("username");
        private readonly By _passwordField = By.Name("password");
        private readonly By _loginButton = By.XPath("//button[@type='submit']");
        private readonly By _dashboardHeader = By.CssSelector("h6");

        public LoginPage(IWebDriver driver, ILogger logger)
        {
            _driver = driver;
            _logger = logger;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        }

        public async Task LoginAsync(string username, string password)
        {
            _logger.LogInformation("Navigating to login page");
            _driver.Navigate().GoToUrl("https://opensource-demo.orangehrmlive.com");

            _logger.LogInformation("Filling login credentials");
            var usernameElement = _wait.Until(d => d.FindElement(_usernameField));
            usernameElement.Clear();
            usernameElement.SendKeys(username);

            var passwordElement = _driver.FindElement(_passwordField);
            passwordElement.Clear();
            passwordElement.SendKeys(password);

            _logger.LogInformation("Clicking login button");
            var loginButton = _driver.FindElement(_loginButton);
            loginButton.Click();

            _wait.Until(d => d.FindElement(_dashboardHeader));
            _logger.LogInformation("Login completed successfully");

            await Task.Delay(1000);     
        }
    }
}
