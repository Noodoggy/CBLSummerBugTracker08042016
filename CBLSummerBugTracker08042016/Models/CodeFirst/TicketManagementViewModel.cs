using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public class TicketManagementViewModel
    {
        public Ticket CreateTicket { get; set; }
        public IList<Ticket> TicketList { get; set; }
    }
}