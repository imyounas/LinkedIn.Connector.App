using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using AppLogger;
using LICommon;
using LICommon.Models;
using LIConnectScraperLib.Selectors;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LIConnectScraperLib
{
    public class LIScraper
    {

        public IWebDriver _driver;

        ScraperUtil _util = null;
        WebDriverWait _wait = null;
        ScraperSettings _settings = null;

        Random r = null;
        int ri = 1;
        string linkedInUrl = "https://www.linkedin.com";
        string seleniumNullUrl = "data:,";

        bool scraperShutdown = false;
        bool manualInput = false;
        bool isLoggedInToLinkedIn = false;

        public LIScraper(ScraperSettings sett)
        {

           
            _settings = sett;
            r = new Random();
            _util = new ScraperUtil(sett.UseOffScreenScraper);

            Logger.Debug("Initializing Web browser driver...");

            _driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _util.GetChromeProfile());

            Logger.Debug("Setting web driver configurations...");

            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(_settings.ImplicitTimeout);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(_settings.ScraperTimeout));

        }

        public void ShutdownScraper()
        {
            if (!scraperShutdown)
            {
                Logger.Debug($"Shutting down web driver");
                _driver.Quit();
                scraperShutdown = true;
            }
        }


        public bool StartConnectRequestFlow(List<LIUserConnectRequestReport> userConnects)
        {
            bool done = false;
            try
            {
                LIUserConnectRequestReport up = null;

                Logger.Debug($"{userConnects.Count} UserProfiles to process");

                if (!isLoggedInToLinkedIn)
                {
                    Logger.Debug($"Starting LinkedIn Login Workflow");
                    LoginToLinkedIn();
                }

                for (int i = 0; i < userConnects.Count; i++)
                {
                    try
                    {

                        string fm = string.Empty;

                        if (i > 0)
                        {
                            ri = r.Next(_settings.ConnMinWait, _settings.ConnMaxWait);
                            int w = ri * 1000;
                            Logger.Debug($"Sleeping for {ri} Seconds before next Connection Request");
                            Thread.Sleep(w);
                        }

                        up = userConnects[i];

                        Logger.Debug($"Processing UserProfile No. {(i + 1)}  - {up.ProfileTitle} - {up.ProfileUrl}");

                        Logger.Debug($"Navigating to UserProfile {up.ProfileTitle}");
                        NavigateToUrl(up.ProfileUrl);
                        LongRandomWait();
                        DoAjaxWait();


                        Logger.Debug($"Checking existence of Connect Button");
                        var btnConnect = _driver.FindElements(By.CssSelector(LIConnectionRequestSelectors.ProfileConnButtonCSSSel));

                        if (btnConnect == null || btnConnect.Count <= 0)
                        {
                            Logger.Debug($"No Connect Button found for Profile {up.ProfileTitle}, URL=> {up.ProfileUrl}");
                            up.Comments = "No Connect Button found for Profile" + Environment.NewLine;
                            continue; ;
                        }

                        Logger.Debug($"Clicking Connect Button");
                        btnConnect[0].Click();
                        ShortRandomWait();

                        Logger.Debug("Waiting for Connection ActionBar to appear");
                        _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(LIConnectionRequestSelectors.SendConnectDivVisibleCSSSel)));

                        var btnAddNote = _driver.FindElements(By.CssSelector(LIConnectionRequestSelectors.ConnAddNoteButtonCSSSel));
                        bool addNote = true;
                        if (btnAddNote == null || btnAddNote.Count <= 0)
                        {
                            Logger.Debug($"No Add Note Button for Profile {up.ProfileTitle}, URL=> {up.ProfileUrl}");
                            up.Comments = "No Add Note Button for Profile" + Environment.NewLine; ;

                            Logger.Debug($"Trying Sending Connect Request without Note");
                            addNote = false;
                        }


                        if (addNote)
                        {

                            if (!string.IsNullOrWhiteSpace(_settings.ConnGreetings) || !string.IsNullOrWhiteSpace(_settings.ConnMessage))
                            {
                                var tp = _util.GetGreetingAndMessage(up, _settings, _settings.ConnGreetings, _settings.ConnMessage);

                                if (!string.IsNullOrWhiteSpace(tp.Item1))
                                {
                                    fm = tp.Item1 + System.Environment.NewLine;
                                }

                                if (!string.IsNullOrWhiteSpace(tp.Item2))
                                {
                                    fm += tp.Item2;
                                }

                                Logger.Debug($"Adding Connection Message {fm}");


                                Logger.Debug($"Clicking Add Note Button");
                                btnAddNote[0].Click();
                                ShortRandomWait();

                                _driver.FindElement(By.CssSelector(LIConnectionRequestSelectors.ConnNoteTextCSSSel)).SendKeys(fm);
                            }
                            else
                            {
                                Logger.Debug($"No Greetings or Message found to send");
                                up.Comments = "No Greetings or Message found to send" + Environment.NewLine; ;
                            }
                        }


                        Logger.Debug($"Sending Connection Request");
                        _driver.FindElement(By.CssSelector(LIConnectionRequestSelectors.ConnSendDoneButtonCSSSel)).Click();
                        ShortRandomWait();
                        DoAjaxWait();

                        up.ConnectMessage = fm;
                        up.Timestamp = DateTime.Now;


                    }
                    catch (Exception ex)
                    {
                        up.Timestamp = DateTime.Now;
                        up.Errors = ex.Message;

                        Logger.Error(ex, $"Inside-Loop-{i+1}: Error while making Connection Request to User {up.ProfileTitle} - {up.ProfileUrl}");
                    }

                }

                done = true;
                Logger.Debug("All Connection Message Requests have been processed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while Connection Request Flow");
            }

            return done;
        }

        public bool StartDirectMessageFlow(List<LIUserDirectMessageReport> userDirMessage)
        {
            bool done = false;
            try
            {
                LIUserDirectMessageReport up = null;

                Logger.Debug($"{userDirMessage.Count} UserProfiles to process");

            
                if (!isLoggedInToLinkedIn)
                {
                    Logger.Debug($"Starting LinkedIn Login Workflow");
                    LoginToLinkedIn();
                }


                for (int i = 0; i < userDirMessage.Count; i++)
                {
                    try
                    {
                        string fm = string.Empty;

                        if (i > 0)
                        {
                            ri = r.Next(_settings.ConnMinWait, _settings.ConnMaxWait);
                            int w = ri * 1000;
                            Logger.Debug($"Sleeping for {ri} Seconds before next Connection Request");
                            Thread.Sleep(w);
                        }

                        up = userDirMessage[i];
                        Logger.Debug($"Processing UserProfile No. {(i + 1)}  - {up.ProfileTitle} - {up.ProfileUrl}");


                        Logger.Debug($"Navigating to UserProfile {up.ProfileTitle}");
                        NavigateToUrl(up.ProfileUrl);
                        LongRandomWait();
                        DoAjaxWait();


                        Logger.Debug($"Checking existence of Message Button");
                        var btnMessage = _driver.FindElements(By.CssSelector(LIDirectMessageSelectors.ProfileDirectMessageButtonCSSSel));

                        if (btnMessage == null || btnMessage.Count <= 0)
                        {
                            Logger.Debug($"No Message Button found for Profile {up.ProfileTitle}, URL=> {up.ProfileUrl}");
                            up.Comments = $"No Message Button found for profile" + Environment.NewLine;
                            continue; ;
                        }

                        Logger.Debug($"Clicking Message Button");
                        btnMessage[0].Click();
                        ShortRandomWait();
                        DoAjaxWait();

                        Logger.Debug("Waiting for Message Form to appear");
                        _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(LIDirectMessageSelectors.DirectMessageFormVisibleCSSSel)));


                        if (!string.IsNullOrWhiteSpace(_settings.DirMsgGreetings) || !string.IsNullOrWhiteSpace(_settings.DirMessage))
                        {
                            var tp = _util.GetGreetingAndMessage(up, _settings, _settings.DirMsgGreetings, _settings.DirMessage);

                            if (!string.IsNullOrWhiteSpace(tp.Item1))
                            {
                                fm = tp.Item1 + System.Environment.NewLine;
                            }

                            if (!string.IsNullOrWhiteSpace(tp.Item2))
                            {
                                fm += tp.Item2;
                            }

                            Logger.Debug($"Adding Direct Message {fm}");
                            _driver.FindElement(By.CssSelector(LIDirectMessageSelectors.DirectMessageTextCSSSel)).SendKeys(fm);
                        }
                        else
                        {
                            Logger.Debug($"No Greetings or Message found to send");
                            up.Comments = $"No Greetings or Message found to send" + Environment.NewLine;
                        }


                        Logger.Debug($"Sending Message Request");
                        _driver.FindElement(By.CssSelector(LIDirectMessageSelectors.DirectMessageSendButtonCSSSel)).Click();
                        ShortRandomWait();
                        DoAjaxWait();

                        up.DirectMessage = fm;
                        up.Timestamp = DateTime.Now;


                    }
                    catch (Exception ex)
                    {
                        up.Timestamp = DateTime.Now;
                        up.Errors = ex.Message;
                        Logger.Error(ex, $"Inside-Loop-{i+1}: Error while making Direct Message to User {up.ProfileTitle} - {up.ProfileUrl}");
                    }
                }
                done = true;
                Logger.Debug("All Direct Message Requests have been processed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while Direct Message Flow");
            }

            return done;
        }

        public List<LIUserData> StartUserProfilesScraping(SearchCriteria sc, int pageNo, out bool areThereMoreResults)
        {
            List<LIUserData> profiles = new List<LIUserData>();
            areThereMoreResults = false;
            try
            {

                if (!isLoggedInToLinkedIn)
                {
                    Logger.Debug($"Starting LinkedIn Login Workflow");
                    LoginToLinkedIn();
                }

                int liPageNo = 1;
                string url = LISearchURLGenerator.URLGenerator(sc, pageNo, _settings.ScraperBatchSize, out liPageNo);
                Logger.Debug($"Auto Generated LinkedIn URL {url}");

                NavigateToUrl(url);

                var empData = ScrapeEmployeesResultPage(_settings.ScraperBatchSize, liPageNo);
                profiles = empData.employeesData;
                areThereMoreResults = empData.currentPageNo < empData.lastPageNo;

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while scraping LinkedIn Search Result Page");
            }

            return profiles;
        }



        private (List<LIUserData> employeesData, int currentPageNo, int lastPageNo) ScrapeEmployeesResultPage(int totalEmployeesToScrape, int currentPage = 1)
        {

            List<LIUserData> employees = new List<LIUserData>();
            Logger.Debug($"Scrolling down page to load dynamic profiles...");
            SearchResultPageScrollDown();

            int loopEnd = 1;
            Logger.Debug($"Request came for {totalEmployeesToScrape} User Profiles Scraping...");

            Logger.Debug($"Scraping  Page {currentPage}...");
            //for first page - where currently it is

            string html = this._driver.PageSource;
            var parser = new HtmlParser();
            var document = parser.Parse(html);

            ScrapeCurrentEmployeesResultPage(employees, document);

            string currentUrl = _driver.Url.ToString();
            currentUrl = currentUrl.Replace("&page=" + currentPage, "");

            loopEnd = (int)Math.Ceiling(totalEmployeesToScrape / (10 * 1.0));
            loopEnd += currentPage;

            int lastPage = 1;

            bool done = GetEmployeeResultLastPageNumber(document, out lastPage);

            int i = currentPage + 1;
            for (; i < loopEnd; i++)
            {
                Logger.Debug($"Checking of there are any next pages...");
                done = GetEmployeeResultLastPageNumber(document, out lastPage);
                if (!done)
                {
                    break;
                }

                string newUrl = currentUrl + string.Format("&page={0}", i);

                Logger.Debug($"Navigating to next page {newUrl}");


                NavigateToUrl(newUrl);

                ri = r.Next(_settings.ScraperMinWait, _settings.ScraperMaxWait);
                Thread.Sleep(ri * 100);

                html = this._driver.PageSource;
                document = parser.Parse(html);

                Logger.Debug($"Checking existence of Employees Search result on page no. {i }");
                var found = document.QuerySelectorAll(LICompanyEmployeeSelectors.SearchResultCSSSel);

                if (found == null || found.Length <= 0)
                {
                    Logger.Debug($"No Employee Search Result found on page {i + 1} , URL=> {newUrl}");

                    break;
                }

                Logger.Debug($"Scrolling down page to load dynamic profiles...");
                SearchResultPageScrollDown();

                try
                {
                    html = this._driver.PageSource;
                    document = parser.Parse(html);

                    ScrapeCurrentEmployeesResultPage(employees, document);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Error while scraping user profile search result page, URL=> {newUrl}");
                }
            }

            return (employees, i - 1, lastPage);
        }


        private bool GetEmployeeResultLastPageNumber(IHtmlDocument document, out int lastPage)
        {
            bool done = false;
            lastPage = 1;
            var pages = document.QuerySelectorAll(LICompanyEmployeeSelectors.PagerCollectionCSSSel);

            if (pages != null && pages.Length > 0)
            {
                Logger.Debug($"No pager found on this LinkedIn page");
                string number = pages[pages.Length - 1].TextContent.Trim();
                lastPage = _util.GetNumberFromString(number);
            }

            var next = document.QuerySelectorAll(LICompanyEmployeeSelectors.NextPageSelector);
            done = next != null && next.Length > 0 ? true : false;

            return done;
        }

        private void ScrapeCurrentEmployeesResultPage(List<LIUserData> employees, IHtmlDocument document)
        {


            int empInNetwork = 0;
            Logger.Debug($"Scraping Employees Data...");

            Logger.Debug($"Querying page for required information");

            var titles = document.QuerySelectorAll(LICompanyEmployeeSelectors.EmployeeNameCSSSelIdx);
            var profileUrls = document.QuerySelectorAll(LICompanyEmployeeSelectors.EmployeeProfileUrlCSSSelIdx);
            var locations = document.QuerySelectorAll(LICompanyEmployeeSelectors.EmployeeLocationCSSSelIdx);
            var designations = document.QuerySelectorAll(LICompanyEmployeeSelectors.EmployeeDesignationCSSSelIdx);
            //var designations2 = document.QuerySelectorAll(LICompanyEmployeeSelectors.EmployeeSRDesignationCSSSelIdx);
            var imageUrls = document.QuerySelectorAll(LICompanyEmployeeSelectors.EmployeeImageUrlCSSSelIdx);
            var parentElems = document.QuerySelectorAll(LICompanyEmployeeSelectors.ParentElementCSSSel);


            Logger.Debug($"{parentElems.Length} Profiles found");

            LIUserData emp = null;
            int currentPageEmployees = titles.Length;
            for (int i = 0; i < currentPageEmployees; i++)
            {
                try
                {

                    string title = GetNodeTextValue(titles[i]);

                    Logger.Debug($"Processing Profile {i + 1} , {title}");

                    if (title == "")
                    {
                        Logger.Debug("Blank titile found " + GetNodeTextValue(profileUrls[i], "href"));
                    }

                    if (!string.IsNullOrWhiteSpace(title) && !_util.ContainsInsensitive(title, _settings.IgnoreEmployeeTitle))
                    {
                        empInNetwork++;
                        emp = new LIUserData();

                        emp.ScraperProfileUrl = GetNodeTextValue(profileUrls[i], "href");
                        emp.ScraperProfileUrl = linkedInUrl + emp.ScraperProfileUrl;

                        emp.ProfileUrl = emp.ScraperProfileUrl;

                        emp.ProfileTitle = title;
                        emp.Location = GetNodeTextValue(locations[i]);
                        emp.CurrentJobTitle = GetNodeTextValue(designations[i]);

                        var pe = parentElems[i];
                        emp.ConnectionDegree = GetNodeTextValue(pe.QuerySelector(LICompanyEmployeeSelectors.EmployeeConnectionDegreeCSSSelIdx));

                        if (string.IsNullOrWhiteSpace(emp.ConnectionDegree))
                        {
                            emp.ConnectionDegree = "-";
                        }

                        emp.CurrentWorkingTitle = GetNodeTextValue(pe.QuerySelector(LICompanyEmployeeSelectors.EmployeeSRDesignationCSSSelIdx));

                        emp.Image = GetNodeTextValue(imageUrls[i], "src");
                        if (string.IsNullOrWhiteSpace(emp.Image) || emp.Image.Contains("data:image"))
                        {
                            //emp.ImageUrl = _settings.NoUserImageName;
                        }

                        emp.CurrentCompany = _util.GetCurrentCompanyName(emp.CurrentJobTitle, emp.CurrentWorkingTitle);

                        employees.Add(emp);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error while scraping user profile card");
                }
            }


            Logger.Debug($"Employees in Network {empInNetwork}");
            Logger.Debug($"Employees not in Network {currentPageEmployees - empInNetwork}");
        }

        private bool LoginToLinkedIn()
        {
            bool loggedIn = false;

            try
            {
                Logger.Debug("Logging in to LinkedIn");

                isLoggedInToLinkedIn = DirectLinkedInLoginToPage(_settings.LIUserName, _settings.LIPassword);

                loggedIn = isLoggedInToLinkedIn;

                if (!loggedIn)
                {
                    throw new Exception("Login to LinkedIn failes, please check your credentials and logs");
                }
            }
            catch (Exception ex)
            {
                loggedIn = false;
                Logger.Error(ex, "Error while logging in to LinkedIn...");
                throw;
            }

            return loggedIn;
        }

        private bool DirectLinkedInLoginToPage(string userName, string password)
        {
            bool isLoggedIn = false;

            Logger.Debug($"Navigating to Login page {LIDirectLoginSelectors.LoginUrl}");
            NavigateToUrl(LIDirectLoginSelectors.LoginUrl);


            ri = r.Next(_settings.ScraperMinWait, _settings.ScraperMaxWait);
            Logger.Debug($"Sleeping for {ri * 100} milliseconds");
            Thread.Sleep(ri * 100);

            Logger.Debug($"Waiting for Login Form");
            _wait.Until(ExpectedConditions.ElementExists(By.CssSelector(LIDirectLoginSelectors.LoginFormCSSSel)));

            _driver.FindElement(By.Id(LIDirectLoginSelectors.UserNameTextId)).SendKeys(userName);
            _driver.FindElement(By.Id(LIDirectLoginSelectors.PasswordTextId)).SendKeys(password);

            Logger.Debug($"Submitting credentials...");
            _driver.FindElement(By.Id(LIDirectLoginSelectors.LoginButtonIdClick)).Click();


            ri = r.Next(_settings.ScraperMinWait, _settings.ScraperMaxWait);
            Thread.Sleep(ri * 100);
            Logger.Debug($"Submitted... sleeping for {ri * 100} milliseconds");

            Logger.Debug($"Waiting for LinkedIn home page...");
            _wait.Until(ExpectedConditions.ElementExists(By.Id(LIDirectLoginSelectors.ProfileNavButtonId)));

            string url = _util.GetDecodedUrl(_driver.Url.ToString());

            //if (url.Contains(LIDirectLoginSelectors.HomeUrlAfterLogin) || url.Contains(LIDirectLoginSelectors.JobUrlAfterLogin))
            if (!CurrentLIPageRequiresLogin(url))
            {
                Logger.Debug($"Successfully Logged in to LinkedIn...");
                isLoggedIn = true;
            }

            return isLoggedIn;
        }

        private bool CurrentLIPageRequiresLogin(string url)
        {
            bool requireLogin = false;

            try
            {
                if (url == string.Empty || url == "data:," || url.Contains(LILoginSelectors.LogInRedirectUrl2) || url.Contains(LILoginSelectors.LogInRedirectUrl)
                         || url.Contains(LILoginSelectors.LogInRedirectUrl3) || url.Contains(LILoginSelectors.LogInRedirectUrl4) || url.Contains(LILoginSelectors.LogInRedirectUrl5))
                {
                    requireLogin = true;
                }

                if (url.Contains("login") || url.Contains("/join?") || url.Contains("reg_redirect") || url.Contains("sessionRedirect")
                       || url.Contains("authwall?") || url.Contains("originalReferer"))
                {
                    requireLogin = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error while checking URL if it requires Login - {ex.Message}");
            }

            return requireLogin;

        }

        private void SearchResultPageScrollDown()
        {
            ri = r.Next(_settings.ScrollMinWait, _settings.ScrollMaxWait);
            Logger.Debug($"Sleeping for {ri * 500} milliseconds for Ajax requests");
            Thread.Sleep(ri * 500);

            Logger.Debug("Scrolling down page...");

            IJavaScriptExecutor jse = (IJavaScriptExecutor)_driver;
            jse.ExecuteScript($"window.scrollBy(0,600)", "");


            jse.ExecuteScript($"window.scrollBy(0,600)", "");

            ri = r.Next(_settings.ScrollMinWait, _settings.ScrollMaxWait);
            Logger.Debug($"Sleeping for {ri * 500} milliseconds for Ajax requests");
            Thread.Sleep(ri * 500);

            jse.ExecuteScript($"window.scrollBy(0,600)", "");

            ri = r.Next(_settings.ScrollMinWait, _settings.ScrollMaxWait);
            Logger.Debug($"Sleeping for {ri * 500} milliseconds for Ajax requests");
            Thread.Sleep(ri * 500);

            jse.ExecuteScript($"window.scrollBy(0,-700)", "");

            ri = r.Next(_settings.ScrollMinWait, _settings.ScrollMaxWait);
            Logger.Debug($"Sleeping for {ri * 500} milliseconds for Ajax requests");
            Thread.Sleep(ri * 500);

            IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(15.00));
            wait.Until(wd => ((IJavaScriptExecutor)_driver).ExecuteScript("return document.readyState").ToString() == "complete");
        }

        private void PageScrolDown(int offset = 1300, bool applyRandomwait = true)
        {
            //wait for loading new emps
            if (applyRandomwait)
            {
                ri = r.Next(_settings.ScrollMinWait, _settings.ScrollMaxWait);
                Thread.Sleep(ri * 100);
            }

            //scroll down to load the employees
            IJavaScriptExecutor jse = (IJavaScriptExecutor)_driver;
            jse.ExecuteScript($"window.scrollBy(0,{offset})", "");

            //wait for loading new emps
            if (applyRandomwait)
            {
                ri = r.Next(_settings.ScrollMinWait, _settings.ScrollMaxWait);
                Logger.Debug($"Sleeping for {ri * 500} milliseconds for Ajax requests");
                Thread.Sleep(ri * 500);
            }

            IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(15.00));
            wait.Until(wd => ((IJavaScriptExecutor)_driver).ExecuteScript("return document.readyState").ToString() == "complete");
        }

        private bool NavigateToUrl(string url)
        {
            bool ret = true;
            try
            {
                Logger.Debug($"Navigating to {url}");
                _driver.Navigate().GoToUrl(url);
                Logger.Debug($"Waiting for any Ajax requests to complete...");
                DoAjaxWait();
            }
            catch (Exception ex)
            {
                ret = false;
            }

            return ret;
        }

        private void LongRandomWait(int waitMSMultiple = 1000)
        {
            ri = r.Next(_settings.ScraperMinWait, _settings.ScraperMaxWait);
            int w = ri * waitMSMultiple;
            Logger.Debug($"Sleeping for {w} Milli Seconds");
            Thread.Sleep(w);
        }

        private void ShortRandomWait(int waitMSMultiple = 500)
        {
            ri = r.Next(_settings.ScraperMinWait, _settings.ScraperMaxWait);
            int w = ri * waitMSMultiple;
            Logger.Debug($"Sleeping for {w} Milli Seconds");
            Thread.Sleep(w);
        }

        private string GetNodeTextValue(IElement element, string attrName = "")
        {
            string value = string.Empty;

            try
            {
                if (element == null)
                {
                    //return string.Empty;
                }
                else if (!string.IsNullOrWhiteSpace(attrName))
                {
                    value = element.GetAttribute(attrName).Trim();
                }
                else
                {
                    value = element.TextContent.Trim();
                }
            }
            catch (Exception ex)
            {

            }

            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(lineSeparator, string.Empty).Replace(paragraphSeparator, string.Empty).Replace(",", ";");

        }

        private string GetWebElementsText(ReadOnlyCollection<IWebElement> collection, int index = 0)
        {
            string value = string.Empty;
            if (collection != null && collection.Count > 0)
            {
                try
                {
                    value = collection[index].Text.Trim();
                }
                catch (Exception ex) { }
            }

            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(lineSeparator, string.Empty).Replace(paragraphSeparator, string.Empty).Replace(",", ";");

        }

        private string GetSingleWebElementText(IWebElement elem)
        {
            string value = string.Empty;
            if (elem != null)
            {
                try
                {
                    value = elem.Text.Trim();
                }
                catch (Exception ex) { }
            }

            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(lineSeparator, string.Empty).Replace(paragraphSeparator, string.Empty).Replace(",", ";");


        }

        private void DoAjaxWait()
        {
            Logger.Debug($"Waiting for any Ajax requests to complete");
            _wait.Until(wd => ((IJavaScriptExecutor)_driver).ExecuteScript("return document.readyState").ToString() == "complete");
        }




    }
}
