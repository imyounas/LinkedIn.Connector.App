using AutoMapper;
using LICommon;
using LICommon.Models;
using LIConnectScraperLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinkedInConnectApp
{
    public partial class LinkedInForm
    {
        int pageNo = 0;
        LIScraper connScraper = null;
        ScraperSettings settings = null;
        SearchCriteria searchCriteria = null;
        List<LIUserData> liUserProfiles = null;
        BindingSource dgvLinkedInScrapingResultSource = null;

        string xySelected = "{0} of {1} UserProfile Selected";

        private void ShowBusyForm_Tab1(bool isBusy)
        {
            if (isBusy)
            {
                this.Cursor = Cursors.WaitCursor;
            }
            else
            {
                this.Cursor = Cursors.Default;
            }

            this.grpSearch.Enabled = !isBusy;
            this.btnSearchLinkedIn.Enabled = !isBusy;
            this.dgvLinkedInResult.Enabled = !isBusy;
            this.btnLoadMore.Enabled = !isBusy;
            this.btnDoneScraping.Enabled = !isBusy;
            this.chkSelectAll.Enabled = !isBusy;
            this.rtbConnectLog.Enabled = true;

            isScrapingNow = isBusy;
        }


        private void dgvLinkedInResult_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

        }

        private async void btnSearchLinkedIn_Click(object sender, EventArgs e)
        {

            try
            {
                busyTab = 1;

                settings = SraperSettingsManager.GetSettings();

                if (string.IsNullOrWhiteSpace(settings.LIUserName) || string.IsNullOrWhiteSpace(settings.LIPassword))
                {
                    throw new Exception("LinkedIn UserName or Password cannot be blank");
                }

                searchCriteria = new SearchCriteria();
                searchCriteria.Keyword = this.txtKeyword.Text;
                searchCriteria.City = this.txtCity.Text;
                searchCriteria.State = this.cmbState.Text;
                searchCriteria.Company = this.txtCompany.Text;
                
                this.dgvLinkedInResult.Rows.Clear();
                this.rtbConnectLog.Clear();

                liUserProfiles = new List<LIUserData>();
                dgvLinkedInScrapingResultSource = new BindingSource();
                dgvLinkedInScrapingResultSource.DataSource = liUserProfiles;
                dgvLinkedInResult.DataSource = dgvLinkedInScrapingResultSource;

                this.grpConnectNow.Enabled = false;
                this.grpConnectSchedule.Enabled = false;

                connScraper = new LIScraper(settings);
                pageNo = 1;

                ScrapeLinkedIn(connScraper, searchCriteria, pageNo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Scraping Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                busyTab = 0;
            }

        }


        private async void ScrapeLinkedIn(LIScraper scraper, SearchCriteria searchCriteria, int pageNo)
        {
            try
            {
                isScrapingNow = true;
                //  this.dgvLinkedInResult.Columns.Clear();
                busyTab = 1;

                ShowBusyForm_Tab1(true);
                ScraperRunner runner = new ScraperRunner();

                 var result = await runner.StartScraping(searchCriteria, pageNo, scraper);
                
                ShowBusyForm_Tab1(false);

               
                this.grpSearch.Enabled = false;
                this.btnDoneScraping.Enabled = true;

                this.btnLoadMore.Enabled = result.AreThereMoreRecords;
                this.btnLoadMore.Enabled = result.AreThereMoreRecords;

                liUserProfiles.AddRange(result.UserProfiles);
                dgvLinkedInScrapingResultSource.ResetBindings(false);


                isScrapingNow = false;
                MessageBox.Show("LinkedIn Scraping Result", "Scraping Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                busyTab = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Scraping Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowBusyForm_Tab1(false);
                busyTab = 0;
            }
        }

        private void btnConnectSchedule_Click (object sender, EventArgs e)
        {

            if (liUserProfiles == null || liUserProfiles.Count <= 0)
            {
                MessageBox.Show("No User Profiles found  for this action", "No Profiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }


            var selectedUserProfiles = liUserProfiles.Where(s => s.IsSelected).ToList();

            if (selectedUserProfiles == null || selectedUserProfiles.Count <= 0)
            {
                MessageBox.Show("No User Profiles Selected for this action", "No Profiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var done = TaskScheduler.Schedule(connectionRequestTaskName, this.dtpConnectSchedule.Value, ScheduleTaskEnum.ConnectionRequest);
            if (done)
            {
                try
                {  
                    var data = Mapper.Map<List<LIUserData>, List<LIUserConnectRequestReport>>(selectedUserProfiles);
                   
                    CSVFileManager.WriteFile(Util.SchedulerConnectionRequestFileName, ScheduleTaskEnum.ConnectionRequest, data,null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Creating Connection Request Schedule Data File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnConnectRequest_Click(object sender, EventArgs e)
        {
            try
            {
                if (liUserProfiles == null || liUserProfiles.Count <= 0)
                {
                    MessageBox.Show("No User Profiles found  for this action", "No Profiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var selectedUserProfiles = liUserProfiles.Where(s => s.IsSelected).ToList();

                if(selectedUserProfiles == null || selectedUserProfiles.Count <= 0)
                {
                    MessageBox.Show("No Profiles Selected for this action", "No Profiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                busyTab = 1;

                ShowBusyForm_Tab1(true);
                
                var data = Mapper.Map<List<LIUserData>, List<LIUserConnectRequestReport>>(selectedUserProfiles);

                settings = SraperSettingsManager.GetSettings();
                var scraper = new LIScraper(settings);

             
                var done = scraper.StartConnectRequestFlow(data);

                ShowBusyForm_Tab1(false);
                scraper.ShutdownScraper();

                UpdateStatus("Generating Report File");
                string f = CSVFileManager.WriteConnectionRequestReport(settings.ConnReportPath, data);

                MessageBox.Show($"Successfully Sent Connection Requests to selected UserProfiles. A report has also been generated {f} ", "Connection Request", MessageBoxButtons.OK, MessageBoxIcon.Information);
                busyTab = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Creating Direct Message Schedule Data File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                busyTab = 0;
                ShowBusyForm_Tab1(false);
            }
        }


        private void btnLoadMore_Click(object sender, EventArgs e)
        {
            try
            {
                if (connScraper == null)
                {
                    throw new Exception("Please first initialize Scraper Object");
                }

                busyTab = 1;
                pageNo++;
                ScrapeLinkedIn(connScraper, searchCriteria, pageNo);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Scraping Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowBusyForm_Tab1(false);
                busyTab = 0;
            }


        }

        private void btnDoneScraping_Click(object sender, EventArgs e)
        {
            this.grpSearch.Enabled = true;
            this.grpConnectNow.Enabled = true;
            this.grpSearch.Enabled = true;
            this.grpConnectSchedule.Enabled = true;
            this.btnSearchLinkedIn.Enabled = true;
            this.btnLoadMore.Enabled = false;
            this.btnDoneScraping.Enabled = true;

            busyTab = 0;
            CloseScraper();
        }

        private void chkSelectAll_CheckStateChanged(object sender, EventArgs e)
        {
            if (liUserProfiles == null)
            {
                this.chkSelectAll.CheckStateChanged-= chkSelectAll_CheckStateChanged;
                chkSelectAll.Checked = !chkSelectAll.Checked;
                this.chkSelectAll.CheckStateChanged += chkSelectAll_CheckStateChanged;
                return;
            }


            this.chkSelectAll.CheckStateChanged -= chkSelectAll_CheckStateChanged;

            if (liUserProfiles != null && liUserProfiles.Count > 0)
            {
                bool f = (chkSelectAll.CheckState == CheckState.Indeterminate || chkSelectAll.CheckState == CheckState.Unchecked) ? false : true;
                liUserProfiles.ForEach(u => u.IsSelected = f);
            }
            else
            {
                this.chkSelectAll.Checked = false;
            }

            dgvLinkedInScrapingResultSource.ResetBindings(false);
            UpdateXofYSelectedLabel();

            this.chkSelectAll.CheckStateChanged += chkSelectAll_CheckStateChanged;
        }

       
     

        private void dgvLinkedInResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dgvLinkedInResult.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dgvLinkedInResult_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                this.chkSelectAll.CheckStateChanged -= chkSelectAll_CheckStateChanged;
                UpdateXofYSelectedLabel();
                this.chkSelectAll.CheckStateChanged += chkSelectAll_CheckStateChanged;
            }
        }

        private void UpdateXofYSelectedLabel()
        {
            if (liUserProfiles != null && liUserProfiles.Count > 0)
            {
                int total = liUserProfiles.Count;
                int selected = liUserProfiles.Where(u => u.IsSelected).Count();
                this.lblXofYSelected.Text = string.Format(this.xySelected, selected, total);

                if (selected == 0)
                {
                    this.chkSelectAll.CheckState = CheckState.Unchecked;
                }
                else if (total != selected)
                {
                    this.chkSelectAll.CheckState = CheckState.Indeterminate;
                }
                else
                {
                    if (total == selected)
                    {
                        this.chkSelectAll.CheckState = CheckState.Checked;
                    }
                }
            }
            else
            {
                this.lblXofYSelected.Text = string.Format(this.xySelected, 0, 0);
                this.chkSelectAll.CheckState = CheckState.Unchecked;
            }
        }


        private Image GetImageFromUrl(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            using (HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (Stream stream = httpWebReponse.GetResponseStream())
                {
                    return Image.FromStream(stream);
                }
            }
        }

        private void CloseScraper()
        {
            try
            {
                if (connScraper != null)
                {
                    connScraper.ShutdownScraper();
                }

                connScraper = null;
                settings = null;
                searchCriteria = null;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Scraper Shutdown Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

    }
}
