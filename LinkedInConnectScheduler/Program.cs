using AppLogger;
using LICommon;
using LICommon.Models;
using LIConnectScraperLib;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkedInConnectScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console()
               .WriteTo.File("logs\\LIConnectScheduleAppLogs.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();


            Logger.DebugMessaged += Logger_DebugMessaged;
            Logger.ErrorMessaged += Logger_ErrorMessaged;

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<LIUserData, LIUserConnectRequestReport>().ReverseMap();
                cfg.CreateMap<LIUserData, LIUserDirectMessageReport>().ReverseMap();
            });

            Log.Debug("Readig Command line arguments...");
            int argNumber = 0;
            try
            {
                if (args.Length > 0)
                {
                    //argNumber = 1;//
                    argNumber =  Convert.ToInt32(args[0]);
                    Log.Debug($"Command is {argNumber}");
                    string err = "";

                    switch (argNumber)
                    {
                       
                        case 1:
                            {
                                Log.Debug("Performing Connection Request Workflow...");
                                var data = CSVFileManager.ReadConnectionRequestSchedulerData(out err);
                                if (data.Count > 0)
                                {
                                    var settings = SraperSettingsManager.GetSettings();
                                    var scraper = new LIScraper(settings);

                                    var done = scraper.StartConnectRequestFlow(data);
                                    scraper.ShutdownScraper();

                                    Log.Debug("Generating Report File");
                                    string f = CSVFileManager.WriteConnectionRequestReport(settings.ConnReportPath, data);
                                    Log.Debug($"Report File Generated {f}");
                                }
                                else
                                {
                                    Log.Debug("No Connection Request Scheduling data...");
                                    if (!string.IsNullOrWhiteSpace(err))
                                    {
                                        Log.Error(err);
                                    }
                                }

                            }
                            break;
                        case 2:
                            {
                                Log.Debug("Performing Direct Message Workflow...");
                                var data = CSVFileManager.ReadDirectMessageSchedulerData(out err);
                                if (data.Count > 0)
                                {
                                    var settings = SraperSettingsManager.GetSettings();
                                    var scraper = new LIScraper(settings);

                                    var done = scraper.StartDirectMessageFlow(data);
                                    scraper.ShutdownScraper();


                                    Log.Debug("Generating Report File");
                                    string f = CSVFileManager.WriteDirectMessageReport(settings.DirMsgReportPath, data);
                                    Log.Debug($"Report File Generated {f}");
                                }
                                else
                                {
                                    Log.Debug("No Direct Message Scheduling data...");
                                    if (!string.IsNullOrWhiteSpace(err))
                                    {
                                        Log.Error(err);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error while running scheduler Task - {argNumber}");
            }


            Log.Debug("All Done , Exiting...");

            return;

            //TestLinkedinScraping();
        }

        public static void TestLinkedinScraping()
        {
            //SearchCriteria sc = new SearchCriteria();
            //sc.City = "New York City";
            //sc.State = "NY";
            //sc.Company = "Keller Williams";

            //sc.Keyword = "agent in";
            //sc.ScrapingBatchSize = 30;

            //LIConnectScraperLib.ScraperSettings settings = new LIConnectScraperLib.ScraperSettings();
            //LIScraper scraper = new LIScraper(settings);


            //List<LIUserData> data = new List<LIUserData>();
            //bool moreRecords = true;
            //int pageNo = 0;
            //while (moreRecords)
            //{
            //    sc.PageNo++;
            //    var d = scraper.StartScraping(sc, out moreRecords);
            //    data.AddRange(d);

            //}


            //Console.ReadLine();
            //scraper.ShutdownScraper();

        }

        private static void Logger_ErrorMessaged(Exception ex, string message)
        {
            if (ex != null)
            {

                Log.Error(message);
            }
            else
            {

                Log.Error(message);
            }
        }

        private static void Logger_DebugMessaged(string message)
        {
            Log.Debug(message);
        }

    }
}
