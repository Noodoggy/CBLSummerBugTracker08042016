using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public class DashboardViewModel
    {
            public List<Project> allProjects { get; set; }
            public List<Ticket> allTickets { get; set; }
            public List<ApplicationUser> pendingUsers { get; set; }
            public List<TicketHistory> latestTicketHistory { get; set; }
            public List<Ticket> latestProjectHistory { get; set; }
            public List<int> fiveProjects { get; set; }
            public int[] threeWeeksTickets { get; set; }
            public int[] fiveWeeksOpenTickets { get; set; }
            public List<Ticket> topThreeTickets { get; set; }
    }
}