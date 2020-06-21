using AppLogger;
using LICommon.Models;
using LIConnectScraperLib.Selectors;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LIConnectScraperLib
{
    public class ScraperUtil
    {
        Random r = null;
        int ri = 1;
        bool useOffScreenScraper = false;
        public ScraperUtil(bool useOffScreenScraper = false)
        {
            r = new Random();
            this.useOffScreenScraper = useOffScreenScraper;
        }

        public ChromeOptions GetChromeProfile()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("start-maximized");
            options.AddArguments("--test-type");
            options.AddArgument("no-sandbox");
            //option.AddExtensions(@"EXTENSION PATH");
            if (useOffScreenScraper)
            {
                options.AddArguments("--headless");
            }



            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-infobars");

            //options.AddArgument("--blink-settings=imagesEnabled=false");

            //options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);




            return options;
        }

        public int GetNumberFromString(string strigCount)
        {
            if (string.IsNullOrWhiteSpace(strigCount))
            {
                return 0;
            }
            //See all 11 employees 
            string empCount = new String(strigCount.ToCharArray().Where(c => Char.IsDigit(c)).ToArray());

            int count = 0;
            Int32.TryParse(empCount.Trim(), out count);

            return count;
        }

        public bool ContainsInsensitive(string source, string find)
        {

            return source != null && find != null && source.IndexOf(find, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public string GetCurrentCompanyName(string designation1, string designation2)
        {
            string company = string.Empty;
            string at = " at ";
            company = GetCompanyNameFromString(designation1, at);

            if (string.IsNullOrWhiteSpace(company))
            {
                company = GetCompanyNameFromString(designation2, at);
            }

            return company;
        }

        private string GetCompanyNameFromString(string source, string at)
        {
            string company = string.Empty;
            try
            {
                if (source != null && source.Length > 0 && source.Contains(at))
                {
                    int idx = source.IndexOf(at);
                    if (idx > 0)
                    {
                        int l = source.Length;
                        int l2 = l - (idx + at.Length);
                        company = source.Substring((idx + at.Length), l2);
                    }
                }

            }
            catch (Exception ex)
            {

            }

            return company;
        }

        public string GetDecodedUrl(string url)
        {
            return System.Net.WebUtility.HtmlDecode(url);
        }

        public static string GetInitials(string names, string separator)
        {
            // Extract the first character out of each block of non-whitespace
            Regex extractInitials = new Regex(@"\s*([^\s])[^\s]*\s*");
            return extractInitials.Replace(names, "$1" + separator).ToUpper();
        }

        public static string[] GetAllNames(string name)
        {
            string[] all = name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var trimedNames = all.Select(x => x.Trim()).ToArray();
            return trimedNames;

        }
        public static Tuple<string, string> SplitUserName(string name)
        {
            int idx = name.LastIndexOf(" ");
            string first = name;
            string last = string.Empty;
            if (idx > 0)
            {
                first = name.Substring(0, idx);
                last = name.Substring(idx, name.Length - idx);
            }

            return Tuple.Create(first.Trim().Replace(" ", ""), last.Trim());
        }

        public static Tuple<string, string> SplitUserName2(string name)
        {
            var arr = GetAllNames(name);
            string first = name;
            string last = string.Empty;

            if (arr != null && arr.Count() > 0)
            {
                first = !string.IsNullOrWhiteSpace(arr.FirstOrDefault()) ? arr.First() : name ;
                last = !string.IsNullOrWhiteSpace(arr.LastOrDefault()) ? arr.Last() : "";
            }
            
            return Tuple.Create(first.Trim(), last.Trim());
        }
        private static string CleanEmployeeName(string employeeName)
        {
            string[] cleanName = employeeName.Split(new char[] { ',' });
            employeeName = cleanName[0];

            cleanName = employeeName.Split(new char[] { '-' });
            employeeName = cleanName[0];

            cleanName = employeeName.Split(new char[] { '|' });
            employeeName = cleanName[0];

            cleanName = employeeName.Split(new char[] { '(' });
            employeeName = cleanName[0];

            cleanName = employeeName.Split(new char[] { '{' });
            employeeName = cleanName[0];

            cleanName = employeeName.Split(new char[] { '[' });
            employeeName = cleanName[0];


            return employeeName;
        }
        public (string, string) GetGreetingAndMessage(LIUserData up, ScraperSettings _settings, string greeting, string message)
        {

            var tp = SplitUserName2(up.ProfileTitle);

            if (!string.IsNullOrWhiteSpace(greeting))
            {
                greeting = ReplaceTokens(_settings, greeting, tp.Item1, tp.Item2, up.CurrentJobTitle);
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                message = ReplaceTokens(_settings, message, tp.Item1, tp.Item2, up.CurrentJobTitle);
            }

            return (greeting,message);
        }

        private string ReplaceTokens(ScraperSettings _settings, string message, string fname, string lname, string job)
        {

            foreach (var st in _settings.SpecialToekns)
            {
                switch (st.ToLower())
                {
                    case "{firstname}":
                        {
                            message = message.Replace(st, fname);
                        }
                        break;
                    case "{lastname}":
                        {
                            message = message.Replace(st, lname);
                        }
                        break;
                    case "{jobtitle}":
                        {
                            message = message.Replace(st, job);
                        }
                        break;
                }
            }

            return message;
        }
    }
}
