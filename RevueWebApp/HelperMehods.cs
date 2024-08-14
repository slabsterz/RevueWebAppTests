using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevueWebApp
{
    public class HelperMehods 
    {        
        private IWebDriver _driver;

        public HelperMehods(IWebDriver driver)
        {
            _driver = driver;
        }

        public void NavigateTo(string? path)
        {
            string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

            if (path == "base" || path == string.Empty)
            {
                _driver.Navigate().GoToUrl(baseUrl);
            }
            else
            {
                _driver.Navigate().GoToUrl(baseUrl + path);
            }
            
        }

        public void UserLogin(string email, string password)
        {
            string path = "/Users/Login";

            NavigateTo(path);

            var form = _driver.FindElement(By.XPath("//section[@id='loginForm']"));

            var actions = new Actions(_driver);
            actions.ScrollToElement(form).Perform();

            Assert.That(form.Displayed, Is.True, "Login form not displayed");

            _driver.FindElement(By.XPath("//input[@id='form3Example3']")).SendKeys(email);
            _driver.FindElement(By.XPath("//input[@id='form3Example4']")).SendKeys(password);
            _driver.FindElement(By.XPath("//button[@type='submit']")).Click();

        }

        public string GenerateRandom(string type)
        {
            var random = new Random();
            string text = $"Random{type}-{random.Next(1,9999)}";
            return text;
        }

    }
}
