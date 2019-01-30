using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXEBoot
{
    public class FoxEventLog
    {
        const string Title = "Fox PXEServer";
        public static void WriteEventLog(string Message, EventLogEntryType type)
        {
            try
            {
                if (EventLog.SourceExists(Title) == true)
                {
                    EventLog ev = new EventLog();
                    ev.Source = Title;
                    ev.WriteEntry(Message, type);
                }
                else
                {
                    EventLog ev = new EventLog();
                    ev.Log = "Application";
                    ev.WriteEntry(Message, type);
                }
            }
            catch
            {

            }
        }

        public static void RegisterEventLog()
        {
            if (EventLog.SourceExists(Title) == false)
            {
                EventLog.CreateEventSource(Title, "Application");
                Console.WriteLine(Title + " Created");
            }
            else
            {
                Console.WriteLine(Title + " Exists");
            }
        }
    }
}
