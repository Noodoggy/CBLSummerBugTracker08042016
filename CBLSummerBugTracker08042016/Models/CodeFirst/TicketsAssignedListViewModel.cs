using CBLSummerBugTracker08042016.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBLSummerBugTracker08042016.Models
{
    public class TicketsAssignedListViewModel
    {

        public string ticketTitle{ get; set; }
        public int Id { get; set; }
        public MultiSelectList tickets { get; set; }
        public string[] selected { get; set; }

        public virtual Ticket ticket { get; set; }
    }
}