using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace ESA_Terra_Argila.Tests
{
    public class SupplierCreatesMaterialUITest : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        private const string BaseUrl = "https://localhost:7197";

        //Supplier
        private const string SupplierEmail = "supplier@test.com";
        private const string SupplierPassword = "Supplier@1-.2/3.-6";

        public SupplierCreatesMaterialUITest()
        {
            var options = new ChromeOptions();

            //options.AddArgument("--headless");

            //Desativar password manager e avisos
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);

            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--start-maximized");

            _driver = new ChromeDriver(options);

            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            //Janela grande
            _driver.Manage().Window.Maximize();
        }

        [Fact(Skip = "Selenium Tests")]
        public void Everything()
        {
            // LOGIN
            LoginAsSupplier();

            // CRIAR CATEGORIA
            var categoryName = "Cat Selenium " + Guid.NewGuid().ToString("N").Substring(6);
            var categoryRef = "RefCat" + Guid.NewGuid().ToString("N").Substring(6);
            CreateCategory(categoryName, categoryRef);

            // CRIAR TAG
            var tagName = "Tag Selenium " + Guid.NewGuid().ToString("N").Substring(6);
            var tagRef = "RefTag" + Guid.NewGuid().ToString("N").Substring(6);
            CreateTag(tagName, tagRef);

            // CRIAR MATERIAL 
            var materialName = "Mat Selenium " + Guid.NewGuid().ToString("N").Substring(6);
            var materialRef = "RefMat" + Guid.NewGuid().ToString("N").Substring(6);
            CreateMaterial(materialName, materialRef, categoryName, tagName);
        }

       
        private void LoginAsSupplier()
        {
            //Login
            _driver.Navigate().GoToUrl($"{BaseUrl}/Identity/Account/Login");

            //Email e pass
            _driver.FindElement(By.Id("Input_Email")).SendKeys(SupplierEmail);
            _driver.FindElement(By.Id("Input_Password")).SendKeys(SupplierPassword);

            //Clica no botão
            _driver.FindElement(By.Id("login-submit")).Click();

            //Espera até chegar à homepage
            _wait.Until(ExpectedConditions.UrlToBe("https://localhost:7197/"));
        }

        
        private void CreateCategory(string categoryName, string categoryRef)
        {
            _driver.Navigate().GoToUrl($"{BaseUrl}/Categories/Create");


            _driver.FindElement(By.Id("Name")).SendKeys(categoryName);
            _driver.FindElement(By.Id("Reference")).SendKeys(categoryRef);

            
            _driver.FindElement(By.CssSelector("input[type='submit'].add-button")).Click();

            //Verifica se está nas categorias
            _wait.Until(ExpectedConditions.UrlContains("/Categories"));
        }

        
        private void CreateTag(string tagName, string tagRef)
        {
            _driver.Navigate().GoToUrl($"{BaseUrl}/Tags/Create");

            _driver.FindElement(By.Id("Name")).SendKeys(tagName);
            _driver.FindElement(By.Id("Reference")).SendKeys(tagRef);

            //True tag publica
            var selectElement = new SelectElement(_driver.FindElement(By.Id("IsPublic")));
            selectElement.SelectByValue("true");  

            _driver.FindElement(By.CssSelector("input[type='submit'].add-button")).Click();

            //Verifica se está nas categorias
            _wait.Until(ExpectedConditions.UrlContains("/Tags"));
        }

        private void CreateMaterial(string materialName, string materialRef, string categoryName, string tagName)
        {
            _driver.Navigate().GoToUrl($"{BaseUrl}/Materials/Create");

            
            _driver.FindElement(By.Id("Name")).SendKeys(materialName);
            _driver.FindElement(By.Id("Reference")).SendKeys(materialRef);
            _driver.FindElement(By.Id("Description")).SendKeys("Teste de material via Selenium");
            _driver.FindElement(By.Id("Price")).SendKeys("99.99");
            _driver.FindElement(By.Id("Unit")).SendKeys("kg");

            //Selecionar categoria com problema
            var categorySelect = new SelectElement(_driver.FindElement(By.Id("CategoryId")));
            categorySelect.SelectByText(categoryName);
            //Verificar se a userId é a igual.

            //Clicar no container "select2-selection--multiple"
            var select2Container = _driver.FindElement(By.CssSelector(".select2-selection--multiple"));
            select2Container.Click();

            //Esperar pelo campo de busca
            var searchField = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".select2-search__field")));

            searchField.SendKeys(tagName);
            Thread.Sleep(500); 
            searchField.SendKeys(Keys.Enter);

            _driver.FindElement(By.CssSelector("input[type='submit'].add-button")).Click();

            //igual a tag e cat
            _wait.Until(ExpectedConditions.UrlContains("/Materials"));
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
