using LICommon.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICommon
{
    public class SraperSettingsManager
    {
        public static ScraperSettings GetSettings()
        {
            ScraperSettings settings = null;
            try
            {
                string data = File.ReadAllText(Util.ScraperSettingsFileName);
                settings = JsonConvert.DeserializeObject<ScraperSettings>(data);
            }
            catch (Exception ex)
            {
                settings = new ScraperSettings();
            }

            return settings;
        }

        public static bool SaveSettings(ScraperSettings settings)
        {
            string data = JsonConvert.SerializeObject(settings);
            File.WriteAllText(Util.ScraperSettingsFileName, data);
            return true;
        }
    }
}
