using LICommon.Models;
using LIConnectScraperLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LinkedInConnectApp
{
    public class ScraperRunner
    {
        public async Task<ScrapingResult> StartScraping(SearchCriteria sc, int pageNo, LIScraper scraper)
        {
            ScrapingResult result = await StartScraingTask(sc, pageNo, scraper);
            return result;
        }

        private Task<ScrapingResult> StartScraingTask(SearchCriteria sc, int pageNo, LIScraper scraper)
        {

            return Task.Run<ScrapingResult>(() => ScrapingTaskOperation(sc, pageNo, scraper));
        }

        private ScrapingResult ScrapingTaskOperation(SearchCriteria sc, int pageNo, LIScraper scraper)
        {
            bool morerecords = false;
            ScrapingResult res = new ScrapingResult();
            res.UserProfiles = scraper.StartUserProfilesScraping(sc, pageNo, out morerecords);
            res.AreThereMoreRecords = morerecords;

            //var res = new ScrapingResult();
            //res.AreThereMoreRecords = false;
            //res.UserProfiles = new List<LIUserData>() { new LIUserData() { ConnectionDegree = "2nd", CurrentWorkingTitle = "jobee", ProfileTitle = "imran" } };
            //Thread.Sleep(10000);


            return res;
        }
    }
}
