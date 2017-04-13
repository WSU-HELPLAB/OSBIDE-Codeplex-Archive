using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Controls.Models
{
    public class OsbideSession
    {
        public OsbideUser User { get; set; }
        public DateTime SessionStart { get; set; }
        public DateTime SessionEnd { get; set; }
        public TimeSpan SessionDuration
        {
            get
            {
                TimeSpan span = SessionEnd - SessionStart;
                return span;
            }
        }
        public TimeSpan ActivityFeedTime
        {
            get
            {
                //set session info if activity logs exist
                if (ActivityLogs.Count > 0)
                {
                    ActionRequestLog first = ActivityLogs.First();
                    ActionRequestLog last = ActivityLogs.Last();
                    return last.AccessDate - first.AccessDate;
                }
                return new TimeSpan();
            }
        }
        public List<ActionRequestLog> ActivityLogs { get; set; }
        public int NumberOfDetailsViews
        {
            get
            {
                var query = (from log in ActivityLogs
                             where log.ControllerName.ToLower() == "details"
                             select log).Count();
                return query;
            }
        }

        public int NumberOfBuildViews
        {
            get
            {
                var query = (from log in ActivityLogs
                             where log.ControllerName.ToLower() == "buildevent"
                             select log).Count();
                return query;
            }
        }

        public int NumberOfChatViews
        {
            get
            {
                var query = (from log in ActivityLogs
                             where log.ControllerName.ToLower() == "chat"
                             select log).Count();
                return query;
            }
        }

        public int NumberOfFeedViews
        {
            get
            {
                var query = (from log in ActivityLogs
                             where log.ControllerName.ToLower() == "feed"
                             select log).Count();
                return query;
            }
        }

        public int NumberOfProfileViews
        {
            get
            {
                var query = (from log in ActivityLogs
                             where log.ControllerName.ToLower() == "profile"
                             select log).Count();
                return query;
            }
        }
    }
}
