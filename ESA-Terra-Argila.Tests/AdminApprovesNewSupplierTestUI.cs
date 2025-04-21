using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Threading;

namespace ESA_Terra_Argila.Tests
{
    public class AdminApprovesNewSupplierTestUI : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        private const string BaseUrl = "https://localhost:7197";

        
        private const string AdminEmail = "admin@test.com";
        private const string AdminPassword = "Admin@1-.2/3.-6";

        

        public AdminApprovesNewSupplierTestUI()
        {
            var options = new ChromeOptions();
            
            //options.AddArgument("--headless");
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--start-maximized");

            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            _driver.Manage().Window.Maximize();
        }

        [Fact]
        [Trait("Category", "Selenium")]
        public void RegisterSupplier_AndApproveAsAdmin()
        {
            //RegistrarSupplier
            string newSupplierEmail = RegisterNewSupplier();

            //Logout
            Logout();

            //Login Admin
            LoginAsAdmin();

            //Acessar gestão de users
            _driver.Navigate().GoToUrl($"{BaseUrl}/Admin/AcceptUsers");

            //Localizar user e aprovar
            ApproveUser(newSupplierEmail);

           
        }

        private string RegisterNewSupplier()
        {
           
            string uniqueEmail = $"selenium_{Guid.NewGuid()}@test.com";

            
            _driver.Navigate().GoToUrl($"{BaseUrl}/Identity/Account/Register");

           
            _driver.FindElement(By.Id("Input_FullName")).SendKeys("Novo Fornecedor Selenium");
            _driver.FindElement(By.Id("Input_Email")).SendKeys(uniqueEmail);
            _driver.FindElement(By.Id("Input_Password")).SendKeys("Teste@1234");
            _driver.FindElement(By.Id("Input_ConfirmPassword")).SendKeys("Teste@1234");

           
            var roleSelect = new SelectElement(_driver.FindElement(By.Id("Input_Role")));
            roleSelect.SelectByValue("Supplier");

            
            _driver.FindElement(By.Id("registerSubmit")).Click();

           
            _wait.Until(ExpectedConditions.UrlContains("/"));

            return uniqueEmail;
        }

        private void Logout()
        {
            try
            {
                
                var logoutButton = _wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.XPath("//form[contains(@action, '/Account/Logout')]//button[contains(text(), 'Logout')]")
                ));
                logoutButton.Click();
            }
            catch (Exception ex)
            {
                
                _driver.Navigate().GoToUrl($"{BaseUrl}/Identity/Account/Logout");
            }

            
            _wait.Until(ExpectedConditions.UrlToBe($"{BaseUrl}/"));
        }

        private void LoginAsAdmin()
        {
            _driver.Navigate().GoToUrl($"{BaseUrl}/Identity/Account/Login");

            _driver.FindElement(By.Id("Input_Email")).SendKeys(AdminEmail);
            _driver.FindElement(By.Id("Input_Password")).SendKeys(AdminPassword);
            _driver.FindElement(By.Id("login-submit")).Click();

            
            _wait.Until(ExpectedConditions.UrlToBe($"{BaseUrl}/"));
        }

        private void ApproveUser(string email)
        {
            
            _wait.Until(ExpectedConditions.ElementExists(By.Id("datatable")));

            
            var row = _driver.FindElement(By.XPath(
                $"//table[@id='datatable']/tbody/tr[td[contains(text(), '{email}')]]"
            ));

            
            var approveButton = row.FindElement(By.CssSelector("button.text-success"));
            approveButton.Click();

            
            _wait.Until(driver => !driver.PageSource.Contains(email));
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
