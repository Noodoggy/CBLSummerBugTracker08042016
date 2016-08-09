using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using CBLSummerBugTracker08042016.Models.CodeFirst;

namespace CBLSummerBugTracker08042016.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        
        //possible add-on later for two-factor authorization
        //public string PhoneNumber { get; set; }

        public ApplicationUser()
        {
            this.TicketComment = new HashSet<TicketComment>();                              //to access all comments by user
            this.TicketAttachment = new HashSet<TicketAttachment>();                        //to access all attachments by user
            this.TicketHistory = new HashSet<TicketHistory>();                              //to access all ticket history by user
            this.Project = new HashSet<Project>();                                          //to access all projects by user
            
        }

        public virtual ICollection<Project> Project { get; set; }                               //navigation properties
        public virtual ICollection<TicketComment> TicketComment { get; set; }
        public virtual ICollection<TicketAttachment> TicketAttachment { get; set; }
        public virtual ICollection<TicketHistory> TicketHistory { get; set; }
        public virtual ICollection<TicketNotification> TicketNotification { get; set; }
        
        
        
        


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this,
                DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }
        public DbSet<TicketNotification> TicketNotifications { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<TicketPriority> TicketPriorities { get; set; }
        public DbSet<Project> Projects { get; set; }

        //public System.Data.Entity.DbSet<CBLSummerBugTracker08042016.Models.ApplicationUser> ApplicationUsers { get; set; }
    }

}