﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public class TicketStatus
    {
        public TicketStatus()//access to tickets by status using LINQ
        {
            Ticket = new HashSet<Ticket>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        
        //navigation property
        public virtual ICollection<Ticket> Ticket { get; set; }
    }
}