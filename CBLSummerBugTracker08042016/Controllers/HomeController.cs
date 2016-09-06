using CBLSummerBugTracker08042016.Models;
using CBLSummerBugTracker08042016.Models.CodeFirst;
using CBLSummerBugTracker08042016.Models.CodeFirst.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBLSummerBugTracker08042016.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        protected ApplicationDbContext ApplicationDbContext { get; set; }
        protected UserManager<ApplicationUser> UserManager { get; set; }

        public ActionResult Index()
        {
            return View();
        }


        [Authorize]
        public ActionResult Dashboard()
        {
            UserRolesHelper helper = new UserRolesHelper();
            DashboardViewModel dash = new DashboardViewModel();
            var currentUserId = User.Identity.GetUserId();                   //find current user by id
            var currentUser = db.Users.Find(User.Identity.GetUserId());
            ViewBag.UserId = User.Identity.GetUserId();
            var pendingUsers = new List<ApplicationUser>();
            foreach (var item in db.Users)
            {
                if ((!helper.IsUserInRole(item.Id, "Admin")) && (!helper.IsUserInRole(item.Id, "Project Manager")) && (!helper.IsUserInRole(item.Id, "Developer")) && (!helper.IsUserInRole(item.Id, "Submitter")))
                {
                    pendingUsers.Add(item);
                }
            }


            //Admin dashboard
            //radar graph
            dash.allProjects = db.Projects.Where(s => s.Name != null).ToList();
            //pie graph
            dash.allTickets = db.Tickets.Where(s => s.TicketStatus != null).ToList();
            //Admin 
            //User Activity
            dash.pendingUsers = pendingUsers.Take(3).ToList();

            //Admin and PM
            //Ticket activity
            dash.latestTicketHistory = db.TicketHistories.OrderByDescending(u => u.Changed).Take(3).ToList();
            //Project activity
            dash.latestProjectHistory = db.Tickets.OrderByDescending(a => a.Updated).Where(s => s.ProjectId != null && s.Project.Name != null).ToList();
            var five = db.Projects.ToList();
            var fiveProjects = new List<Project>();
            var listOfFiveProjects = new List<int>();
            foreach (var item in five)
            {
                if (item.Ticket.Count != 0)
                fiveProjects.Add(item);
            }
            foreach(var item in fiveProjects.Take(5))
            {
                var ticketCount = item.Ticket.Count;
                var resolvedTicket = (item.Ticket.Where(s => s.TicketStatus.Name == "Resolved")).Count();
                var percent = (200 * resolvedTicket + ticketCount) / (ticketCount * 2);
                
                listOfFiveProjects.Add(percent);
            }
            dash.fiveProjects = listOfFiveProjects;
            //Project Manager dashboard
            //newest 5 projects and # of closed tickets versus total tickets


            //tickets created over the past 3 weeks line chart
            dash.threeWeeksTickets = new int[4];
            var j = 21;
            for (var i = 0; i<=3; i++)
            {
                
                dash.threeWeeksTickets[i] = dash.allTickets.Where(u => DateTime.UtcNow - u.Created > TimeSpan.FromDays(j)).Count();
                j = j - 7;
            }

            var fiveTickets = db.Tickets.Where(s => s.TicketStatus.Name == "Open").ToList();
            dash.fiveWeeksOpenTickets = new int[5];
            j = 28;
            for (var i = 0; i<=4; i++)
            {
                dash.fiveWeeksOpenTickets[i] = fiveTickets.Where(u => DateTime.UtcNow - u.Created > TimeSpan.FromDays(j)).Where(s => s.TicketStatus.Name == "Open").Count();
                j = j - 7;
            }



            //Developer and submitter dashboard
            //ticket activity
            dash.topThreeTickets = db.Tickets.OrderByDescending(u => u.Updated).Where(u => u.OwnerUserId == currentUserId || u.AssignedToUserId == currentUserId).Take(3).ToList();



            

            return View(dash);
        }



 
    }
}