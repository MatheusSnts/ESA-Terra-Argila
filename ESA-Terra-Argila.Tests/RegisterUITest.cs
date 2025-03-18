
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
    public class RegisterUITest : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public RegisterUITest()
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
            _driver.Navigate().GoToUrl("https://localhost:7197/Identity/Account/Register");

            _driver.FindElement(By.Id("Input_FullName")).SendKeys("Teste Selenium");
            var email = $"selenium_{Guid.NewGuid()}@test.com";
            _driver.FindElement(By.Id("Input_Email")).SendKeys(email);
            _driver.FindElement(By.Id("Input_Password")).SendKeys("Teste@1-.2/3.-6");
            _driver.FindElement(By.Id("Input_ConfirmPassword")).SendKeys("Teste@1-.2/3.-6");

            var roleDropdown = _driver.FindElement(By.Id("Input_Role"));
            var selectRole = new SelectElement(roleDropdown);
            selectRole.SelectByValue("Vendor");

            var registerButton = _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("registerSubmit")));

            //Forçar scroll até o botão, caso seja preciso
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", registerButton);

            registerButton.Click();

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
