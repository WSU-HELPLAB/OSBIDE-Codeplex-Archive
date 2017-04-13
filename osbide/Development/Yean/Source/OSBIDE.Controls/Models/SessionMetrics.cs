using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Controls.Models
{
    public class SessionMetrics
    {
        private OsbideContext _db = null;
        public TimeSpan ActivityFeedSessionTimeout { get; set; }

        public SessionMetrics(OsbideContext db)
        {
            _db = db;
            ActivityFeedSessionTimeout = new TimeSpan(0, 10, 0);
        }

        public Dictionary<int, List<OsbideActivity>> GetActivityFeedSessionInfo()
        {
            return GetActivityFeedSessionInfo(DateTime.MinValue, DateTime.MaxValue);
        }

        public Dictionary<int, List<OsbideActivity>> GetActivityFeedSessionInfo(DateTime startDate, DateTime endDate, int userId = -1)
        {
            //TODO: pull from action request logs to build session report
            Dictionary<int, List<OsbideActivity>> sessions = new Dictionary<int, List<OsbideActivity>>();
            ActionRequestLog action = new ActionRequestLog();

            var logsQuery = _db.ActionRequestLogs
                .Include("Creator")
                .Where(a => a.AccessDate >= startDate)
                .Where(a => a.AccessDate <= endDate)
                .OrderBy(a => a.AccessDate);

            //filter by user id if requested
            if (userId > 0)
            {
                logsQuery.Where(a => a.CreatorId == userId);
            }

            foreach(ActionRequestLog log in logsQuery)
            {
                //add key if it doesn't already exist
                if (sessions.ContainsKey(log.CreatorId) == false)
                {
                    sessions.Add(log.CreatorId, new List<OsbideActivity>());
                    OsbideActivity session = new OsbideActivity()
                    {
                        User = log.Creator,
                        StartDate = log.AccessDate,
                        EndDate = log.AccessDate,
                        Source = "Activity Feed"
                    };
                    sessions[log.CreatorId].Add(session);
                }

                OsbideActivity currentSession = sessions[log.CreatorId].Last();
                
                //if the delta between the log's date and our timeout does not exceed our current ending date, 
                //then update the current session's ending date
                if (log.AccessDate - ActivityFeedSessionTimeout < currentSession.EndDate)
                {
                    currentSession.EndDate = log.AccessDate;
                }
                else
                {
                    //ELSE: the delta was too great.  Create a new session.
                    OsbideActivity session = new OsbideActivity()
                    {
                        User = log.Creator,
                        StartDate = log.AccessDate,
                        EndDate = log.AccessDate,
                        Source = "Activity Feed"
                    };
                    sessions[log.CreatorId].Add(session);
                }
            }
            return sessions;
        }

        public Dictionary<int, List<OsbideActivity>> GetIdeSessionInfo()
        {
            return GetIdeSessionInfo(DateTime.MinValue, DateTime.MaxValue);
        }

        public Dictionary<int, List<OsbideActivity>> GetIdeSessionInfo(DateTime startDate, DateTime endDate)
        {
            //A session is defined as usage that contains no more than "ActivityFeedSessionTimeout" minutes of inactivity
            
            //goal: loop through all event logs:
            //  If current user doesn't have a session, create one with starting and ending times as the current log
            //  If the difference between the last observed log's date and the current log's date is greater than
            //  the timeout grade period, then start a new session.

            //step #1: get all logs within our date range
            Dictionary<int, List<OsbideActivity>> sessions = new Dictionary<int, List<OsbideActivity>>();
            var logsQuery = _db.EventLogs
                .Include("Sender")
                .Where(e => e.DateReceived >= startDate)
                .Where(e => e.DateReceived <= endDate)
                .OrderBy(e => e.Id);
            foreach (EventLog log in logsQuery)
            {
                //add key if it doesn't already exist
                if (sessions.ContainsKey(log.SenderId) == false)
                {
                    sessions.Add(log.SenderId, new List<OsbideActivity>());
                    OsbideActivity session = new OsbideActivity()
                    {
                        User = log.Sender,
                        StartDate = log.DateReceived,
                        EndDate = log.DateReceived,
                        Source = "IDE Activity"
                    };
                    sessions[log.SenderId].Add(session);
                }

                OsbideActivity currentSession = sessions[log.SenderId].Last();
                
                //if the delta between the log's date and our timeout does not exceed our current ending date, 
                //then update the current session's ending date
                if (log.DateReceived - ActivityFeedSessionTimeout < currentSession.EndDate)
                {
                    currentSession.EndDate = log.DateReceived;
                }
                else
                {
                    //ELSE: the delta was too great.  Create a new session.
                    OsbideActivity session = new OsbideActivity()
                    {
                        User = log.Sender,
                        StartDate = log.DateReceived,
                        EndDate = log.DateReceived,
                        Source = "IDE Activity"
                    };
                    sessions[log.SenderId].Add(session);
                }
            }

            return sessions;
        }
    }
}
