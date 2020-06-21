using LICommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinkedInConnectApp
{
    public partial class LinkedInForm 
    {
        private void btnSavePrefrences_Click(object sender, EventArgs e)
        {
            SaveAppPrefrences();
        }

        private void btnConnMsgPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            folderDlg.Description = "Select folder for Connection Request Reports";

            DialogResult result = folderDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.txtConnMsgReportPath.Text = folderDlg.SelectedPath;
                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        }

        private void btnDirecMsgPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            folderDlg.Description = "Select folder for Direct Message Reports";

            DialogResult result = folderDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.txtDirecMsgReportPath.Text = folderDlg.SelectedPath;
                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        }

        private void SaveAppPrefrences()
        {

            try
            {
               settings = SraperSettingsManager.GetSettings();

                settings.LIUserName = this.txtUserName.Text;
                settings.LIPassword = this.txtPassword.Text;

                if (string.IsNullOrWhiteSpace(settings.LIUserName) || string.IsNullOrWhiteSpace(settings.LIPassword))
                {
                    throw new Exception("LinkedIn UserName or Password cannot be blank");
                }

                settings.ScraperMaxWait = (int)this.numScraperMaxWait.Value;
                settings.ScraperMinWait = (int)this.numScraperMinWait.Value;

                if (settings.ScraperMaxWait < settings.ScraperMinWait)
                {
                    throw new Exception("ScraperMaxWait cannot be less than ScraperMinWait");
                }

                settings.ScraperTimeout = (int)this.numScraperTimeout.Value;
                settings.ScraperBatchSize = (int)this.numBatchSize.Value;

                settings.ScrollMinWait = (int)this.numScraperScrollMinWait.Value;
                settings.ScrollMaxWait = (int)this.numScraperScrollMaxWait.Value;

                settings.ConnMinWait = (int)this.numMinConnMsgSec.Value;
                settings.ConnMaxWait = (int)this.numMaxConnMsgSec.Value;
                settings.ConnGreetings = this.txtConnMsgGreetings.Text;
                settings.ConnMessage = this.rtbConnMessage.Text;
                settings.ConnReportPath = this.txtConnMsgReportPath.Text;

                settings.DirMsgMinWait = (int)this.numMinDirecMsgSec.Value;
                settings.DirMsgMaxWait = (int)this.numMaxDirecMsgSec.Value;
                settings.DirMsgGreetings = this.txtDirecMsgGreetings.Text;
                settings.DirMessage = this.rtbDirecMessage.Text;
                settings.DirMsgReportPath = this.txtDirecMsgReportPath.Text;

                settings.DefaultKeyword = this.txtDefualtKeyword.Text;
                settings.UseOffScreenScraper = this.chkOffScreenScraping.Checked;

                SraperSettingsManager.SaveSettings(settings);

                MessageBox.Show("Successfully Updated Scraper Settings", "Settings Update", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Save Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void ApplyPrefrences()
        {
            try
            {
                 settings = SraperSettingsManager.GetSettings();

                this.txtUserName.Text = settings.LIUserName;
                this.txtPassword.Text = settings.LIPassword;

                this.numScraperMaxWait.Value = settings.ScraperMaxWait;
                this.numScraperMinWait.Value = settings.ScraperMinWait;

                this.numScraperTimeout.Value = settings.ScraperTimeout;
                this.numBatchSize.Value = settings.ScraperBatchSize;

                this.numScraperScrollMinWait.Value = settings.ScrollMinWait;
                this.numScraperScrollMaxWait.Value = settings.ScrollMaxWait;


                this.numMinConnMsgSec.Value = settings.ConnMinWait;
                this.numMaxConnMsgSec.Value = settings.ConnMaxWait;
                this.txtConnMsgGreetings.Text = settings.ConnGreetings;
                this.rtbConnMessage.Text = settings.ConnMessage;
                this.txtConnMsgReportPath.Text = settings.ConnReportPath;

                this.numMinDirecMsgSec.Value = settings.DirMsgMinWait;
                this.numMaxDirecMsgSec.Value = settings.DirMsgMaxWait;
                this.txtDirecMsgGreetings.Text = settings.DirMsgGreetings;
                this.rtbDirecMessage.Text = settings.DirMessage;
                this.txtDirecMsgReportPath.Text = settings.DirMsgReportPath;

                this.txtDefualtKeyword.Text = settings.DefaultKeyword;
                this.chkOffScreenScraping.Checked = settings.UseOffScreenScraper;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Applying Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
