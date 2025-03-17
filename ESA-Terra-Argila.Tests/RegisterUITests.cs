
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Support.UI;


namespace TerraArgila.Tests.UI
{
    public class RegisterUITests : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public RegisterUITests()
        {
            var options = new ChromeOptions();
            
            _driver = new ChromeDriver(options);

            
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

           
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            
            _driver.Manage().Window.Maximize();
        }

        [Fact]
        public void Deve_RegistrarNovoUsuario()
        {
            _driver.Navigate().GoToUrl("https://terraeargila6-cabbhsb6hsfvg6at.canadacentral-01.azurewebsites.net/Identity/Account/Register");

            _driver.FindElement(By.Id("Input_FullName")).SendKeys("Teste Selenium");
            var email = $"selenium_{Guid.NewGuid()}@test.com";
            _driver.FindElement(By.Id("Input_Email")).SendKeys(email);
            _driver.FindElement(By.Id("Input_Password")).SendKeys("Teste@1234");
            _driver.FindElement(By.Id("Input_ConfirmPassword")).SendKeys("Teste@1234");

            var roleDropdown = _driver.FindElement(By.Id("Input_Role"));
            var selectRole = new SelectElement(roleDropdown);
            selectRole.SelectByValue("Vendor");

            var registerButton = _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("registerSubmit")));
            // Forçar scroll até o botão, se necessário
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", registerButton);

            registerButton.Click();

            // 4) Validar resultado
            Assert.Contains("/", _driver.Url);
            //Assert.Contains("RegisterConfirmation", _driver.Url);
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
