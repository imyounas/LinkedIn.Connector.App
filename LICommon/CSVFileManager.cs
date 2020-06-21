using CsvHelper;
using LICommon.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICommon
{
    public class CSVFileManager
    {
        public static List<LIUserData> ReadUserProfilesImportFile(string filePath)
        {
            List<LIUserData> LIUserData = null;
            if (File.Exists(filePath))
            {
                using (TextReader writer = File.OpenText(filePath))
                {
                    var csv = new CsvReader(writer);
                    LIUserData = csv.GetRecords<LIUserData>().ToList();
                }
            }
            else
            {
                LIUserData = new List<LIUserData>();
            }
            return LIUserData;
        }

        public static List<LIUserDirectMessageReport> ReadDirectMessageSchedulerData(out string error)
        {
            List<LIUserDirectMessageReport>  data = null;
            error = "";
            string filePath = Util.SchedulerDirectMessageFileName;

            try
            {
                if (File.Exists(filePath))
                {
                    using (TextReader writer = File.OpenText(filePath))
                    {
                        var csv = new CsvReader(writer);
                        data = csv.GetRecords<LIUserDirectMessageReport>().ToList();
                    }
                }
                else
                {
                    error = $"No File found at specified path {filePath}";

                    data = new List<LIUserDirectMessageReport>();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return data;
        }

        public static List<LIUserConnectRequestReport> ReadConnectionRequestSchedulerData(out string error)
        {
            List<LIUserConnectRequestReport> data = null;
            error = "";
            string filePath = Util.SchedulerConnectionRequestFileName;
            try
            {
                if (File.Exists(filePath))
                {
                    using (TextReader writer = File.OpenText(filePath))
                    {
                        var csv = new CsvReader(writer);
                        data = csv.GetRecords<LIUserConnectRequestReport>().ToList();
                    }
                }
                else
                {
                    error = $"No File found at specified path {filePath}";
                    data = new List<LIUserConnectRequestReport>();
                }
            }
            catch (Exception ex)
            {

                error = ex.Message;
            }
            return data;
        }

        public static string WriteFile(string filePath , ScheduleTaskEnum type , List<LIUserConnectRequestReport> connData , List<LIUserDirectMessageReport> msgData )
        {
            using (TextWriter writer = new StreamWriter(filePath))
            {
                var csv = new CsvWriter(writer);
                if (type == ScheduleTaskEnum.ConnectionRequest)
                {
                    csv.WriteRecords(connData);
                }
                else
                {
                    csv.WriteRecords(msgData);
                }
            }

            return filePath;
        }

        public static string WriteDirectMessageReport(string path,  List<LIUserDirectMessageReport> msgData)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                path = "";
            }
            path = Path.Combine(path, Util.LinkedInDirectMessageFileName(DateTime.Now));
            return WriteFile(path, ScheduleTaskEnum.DirectMessage, null, msgData);
        }

        public static string WriteConnectionRequestReport(string path, List<LIUserConnectRequestReport> connData)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = "";
            }

            path = Path.Combine(path, Util.LinkedInConnectionRequestFileName(DateTime.Now));
            return WriteFile(path, ScheduleTaskEnum.ConnectionRequest, connData, null);
        }
    }
}
