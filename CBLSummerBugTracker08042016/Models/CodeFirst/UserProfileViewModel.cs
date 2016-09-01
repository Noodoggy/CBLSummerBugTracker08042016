using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public class UserProfileViewModel
    {
        public ApplicationUser Id { get; set; }
        public IList<Ticket> MyTickets { get; set; }
        public IList<Ticket> AssignedTickets { get; set; }
        public IList<Ticket> ProjectTickets { get; set; }
    }
}