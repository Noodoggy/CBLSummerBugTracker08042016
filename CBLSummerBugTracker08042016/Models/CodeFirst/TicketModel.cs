using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public class Ticket
    {

        public Ticket()
        {
            this.TicketComment = new HashSet<TicketComment>();//to access all ticket comments by ticket using LINQ
            this.TicketAttachment = new HashSet<TicketAttachment>();//to access all ticket attachments by ticket using LINQ
            this.TicketHistory = new HashSet<TicketHistory>();//to access all ticket histories by ticket using LINQ
            this.TicketNotification = new HashSet<TicketNotification>();//to access all ticket notifications by ticket using LINQ
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public int ProjectId { get; set; }
        public int TicketTypeId { get; set; }
        public int TicketPriorityId { get; set; }
        public int TicketStatusId { get; set; }
        public string OwnerUserId { get; set; }
        public string AssignedToUserId { get; set; }
        
        //foreign keys
        public virtual ApplicationUser OwnerUser { get; set; }
        public virtual ApplicationUser AssignedToUser { get; set; }
        public virtual Project Project { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public virtual TicketType TicketType { get; set; }
        
        //navigation property
        public virtual ICollection<TicketComment> TicketComment { get; set; }
        public virtual ICollection<TicketAttachment> TicketAttachment { get; set; }
        public virtual ICollection<TicketHistory> TicketHistory { get; set; }
        public virtual ICollection<TicketNotification> TicketNotification { get; set; }
    }
}