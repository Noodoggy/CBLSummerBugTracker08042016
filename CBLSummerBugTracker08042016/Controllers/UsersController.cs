using CBLSummerBugTracker08042016.Models;
using CBLSummerBugTracker08042016.Models.CodeFirst;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBLSummerBugTracker08042016.Controllers
{
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //public ActionResult UserProfile(string name)
        //{
        //    var model = new UserProfileViewModel();
        //    model.Id = db.Users.Find(name);
        //    return View(model);
        //}
        // GET: Users
        public ActionResult UserProfile(string id)
        {
            if (id == null)
            {
                id = User.Identity.GetUserId();
            }
            var model = new UserProfileViewModel();
            model.Id = db.Users.Find(id);
            model.MyTickets = db.Tickets.Where(u => u.OwnerUserId == id).ToList();
            model.AssignedTickets = db.Tickets.Where(u => u.AssignedToUserId == id).ToList();
            model.ProjectTickets = model.Id.Project.SelectMany(p => p.Ticket).ToList();
            ViewBag.UserId = model.Id;
            return View(model);

        }

        public ActionResult MyTickets(IList<Ticket> model)
        {


            var currentUserId = db.Users.Find();
            ViewBag.UserId = currentUserId;
            return View(model);
        }



        public ActionResult AssignedTickets(string id)
        {
            if (id == null)
            {
                id = User.Identity.GetUserId();
            }
            var currentUserId = db.Users.Find(id);
            var ticket = db.Tickets.Where(u => u.AssignedToUserId == id);
            ViewBag.UserId = currentUserId;
            return View(ticket.ToList());
        }

        public ActionResult MyProjectTickets(/*[Bind(Include = "ProjectName, Id, tickets")] TicketViewModel model*/string id)
        {
            if (id == null)
            {
                id = User.Identity.GetUserId();
            }
            var currentUserId = db.Users.Find(id);

            var ticket = db.Users.Find(currentUserId).Project.SelectMany(p => p.Ticket);
            ViewBag.UserId = currentUserId;
            return View(ticket.ToList());
        }
    }
}