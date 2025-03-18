using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace TerraArgila.Tests.UI
{
    public class LoginUITest : IDisposable
    {
        private readonly IWebDriver _driver;

        public LoginUITest()
        {
            // options.AddArgument("--headless");

            var options = new ChromeOptions();

            _driver = new ChromeDriver(options);

            //Espera de 5 segundos para encontrar os elementos 
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        [Fact]
        public void Deve_FazerLogin_UsuarioVendor()
        {
            // azure: "https://terraeargila6-cabbhsb6hsfvg6at.canadacentral-01.azurewebsites.net/"

            var loginUrl = "https://localhost:7197/Identity/Account/Login";
            _driver.Navigate().GoToUrl(loginUrl);

            var emailField = _driver.FindElement(By.Id("Input_Email"));
            var passwordField = _driver.FindElement(By.Id("Input_Password"));
            var submitBtn = _driver.FindElement(By.Id("login-submit"));

            //Email pass
            emailField.SendKeys("vendor@test.com");
            passwordField.SendKeys("Vendor@1-.2/3.-6");

            //Botão
            submitBtn.Click();

            //Alterar depois para rota final 
            Assert.Contains("/", _driver.Url);
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }

}
