using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace RevueWebApp
{
    public class RevueFunctionalityTests
    {
        protected IWebDriver _driver;
        private readonly string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";
        private HelperMehods _helper;
        private readonly string Email = "test@testers.com";
        private readonly string Password = "123123";
        private Actions _actions;

        private string RevueTitle;
        private string RevueDescription;

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddArgument("--disable-search-engine-choice-screen");

            _driver = new ChromeDriver(options);
            _helper = new HelperMehods(_driver);
            _actions = new Actions(_driver);

            _driver.Manage().Window.Maximize();
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            _helper.UserLogin(Email, Password);
        }

        [TearDown]
        public void Teardown()
        {
            _driver.Quit();
            _driver.Dispose();
        }

        [Test, Order(1)]
        public void Create_Revue_WithInvalidData()
        {
            string emptyInput = string.Empty;

            _driver.FindElement(By.XPath("//a[text()='Create Revue']")).Click();

            var createRevueForm = _driver.FindElement(By.XPath("//div[@class='col-md-10 col-lg-6 col-xl-5 order-2 order-lg-1']"));

            _actions.ScrollToElement(createRevueForm).Perform();
            Assert.True(createRevueForm.Displayed);

            _driver.FindElement(By.XPath("//input[@id='form3Example1c']")).SendKeys(emptyInput);
            _driver.FindElement(By.XPath("//input[@id='form3Example3c']")).SendKeys(emptyInput);
            _driver.FindElement(By.XPath("//textarea[@id='form3Example4cd']")).SendKeys(emptyInput);
            _driver.FindElement(By.XPath("//button[@type='submit']")).Click();

            string createRevueErrorMessage = _driver.FindElement(By.XPath("//div[@class='text-danger validation-summary-errors']//li")).Text;
            string titleErrorMessage = _driver.FindElement(By.XPath("//span[@data-valmsg-for='Title']")).Text;
            string descriptionErrorMessage = _driver.FindElement(By.XPath("//span[@data-valmsg-for='Description']")).Text;

            Assert.That(_driver.Url, Is.EqualTo(BaseUrl + "/Revue/Create#createRevue"));

            Assert.Multiple(() =>
            {
                Assert.That(createRevueErrorMessage, Is.EqualTo("Unable to create new Revue!"));
                Assert.That(titleErrorMessage, Is.EqualTo("The Title field is required."));
                Assert.That(descriptionErrorMessage, Is.EqualTo("The Description field is required."));
            });

        }

        [Test, Order(2)]
        public void Create_RandomTestRevue_WithValidData()
        {
            RevueTitle = _helper.GenerateRandom("Title");
            RevueDescription = _helper.GenerateRandom("Description");

            _driver.FindElement(By.XPath("//a[text()='Create Revue']")).Click();

            var createRevueForm = _driver.FindElement(By.XPath("//div[@class='col-md-10 col-lg-6 col-xl-5 order-2 order-lg-1']"));

            _actions.ScrollToElement(createRevueForm).Perform();
            Assert.True(createRevueForm.Displayed);

            _driver.FindElement(By.XPath("//input[@id='form3Example1c']")).SendKeys(RevueTitle);
            _driver.FindElement(By.XPath("//input[@id='form3Example3c']")).SendKeys(string.Empty);
            _driver.FindElement(By.XPath("//textarea[@id='form3Example4cd']")).SendKeys(RevueDescription);
            _driver.FindElement(By.XPath("//button[@type='submit']")).Click();

            Assert.True(_driver.Url.Equals($"{BaseUrl}/Revue/MyRevues#createRevue"));

            var allCreatedRevues = _driver.FindElements(By.XPath("//div[@class='card mb-4 box-shadow']"));
            var lastCreatedRevue = allCreatedRevues.Last();

            _actions.ScrollToElement(lastCreatedRevue).Perform();

            var lastCreatedRevueTitle = lastCreatedRevue.FindElement(By.XPath(".//div[@class='text-muted text-center']"));

            Assert.That(lastCreatedRevueTitle.Text, Is.EqualTo(RevueTitle));
        }

        [Test, Order(3)]
        public void Search_RevueTitle()
        {
            _driver.FindElement(By.XPath("//a[text()='My Revues']")).Click();

            var searchField = _driver.FindElement(By.XPath("//input[@type='search']"));
            _actions.ScrollToElement(searchField).Perform();

            searchField.SendKeys(RevueTitle);
            _driver.FindElement(By.XPath("//button[@id='search-button']")).Click();

            var allCreatedRevues = _driver.FindElements(By.XPath("//div[@class='card mb-4 box-shadow']"));
            var lastCreatedRevue = allCreatedRevues.Last();

            _actions.ScrollToElement(lastCreatedRevue).Perform();

            var lastCreatedRevueTitle = lastCreatedRevue.FindElement(By.XPath(".//div[@class='text-muted text-center']"));

            Assert.That(lastCreatedRevueTitle.Text, Is.EqualTo(RevueTitle)); // or lastCreatedRevue.Text;
        }

        [Test, Order(4)]   
        public void Edit_LastCreatedRevue()
        {
            _driver.FindElement(By.XPath("//a[text()='My Revues']")).Click();
            var allCreatedRevues = _driver.FindElements(By.XPath("//div[@class='card mb-4 box-shadow']"));

            Assert.That(allCreatedRevues.Count, Is.AtLeast(1));

            var lastCreatedRevue = allCreatedRevues.Last();
            _actions.ScrollToElement(lastCreatedRevue).Perform();

            var editButton = lastCreatedRevue.FindElement(By.XPath(".//a[text()='Edit']"));
            editButton.Click();

            var editForm = _driver.FindElement(By.XPath("//div[@class='col-md-10 col-lg-6 col-xl-5 order-2 order-lg-1']"));
            _actions.ScrollToElement(editForm).Perform();

            var titleField = _driver.FindElement(By.XPath("//input[@id='form3Example1c']"));
            titleField.Clear();
            titleField.SendKeys($"Edited title: {RevueTitle}");
            _driver.FindElement(By.XPath("//button[@type='submit']")).Click(); //EditButton

            Assert.That(_driver.Url, Is.EqualTo($"{BaseUrl}/Revue/MyRevues"));

            allCreatedRevues = _driver.FindElements(By.XPath("//div[@class='card mb-4 box-shadow']"));
            lastCreatedRevue = allCreatedRevues.Last();
            _actions.ScrollToElement(lastCreatedRevue).Perform();

            Assert.That(lastCreatedRevue.Text, Does.Contain("Edited title:"));
        }

        [Test, Order(5)]
        public void Delete_LastCreatedRevue()
        {
            _driver.FindElement(By.XPath("//a[text()='My Revues']")).Click();
            var allCreatedRevues = _driver.FindElements(By.XPath("//div[@class='card mb-4 box-shadow']"));
            var revueCount = allCreatedRevues.Count();  

            var lastCreatedRevue = allCreatedRevues.Last();
            _actions.ScrollToElement(lastCreatedRevue).Perform();

            var deleteButton = lastCreatedRevue.FindElement(By.XPath(".//a[text()='Delete']"));
            deleteButton.Click();

            Assert.That(_driver.Url, Is.EqualTo($"{BaseUrl}/Revue/MyRevues"));

            allCreatedRevues = _driver.FindElements(By.XPath("//div[@class='card mb-4 box-shadow']"));
            lastCreatedRevue = allCreatedRevues.Last();

            Assert.That(allCreatedRevues.Count < revueCount);
            Assert.That(lastCreatedRevue.Text, Does.Not.Contain(RevueTitle));   
        }

        [Test, Order(6)]    
        public void Search_DeletedRevue()
        {
            _driver.FindElement(By.XPath("//a[text()='My Revues']")).Click();

            var searchField = _driver.FindElement(By.XPath("//input[@type='search']"));
            _actions.ScrollToElement(searchField).Perform();

            searchField.SendKeys(RevueTitle);
            _driver.FindElement(By.XPath("//button[@id='search-button']")).Click();

            //var allCreatedRevues = _driver.FindElements(By.XPath("//div[@class='card mb-4 box-shadow']"));
            //var isFound = allCreatedRevues.Any(x => x.Text.Contains(RevueTitle));

            var noResultsMessage = _driver.FindElement(By.XPath("//div[@class='row text-center']//span"));

            Assert.That(noResultsMessage.Text, Is.EqualTo("No Revues yet!"));

        }

    }
}