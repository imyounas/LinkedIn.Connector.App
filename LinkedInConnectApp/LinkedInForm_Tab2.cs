using AutoMapper;
using LICommon;
using LICommon.Models;
using LIConnectScraperLib;
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
        List<LIUserData> liImportUserProfiles = null;
        BindingSource dgvImportUserProfilesSource = null;

        private void btnUserProfileImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            settings = SraperSettingsManager.GetSettings();
            if (settings != null && !string.IsNullOrWhiteSpace(settings.ConnReportPath))
            {
                openFileDialog1.InitialDirectory = settings.ConnReportPath;
            }
            else
            {
                openFileDialog1.InitialDirectory = @"C:\";
            }

            openFileDialog1.Title = "Import LinkedIn UserProfiles File";

            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;

            openFileDialog1.DefaultExt = "csv";
            openFileDialog1.Filter = "UserProfiles File (*.csv)|*.csv";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;

            openFileDialog1.ReadOnlyChecked = true;
            // openFileDialog1.ShowReadOnly = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string impFilePath = openFileDialog1.FileName;
                    this.txtUserProfileImport.Text = impFilePath;
                    try
                    {
                        liImportUserProfiles = CSVFileManager.ReadUserProfilesImportFile(impFilePath);
                        dgvImportUserProfilesSource = new BindingSource();
                        dgvImportUserProfilesSource.DataSource = liImportUserProfiles;

                        this.dgvUserProfileImport.Rows.Clear();
                        this.dgvUserProfileImport.DataSource = dgvImportUserProfilesSource;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Invalid Import File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    MessageBox.Show("Successfully Imported the UserProfiles", "File Import", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message, "Error Import File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDIrectMessageSchedule_Click(object sender, EventArgs e)
        {

            if (liImportUserProfiles == null || liImportUserProfiles.Count <= 0)
            {
                MessageBox.Show("No User Profiles found  for this action", "No Profiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedUserProfiles = liImportUserProfiles.Where(s => s.IsSelected).ToList();

            if (selectedUserProfiles == null || selectedUserProfiles.Count <= 0)
            {
                MessageBox.Show("No User Profiles Selected for this action", "No Profiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var done = TaskScheduler.Schedule(directMessageTaskName, this.dtpConnectSchedule.Value, ScheduleTaskEnum.DirectMessage);
            if (done)
            {
                try
                {
                    var dmdata = Mapper.Map<List<LIUserData>, List<LIUserDirectMessageReport>>(selectedUserProfiles);
                    CSVFileManager.WriteFile(Util.SchedulerDirectMessageFileName, ScheduleTaskEnum.DirectMessage, null, dmdata);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Creating Direct Message Schedule Data File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDirectMessageRequest_Click(object sender, EventArgs e)
        {
            try
            {
                if (liImportUserProfiles == null || liImportUserProfiles.Count <= 0)
                {
                    MessageBox.Show("No User Profiles found  for this action", "No Profiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var selectedUserProfiles = liImportUserProfiles.Where(s => s.IsSelected).ToList();

                if (selectedUserProfiles == null || selectedUserProfiles.Count <= 0)
                {
                    MessageBox.Show("No User Profiles Selected for this action", "No Profiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                busyTab = 2;

                ShowBusyForm_Tab2(true);
                var data = Mapper.Map<List<LIUserData>, List<LIUserDirectMessageReport>>(selectedUserProfiles);


                settings = SraperSettingsManager.GetSettings();
                var scraper = new LIScraper(settings);

                var done = scraper.StartDirectMessageFlow(data);

                scraper.ShutdownScraper();
                busyTab = 0;
                ShowBusyForm_Tab2(false);


                UpdateStatus("Generating Report File");
                string f = CSVFileManager.WriteDirectMessageReport(settings.DirMsgReportPath, data);

                MessageBox.Show($"Successfully Sent Direct Messages to selected UserProfiles. A report has also been generated {f} ", "Direct Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Creating Direct Message Schedule Data File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                busyTab = 0;
                ShowBusyForm_Tab2(false);
            }
        }

        private void chkSelectAllProfiles_CheckStateChanged(object sender, EventArgs e)
        {
            if (liImportUserProfiles == null)
            {
                this.chkSelectAllProfiles.CheckStateChanged -= chkSelectAllProfiles_CheckStateChanged;
                chkSelectAllProfiles.Checked = !chkSelectAllProfiles.Checked;
                this.chkSelectAllProfiles.CheckStateChanged += chkSelectAllProfiles_CheckStateChanged;
                return;
            }

            this.chkSelectAllProfiles.CheckStateChanged -= chkSelectAllProfiles_CheckStateChanged;

            if (liImportUserProfiles != null && liImportUserProfiles.Count > 0)
            {
                bool f = (chkSelectAllProfiles.CheckState == CheckState.Indeterminate || chkSelectAllProfiles.CheckState == CheckState.Unchecked) ? false : true;
                liImportUserProfiles.ForEach(u => u.IsSelected = f);
            }
            else
            {
                this.chkSelectAllProfiles.Checked = false;
            }

            dgvImportUserProfilesSource.ResetBindings(false);
            UpdateXofYUserProfilesSelectedLabel();

            this.chkSelectAllProfiles.CheckStateChanged += chkSelectAllProfiles_CheckStateChanged;
        }




        private void dgvUserProfileImport_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dgvUserProfileImport.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dgvUserProfileImport_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                this.chkSelectAllProfiles.CheckStateChanged -= chkSelectAllProfiles_CheckStateChanged;
                UpdateXofYUserProfilesSelectedLabel();
                this.chkSelectAllProfiles.CheckStateChanged += chkSelectAllProfiles_CheckStateChanged;
            }
        }

        private void UpdateXofYUserProfilesSelectedLabel()
        {
            if (liImportUserProfiles != null && liImportUserProfiles.Count > 0)
            {
                int total = liImportUserProfiles.Count;
                int selected = liImportUserProfiles.Where(u => u.IsSelected).Count();
                this.lblXofYProfilesSelected.Text = string.Format(this.xySelected, selected, total);

                if (selected == 0)
                {
                    this.chkSelectAllProfiles.CheckState = CheckState.Unchecked;
                }
                else if (total != selected)
                {
                    this.chkSelectAllProfiles.CheckState = CheckState.Indeterminate;
                }
                else
                {
                    if (total == selected)
                    {
                        this.chkSelectAllProfiles.CheckState = CheckState.Checked;
                    }
                }
            }
            else
            {
                this.lblXofYProfilesSelected.Text = string.Format(this.xySelected, 0, 0);
                this.chkSelectAllProfiles.CheckState = CheckState.Unchecked;
            }
        }

        private void ShowBusyForm_Tab2(bool isBusy)
        {
            if (isBusy)
            {
                this.Cursor = Cursors.WaitCursor;
            }
            else
            {
                this.Cursor = Cursors.Default;
            }

            this.grpUserProfileImport.Enabled = !isBusy;
            this.groupDirectMsgSchedule.Enabled = !isBusy;
            this.dgvUserProfileImport.Enabled = !isBusy;
            this.chkSelectAllProfiles.Enabled = !isBusy;
            this.rtbDirecMessage.Enabled = true;
            isScrapingNow = isBusy;
        }
    }
}
