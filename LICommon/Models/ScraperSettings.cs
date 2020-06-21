using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICommon.Models
{
    public class ScraperSettings
    {
        public int ScrollMinWait { get; set; }
        public int ScrollMaxWait { get; set; }

        public int ScraperMinWait { get; set; }
        public int ScraperMaxWait { get; set; }

        public int ImplicitTimeout { get; set; }


        public List<string> SpecialToekns { get; set; }

        public int ScraperBatchSize { get; set; }
        private int _ScraperTimeout;
        public int ScraperTimeout
        {
            get
            {
                return _ScraperTimeout;
            }
            set
            {
                _ScraperTimeout = value;
                ImplicitTimeout = _ScraperTimeout * 2;
            }
        }

        public bool UseOffScreenScraper { get; set; }

        public string LIUserName { get; set; }
        public string LIPassword { get; set; }
        public string IgnoreEmployeeTitle { get; set; }

        public int ConnMinWait { get; set; }
        public int ConnMaxWait { get; set; }
        public string ConnGreetings { get; set; }
        public string ConnMessage { get; set; }
        public string ConnReportPath { get; set; }

        public int DirMsgMinWait { get; set; }
        public int DirMsgMaxWait { get; set; }
        public string DirMsgGreetings { get; set; }
        public string DirMessage { get; set; }
        public string DirMsgReportPath { get; set; }

        public string DefaultKeyword { get; set; }

        public ScraperSettings()
        {
            this.IgnoreEmployeeTitle = "LinkedIn Member";
            //this.LIUserName = "";
            //this.LIPassword = "";

            ScrollMinWait = 1;
            ScrollMaxWait = 4;

            ScraperMinWait = 2;
            ScraperMaxWait = 4;

            ImplicitTimeout = 20;
            ScraperTimeout = 10;

            ConnMinWait = 25;
            ConnMaxWait = 50;

            DirMsgMinWait = 25;
            DirMsgMaxWait = 50;

            this.ConnGreetings = "Hi {FirstName} ,";
            this.ConnMessage = "I Would like to add you to my LinkedIn Network";


            this.DirMsgGreetings = "Hi {FirstName} ,";
            this.DirMessage = "I would liek to wish you happy new year 2018.";

            SpecialToekns = new List<string>();
            SpecialToekns.Add("{FirstName}");
            SpecialToekns.Add("{LastName}");
            SpecialToekns.Add("{JobTitle}");

            this.ScraperBatchSize = 10;

            DefaultKeyword = "agent in";

            UseOffScreenScraper = false;
        }
    }

}
