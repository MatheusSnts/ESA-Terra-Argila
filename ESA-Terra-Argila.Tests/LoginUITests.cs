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
    public class LoginUITests : IDisposable
    {
        private readonly IWebDriver _driver;

        public LoginUITests()
        {
            // 1) Configura o ChromeDriver
            var options = new ChromeOptions();

            // Caso queira rodar em modo headless (sem UI):
            // options.AddArgument("--headless");

            // Instancia o WebDriver com as opções
            _driver = new ChromeDriver(options);

            // Espera implícita de até 5s para encontrar elementos
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        [Fact]
        public void Deve_FazerLogin_UsuarioVendor()
        {
            // 2) Acessar a URL de login no seu Azure
            // Você mencionou: "https://terraeargila6-cabbhsb6hsfvg6at.canadacentral-01.azurewebsites.net/"
            // E o login está em /Identity/Account/Login (por padrão do ASP.NET Identity)
            var loginUrl = "https://terraeargila6-cabbhsb6hsfvg6at.canadacentral-01.azurewebsites.net/Identity/Account/Login";
            _driver.Navigate().GoToUrl(loginUrl);

            // 3) Localizar campos por ID ou outro seletor
            // Pelo seu "Login.cshtml", "asp-for='Input.Email'" gera id="Input_Email"
            // "asp-for='Input.Password'" gera id="Input_Password"
            // "asp-for='Input.RememberMe'" gera id="Input_RememberMe"
            // O botão está com id="login-submit"

            var emailField = _driver.FindElement(By.Id("Input_Email"));
            var passwordField = _driver.FindElement(By.Id("Input_Password"));
            var submitBtn = _driver.FindElement(By.Id("login-submit"));

            // 4) Preencher email e senha existentes
            emailField.SendKeys("vendor@test.com");
            passwordField.SendKeys("Vendor@123");

            // (Opcional) Clicar em "Manter-me ligado"
            // var rememberMe = _driver.FindElement(By.Id("Input_RememberMe"));
            // rememberMe.Click();

            // 5) Clicar em "Login"
            submitBtn.Click();

            // 6) Validar que o login funcionou
            // Exemplo: verificar se a URL redirecionou para a Home ("/") ou algo parecido
            // (Você mencionou que Redireciona para "~/" se o login for OK.)
            // Ou ainda, caso apareça "Utilizador autenticado." em algum lugar.

            // Aqui, como não sei exatamente a rota final, faço um exemplo de Contains:
            Assert.Contains("/", _driver.Url);

            // Se o site exibe alguma saudação, também podemos procurar:
            // var welcome = _driver.FindElement(By.Id("WelcomeUser"));
            // Assert.Equal("Bem vindo, vendor@test.com", welcome.Text);
        }

        public void Dispose()
        {
            // Fecha o navegador ao final de cada teste
            _driver.Quit();
            _driver.Dispose();
        }
    }

}
