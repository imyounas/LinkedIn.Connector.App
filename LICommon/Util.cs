using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICommon
{
    public class Util
    {
        public static string ScraperSettingsFileName = "ScraperSettingsFile.json";

        public static string SchedulerConnectionRequestFileName = "SchedulerConnectionRequestData.csv";
        public static string SchedulerDirectMessageFileName = "SchedulerDirectMessageData.csv";

        public static string LinkedInConnectionRequestFileName(DateTime dt)
        {
            return $"LinkedInConnectionRequest_Report_{String.Format("{0:ddMMyy_hhmm}", dt)}.csv";
        }

        public static string LinkedInDirectMessageFileName(DateTime dt)
        {
            return $"LinkedInDirectMessage_Report_{String.Format("{0:ddMMyy_hhmm}", dt)}.csv";
        }
    }
}
