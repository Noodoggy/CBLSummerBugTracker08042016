using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public class Project
    {
        public Project()
        {
            this.User = new HashSet<ApplicationUser>();                 //to access all users in project
            this.Ticket = new HashSet<Ticket>();                    //to access all tickets in project
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual ICollection<Ticket> Ticket { get; set; }
        public virtual ICollection<ApplicationUser> User { get; set; }
    }
}