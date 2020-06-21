using AppLogger;
using LICommon;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinkedInConnectApp
{
    public class TaskScheduler
    {
        public static bool Schedule(string taskName,DateTime taskTime,   ScheduleTaskEnum type)
        {
            bool done = false;
            try
            {
                string taskProcess = ConfigurationManager.AppSettings["SchedulerTaskExePath"];
                string scheduleTaskUserId = ConfigurationManager.AppSettings["ScheduledTaskUserId"];
                string scheduleTaskConnectionInterval = ConfigurationManager.AppSettings["ScheduleTaskConnectionInterval"];
                string scheduleTaskDirectMessageInterval = ConfigurationManager.AppSettings["ScheduleTaskDirectMessageInterval"];

                if (string.IsNullOrEmpty(taskProcess))
                {
                    MessageBox.Show("No Scheduler App Path found", "Scheduler App", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (string.IsNullOrEmpty(scheduleTaskUserId))
                {
                    scheduleTaskUserId = "NT AUTHORITY\\SYSTEM";
                }

                
                int runtimeParam = (int)type;
                int schInterval = runtimeParam == 1 ? Convert.ToInt32(scheduleTaskConnectionInterval) : Convert.ToInt32(scheduleTaskDirectMessageInterval);

                string taskDirectory = Path.GetDirectoryName(taskProcess);

                if (string.IsNullOrWhiteSpace(taskDirectory))
                {
                    taskDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                }

                using (TaskService ts = new TaskService())
                {
                    try
                    {
                        TaskDefinition td = ts.NewTask();
                        td.RegistrationInfo.Description = taskName;
                        td.Principal.LogonType = TaskLogonType.ServiceAccount;
                        td.Principal.UserId = scheduleTaskUserId;
                        DateTime time = taskTime;
                        DailyTrigger dt = (DailyTrigger)td.Triggers.Add(new DailyTrigger { DaysInterval = (short)schInterval, StartBoundary = time });

                        td.Actions.Add(new ExecAction(taskProcess, runtimeParam.ToString(), taskDirectory));
                        td.Principal.RunLevel = TaskRunLevel.Highest;
                        td.Settings.StopIfGoingOnBatteries = false;
                        td.Settings.ExecutionTimeLimit = new TimeSpan(0, 1, 0, 0);

                        try
                        {
                            ts.RootFolder.DeleteTask(taskName, true);
                            //Logger.Debug("To update the task, successfully deleted the old ScheduledTask" + taskName);
                        }
                        catch (Exception ex)
                        {
                            if (ex.GetType().ToString() != "System.IO.FileNotFoundException")
                            {
                                Logger.Debug($"Error on Deleting old Task {taskName}");
                            }
                        }

                        Microsoft.Win32.TaskScheduler.Task tk = ts.RootFolder.RegisterTaskDefinition(taskName, td, TaskCreation.CreateOrUpdate, scheduleTaskUserId, null, TaskLogonType.ServiceAccount);

                        MessageBox.Show($"{taskName} has been successfully created/updated", "Scheduling Task", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        done = true;
                    }
                    catch (Exception ex)
                    {
                        done = false;
                        MessageBox.Show(ex.Message, "Error Creating Scheduling Task", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

            }
            catch (Exception ex)
            {
                done = false;
                MessageBox.Show(ex.Message, "Error Creating Scheduling Task", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return done;
        }
    }
}
