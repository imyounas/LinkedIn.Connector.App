using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLogger
{
    public static class Logger
    {
        public delegate void DebugLogEventHandler(string message);
        public static event DebugLogEventHandler DebugMessaged;

        public delegate void ErrorLogEventHandler(Exception ex, string message);
        public static event ErrorLogEventHandler ErrorMessaged;

        public static void Debug(string m)
        {
            // Log.Debug(m);
            OnDebugMessaged(m);
        }

        public static void Error(Exception ex, string m)
        {
            //Log.Error(ex, m);
            OnErrorMessaged(ex, m);
        }

        public static void OnDebugMessaged(string m)
        {
            if (DebugMessaged != null)
            {
                DebugMessaged(m);
            }
        }

        public static void OnErrorMessaged(Exception ex, string m)
        {
            if (ErrorMessaged != null)
            {
                ErrorMessaged(ex, m);
            }
        }
    }
}
