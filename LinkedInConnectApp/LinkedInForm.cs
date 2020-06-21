using AppLogger;
using AutoMapper;
using LICommon;
using LICommon.Models;
using LIConnectScraperLib;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinkedInConnectApp
{
    public partial class LinkedInForm : Form
    {

        BindingSource sourcedgvLinkedIn = null;
        List<LIUserData> scrapingResult = null;
        bool isScrapingNow = false;
        int activeTab = 0;
        int busyTab = 0;

        string connectionRequestTaskName = "LinkedIn Connection Request Scheduled Task";
        string directMessageTaskName = "LinkedIn Direct Message Scheduled Task";

        public LinkedInForm()
        {
            InitializeComponent();

            Logger.DebugMessaged += Logger_DebugMessaged;
            Logger.ErrorMessaged += Logger_ErrorMessaged;

            Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .WriteTo.Console()
              .WriteTo.File("logs\\LinkedInScraperWinAppLogs.txt", rollingInterval: RollingInterval.Day)
              .CreateLogger();

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<LIUserData, LIUserConnectRequestReport>().ReverseMap();
                cfg.CreateMap<LIUserData, LIUserDirectMessageReport>().ReverseMap();
            });

            this.dgvLinkedInResult.AutoGenerateColumns = false;
            this.dgvUserProfileImport.AutoGenerateColumns = false;

           
        }

        public static DateTime GetNistTime()
        {
            DateTime dateTime = DateTime.MinValue;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://nist.time.gov/actualtime.cgi?lzbc=siqm9b");
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore); //No caching
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader stream = new StreamReader(response.GetResponseStream());
                string html = stream.ReadToEnd();//<timestamp time=\"1395772696469995\" delay=\"1395772696469995\"/>
                string time = Regex.Match(html, @"(?<=\btime="")[^""]*").Value;
                double milliseconds = Convert.ToInt64(time) / 1000.0;
                dateTime = new DateTime(1970, 1, 1).AddMilliseconds(milliseconds).ToLocalTime();
            }

            return dateTime;
        }

        private void Logger_ErrorMessaged(Exception ex, string message)
        {

            if (ex != null)
            {
                UpdateStatus($"{message} - Exception: {ex.Message}");
                Log.Error(message);
            }
            else
            {
                UpdateStatus($"{message}");
                Log.Error(message);
            }
        }

        private void Logger_DebugMessaged(string message)
        {
            UpdateStatus($"{message}");
            Log.Debug(message);
        }

        public void UpdateStatus(string status)
        {
            if (InvokeRequired)
                Invoke(new Action<string>(UpdateLableStatus), status);
            else
                UpdateLableStatus(status);
        }

        private void UpdateLableStatus(string Item)
        {
            if (activeTab == 1)
            {
                this.rtbDirectmessageLog.AppendText(Environment.NewLine);
                this.rtbDirectmessageLog.AppendText(Item);
                this.rtbDirecMessage.ScrollToCaret();
            }
            else
            {
                this.rtbConnectLog.AppendText(Environment.NewLine);
                this.rtbConnectLog.AppendText(Item);
                this.rtbConnectLog.ScrollToCaret();
            }

        }



        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (busyTab > 0 || isScrapingNow)
            {
                e.Cancel = true;
                return;
            }
        }


        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {

            int idx = this.tabControl.SelectedIndex;
            activeTab = idx;

            switch (idx)
            {
                case 0:
                    {
                        this.txtKeyword.Text = settings.DefaultKeyword;
                    }
                    break;
                case 1:
                    {

                    }
                    break;
                case 2:
                    {
                        ApplyPrefrences();
                    }
                    break;
            }
        }

        private void LinkedInForm_Shown(object sender, EventArgs e)
        {
            settings = SraperSettingsManager.GetSettings();
            this.txtKeyword.Text = settings.DefaultKeyword;
          
        }

        
    }
}
