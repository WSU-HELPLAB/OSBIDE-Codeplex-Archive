using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSBIDE.Library.Models;
using OSBIDE.Controls.Models;
using OSBIDE.Library.CSV;
using System.IO;

namespace OSBIDE.Analytics.Terminal
{
    public class SessionMetricsView
    {
        private const string DB_HOST_KEY = "DB_HOST_KEY";
        private const string DB_USER_KEY = "DB_USER_KEY";
        private const string DB_NAME_KEY = "DB_NAME_KEY";
        private const string CONNECTION_STRING_KEY = "CONNECTION_STRING_KEY";
        private OsbideContext _db;

        public SessionMetricsView()
        {
            _db = new OsbideContext(ConfigurationManager.ConnectionStrings["OsbideDbContext"].ToString());
        }

        #region set app settings
        private void GetDbUser()
        {
            string userName = "";
            while (string.IsNullOrWhiteSpace(userName) == true)
            {
                Console.Write("Enter DB User Name: ");
                userName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(userName) == true)
                {
                    Console.Write("Invalid input.\n");
                }
            }
            ConfigurationManager.AppSettings.Set(DB_USER_KEY, userName);
        }
        private void GetDbHost()
        {
            string hostName = "";
            while (string.IsNullOrWhiteSpace(hostName) == true)
            {
                Console.Write("Enter DB Host Name: ");
                hostName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(hostName) == true)
                {
                    Console.Write("Invalid input.\n");
                }
            }
            ConfigurationManager.AppSettings.Set(DB_HOST_KEY, hostName);
        }
        private void GetDbName()
        {
            string name = "";
            while (string.IsNullOrWhiteSpace(name) == true)
            {
                Console.Write("Enter DB Name: ");
                name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name) == true)
                {
                    Console.Write("Invalid input.\n");
                }
            }
            ConfigurationManager.AppSettings.Set(DB_NAME_KEY, name);
        }

        private void GetDbCredentials(bool checkForExisting = true)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(DB_HOST_KEY) == false || checkForExisting == false)
            {
                GetDbHost();
            }
            if (ConfigurationManager.AppSettings.AllKeys.Contains(DB_USER_KEY) == false || checkForExisting == false)
            {
                GetDbUser();
            }
            if (ConfigurationManager.AppSettings.AllKeys.Contains(DB_NAME_KEY) == false || checkForExisting == false)
            {
                GetDbName();
            }
        }
        #endregion

        private void SessionToCsv(Dictionary<int, List<OsbideActivity>> sessions, string outfileName)
        {
            CsvWriter csvWriter = new CsvWriter();

            //build CSV file
            csvWriter.AddToCurrentLine("User Name");
            csvWriter.AddToCurrentLine("Email");
            csvWriter.AddToCurrentLine("Start Date");
            csvWriter.AddToCurrentLine("Start Time");
            csvWriter.AddToCurrentLine("End Date");
            csvWriter.AddToCurrentLine("End Time");
            csvWriter.CreateNewRow();
            foreach (List<OsbideActivity> userSessions in sessions.Values)
            {
                foreach (OsbideActivity session in userSessions)
                {
                    csvWriter.AddToCurrentLine(session.User.FirstAndLastName);
                    csvWriter.AddToCurrentLine(session.User.Email);
                    csvWriter.AddToCurrentLine(session.StartDate.ToString("yyyy-MM-dd"));
                    csvWriter.AddToCurrentLine(session.StartDate.ToString("HH:mm:ss"));
                    csvWriter.AddToCurrentLine(session.EndDate.ToString("yyyy-MM-dd"));
                    csvWriter.AddToCurrentLine(session.EndDate.ToString("HH:mm:ss"));
                    csvWriter.CreateNewRow();
                }
            }

            StreamWriter writer = new StreamWriter(outfileName);
            writer.Write(csvWriter.ToString());
            writer.Close();
        }

        public void Run()
        {
            SessionMetrics metrics = new SessionMetrics(_db);
            metrics.ActivityFeedSessionTimeout = new TimeSpan(0, 10, 0);

            //First, get IDE usage
            Console.WriteLine("Calculating IDE usage...");
            Dictionary<int, List<OsbideActivity>> ideActivity = metrics.GetIdeSessionInfo();
            //SessionToCsv(ideSessions, "IdeSessionMetrics.csv");

            //Now that we have IDE usage, we can begin to calculate feed usage as a % of time
            Console.WriteLine("Building Session Report...");
            Dictionary<int, List<OsbideSession>> osbideSessions = new Dictionary<int, List<OsbideSession>>();
            foreach (KeyValuePair<int, List<OsbideActivity>> kvp in ideActivity)
            {
                osbideSessions.Add(kvp.Key, new List<OsbideSession>());
                foreach (OsbideActivity record in kvp.Value)
                {
                    OsbideSession currentSession = new OsbideSession();
                    currentSession.SessionStart = record.StartDate;
                    currentSession.SessionEnd = record.EndDate;
                    currentSession.User = record.User;

                    currentSession.ActivityLogs = 
                                    (_db.ActionRequestLogs
                                    .Include("Creator")
                                    .Where(a => a.AccessDate >= record.StartDate)
                                    .Where(a => a.AccessDate <= record.EndDate)
                                    .Where(a => a.CreatorId == kvp.Key)
                                    .OrderBy(a => a.AccessDate)).ToList();
                    osbideSessions[kvp.Key].Add(currentSession);
                }
            }

            //Output to CSV file
            Console.WriteLine("Writing to CSV...");

            CsvWriter csvWriter = new CsvWriter();
            csvWriter.AddToCurrentLine("User Name");
            csvWriter.AddToCurrentLine("Email");
            csvWriter.AddToCurrentLine("Session Start");
            csvWriter.AddToCurrentLine("Session End");
            csvWriter.AddToCurrentLine("Session Duration");
            csvWriter.AddToCurrentLine("Feed Opened (seconds)");
            csvWriter.AddToCurrentLine("Feed Opened (%)");
            csvWriter.AddToCurrentLine("# Details");
            csvWriter.AddToCurrentLine("# Build Diff");
            csvWriter.AddToCurrentLine("# Chat");
            csvWriter.AddToCurrentLine("# Activity Feed");
            csvWriter.CreateNewRow();
            foreach (List<OsbideSession> userSessions in osbideSessions.Values)
            {
                foreach (OsbideSession session in userSessions)
                {
                    csvWriter.AddToCurrentLine(session.User.FirstAndLastName);
                    csvWriter.AddToCurrentLine(session.User.Email);
                    csvWriter.AddToCurrentLine(session.SessionStart.ToString("yyyy-MM-dd HH:mm:ss"));
                    csvWriter.AddToCurrentLine(session.SessionEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                    csvWriter.AddToCurrentLine(session.SessionDuration.TotalSeconds.ToString());
                    csvWriter.AddToCurrentLine(session.ActivityFeedTime.TotalSeconds.ToString());
                    csvWriter.AddToCurrentLine(((session.ActivityFeedTime.TotalSeconds / session.SessionDuration.TotalSeconds) * 100).ToString());
                    csvWriter.AddToCurrentLine(session.NumberOfDetailsViews.ToString());
                    csvWriter.AddToCurrentLine(session.NumberOfBuildViews.ToString());
                    csvWriter.AddToCurrentLine(session.NumberOfChatViews.ToString());
                    csvWriter.AddToCurrentLine(session.NumberOfFeedViews.ToString());
                    csvWriter.CreateNewRow();
                }
            }

            StreamWriter writer = new StreamWriter("output.csv");
            writer.Write(csvWriter.ToString());
            writer.Close();

            //activity feed usage
            //Console.WriteLine("Calculating activity feed usage...");
            //Dictionary<int, List<OsbideActivity>> feedSessions = metrics.GetActivityFeedSessionInfo();
            //SessionToCsv(ideSessions, "ActivityFeedMetrics.csv");

            Console.WriteLine("Done.");
        }
    }
}
