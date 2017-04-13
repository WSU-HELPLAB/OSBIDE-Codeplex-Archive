using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal
{
    class Program
    {
        static void Main(string[] args)
        {
            SessionMetricsView metrics = new SessionMetricsView();
            metrics.Run();
        }
    }
}
