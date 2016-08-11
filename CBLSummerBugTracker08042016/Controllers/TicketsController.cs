using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Configuration;
using SendGrid;
using CBLSummerBugTracker08042016.Models;
using CBLSummerBugTracker08042016.Models.CodeFirst;
using CBLSummerBugTracker08042016.Models.CodeFirst.Helpers;

namespace CBLSummerBugTracker08042016.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        protected ApplicationDbContext ApplicationDbContext { get; set; }           
        protected UserManager<ApplicationUser> UserManager { get; set; }            



        // GET: Tickets
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var ticket = db.Tickets.Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View(ticket.ToList());
        }

        // GET: Tickets/Details/5
        [Authorize(Roles = "Submitter, Admin")]
        public ActionResult Details(int? id)                //only able to see ticket details if assigned to ticket or ticket owner
        {                                                   //modify to include myproject tickets without edit ability
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);                                    //find ticket object by id
            if (ticket == null)
            {
                return HttpNotFound();
            }
            UserRolesHelper helper = new UserRolesHelper();
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();                   //find current user by id
            var currentUser = db.Users.Find(currentUserId);                                                //find user id 
            var ticketassigned = db.Tickets.Where(u => u.AssignedToUserId == currentUserId);             //list of tickets assigned to user
            if ((ticket.OwnerUserId == currentUserId) || (ticket.AssignedToUserId == currentUserId) || helper.IsUserInRole(currentUserId, "Admin"))
            {
                return View(ticket);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }


        // GET: Tickets/Create
        [Authorize(Roles = "Submitter, Admin")]
        public ActionResult Create()
        {

            UserRolesHelper helper = new UserRolesHelper();
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var currentUser = db.Users.Find(currentUserId);
            if (helper.IsUserInRole(User.Identity.GetUserId(), "Admin"))
            {
                ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            }
            else
            {
                ViewBag.ProjectId = new SelectList(currentUser.Project, "Id", "Name");
            }
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }



        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Submitter, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Created,Updated,Title,Description,OwnerUserId,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId")] Ticket tickets)
        {
            if (ModelState.IsValid)
            {
                db.Tickets.Add(tickets);                                                        //add data to tickets table
                tickets.OwnerUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();         //set owner to current user id
                tickets.OwnerUser = db.Users.Find(tickets.OwnerUserId);                     //set owner to current user (for display purposes)
                tickets.Created = DateTimeOffset.Now;
                db.SaveChanges();


            }

            ViewBag.ProjectId = new SelectList(tickets.OwnerUser.Project, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");

            return RedirectToAction("Index");


        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            Ticket ticket = db.Tickets.Find(id);
            UserRolesHelper helper = new UserRolesHelper();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (ticket == null)
            {
                return HttpNotFound();
            }

            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();  //
            var currentUser = db.Users.Find(currentUserId);
            var project = (currentUser.Project);
            UserProjectsHelper helper2 = new UserProjectsHelper(db);
            //var ticketowner = tickets.Where(u => u.OwnerUserId == currentUserId);
            var ticketassigned = db.Tickets.Where(u => u.AssignedToUserId == currentUserId);
            if ((ticket.OwnerUserId == currentUserId) || (ticket.AssignedToUserId == currentUserId) || (helper2.IsUserOnProject(currentUserId, ticket.ProjectId)) || (helper.IsUserInRole(User.Identity.GetUserId(), "Admin")))
            {

                if (helper.IsUserInRole(User.Identity.GetUserId(), "Admin") || helper.IsUserInRole(User.Identity.GetUserId(), "Project Manager"))
                {

                    ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "Username", ticket.AssignedToUserId);
                }
                else
                {
                    ViewBag.AssignedToUserId = new SelectList(ticket.AssignedToUserId);
                };

                if (helper.IsUserInRole(User.Identity.GetUserId(), "Admin"))
                {
                    ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
                }
                else
                {

                    ViewBag.ProjectId = new SelectList(currentUser.Project, "Id", "Name");
                }
                ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
                ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
                ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
                db.SaveChanges();
                return View(ticket);
            }
            else
            {
                return HttpNotFound();
            }
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]

        public ActionResult Edit([Bind(Include = "Id,Created,Updated,Title,Description,OwnerUserId,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,AssignedToUserId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var user = db.Users.Find(userId);
                var changed = DateTime.Now;
                var editId = Guid.NewGuid().ToString();
                var oldticket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);
                var apiKey = ConfigurationManager.AppSettings["SendGridAPIKey"];
                var from = ConfigurationManager.AppSettings["ContactEmail"];
                var notification = new IdentityMessage();
                if (oldticket.AssignedToUserId != null)
                {

                    notification.Destination = db.Users.Find(oldticket.AssignedToUserId).UserName;          //if already assigned, send notification
                    notification.Subject = "A change has been made to your Assigned Ticket" + oldticket.Title;
                    TicketNotification tn = new TicketNotification
                    {
                        TicketId = ticket.Id,
                        UserId = oldticket.AssignedToUserId
                    };
                    TicketHistory thn = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "Notification",
                        OldValue = "N/A",
                        NewValue = "Sent to " + db.Users.Find(oldticket.AssignedToUserId).UserName,
                        Changed = changed,
                        UserId = userId,

                        User = user,
                    };
                    db.TicketHistories.Add(thn);
                    db.TicketNotifications.Add(tn);
                }
                else
                {

                    notification.Destination = db.Users.Find(ticket.AssignedToUserId).UserName;            //if edited assigned user, send notification
                    notification.Subject = "A change has been made to your Assigned Ticket" + oldticket.Title;
                    TicketNotification tn = new TicketNotification
                    {
                        TicketId = ticket.Id,
                        UserId = ticket.AssignedToUserId
                    };

                    TicketHistory thn = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "Notification",
                        OldValue = "N/A",
                        NewValue = "Sent to " + db.Users.Find(oldticket.AssignedToUserId).UserName,
                        Changed = changed,
                        UserId = userId,

                        User = user,
                    };
                    db.TicketHistories.Add(thn);
                    db.TicketNotifications.Add(tn);
                }


                SendGridMessage mymessage = new SendGridMessage();
                mymessage.AddTo(notification.Destination);
                mymessage.From = new System.Net.Mail.MailAddress(from);
                mymessage.Subject = notification.Subject;

                var transportWeb = new Web(ConfigurationManager.AppSettings["SendGridAPIKey"]);

                TicketHistory thf = new TicketHistory
                foreach (var item in thf)
                {
                    TicketHistory thf = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "Notification",
                        OldValue = "N/A",
                        NewValue = "Sent to " + db.Users.Find(oldticket.AssignedToUserId).UserName,
                        Changed = changed,
                        UserId = userId,

                        User = user,
                    };
                    db.TicketHistories.Add(thn);
                }

                if (oldticket?.Title != ticket.Title)
                {
                    TicketHistory th1 = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "Title",
                        OldValue = oldticket?.Title,
                        NewValue = ticket.Title,
                        Changed = changed,
                        UserId = userId,

                        User = user,                            //this is for use in notification message
                    };

                    notification.Body = "The " + th1.Property + " has been changed from " + th1.OldValue + " to " + th1.NewValue + " by " + th1.User.DisplayName + ".";

                    mymessage.Html = notification.Body;
                    transportWeb.DeliverAsync(mymessage);
                    db.TicketHistories.Add(th1);
                }

                if (oldticket?.Description != ticket.Description)
                {
                    TicketHistory th2 = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "Description",
                        OldValue = oldticket?.Description,
                        NewValue = ticket.Description,
                        Changed = changed,
                        UserId = userId,

                        User = user
                    };
                    notification.Body = "The " + th2.Property + " has been changed from " + th2.OldValue + " to " + th2.NewValue + " by " + th2.User.DisplayName + ".";

                    mymessage.Html = notification.Body;
                    transportWeb.DeliverAsync(mymessage);
                    db.TicketHistories.Add(th2);
                }

                if (oldticket?.ProjectId != ticket.ProjectId)
                {
                    TicketHistory th3 = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "Updated",
                        OldValue = db.Projects.Find(oldticket?.ProjectId).Name,
                        NewValue = db.Projects.Find(ticket.ProjectId).Name,
                        Changed = changed,
                        UserId = userId,

                        User = user
                    };
                    notification.Body = "The " + th3.Property + " has been changed from " + th3.OldValue + " to " + th3.NewValue + " by " + th3.User.DisplayName + ".";

                    mymessage.Html = notification.Body;
                    transportWeb.DeliverAsync(mymessage);
                    db.TicketHistories.Add(th3);
                }

                if (oldticket?.TicketTypeId != ticket.TicketTypeId)
                {
                    TicketHistory th4 = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "TicketTypeId",
                        OldValue = db.TicketTypes.Find(oldticket?.TicketTypeId).Name,
                        NewValue = db.TicketTypes.Find(ticket.TicketTypeId).Name,
                        Changed = changed,
                        UserId = userId,

                        User = user
                    };
                    notification.Body = "The " + th4.Property + " has been changed from " + th4.OldValue + " to " + th4.NewValue + " by " + th4.User.DisplayName + ".";

                    mymessage.Html = notification.Body;
                    transportWeb.DeliverAsync(mymessage);
                    db.TicketHistories.Add(th4);
                }

                if (oldticket?.TicketPriorityId != ticket.TicketPriorityId)
                {
                    TicketHistory th5 = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "TicketPriorityId",
                        OldValue = db.TicketPriorities.Find(oldticket?.TicketPriorityId).Name,
                        NewValue = db.TicketPriorities.Find(ticket.TicketPriorityId).Name,
                        Changed = changed,
                        UserId = userId,

                        User = user
                    };
                    notification.Body = "The " + th5.Property + " has been changed from " + th5.OldValue + " to " + th5.NewValue + " by " + th5.User.DisplayName + ".";

                    mymessage.Html = notification.Body;
                    transportWeb.DeliverAsync(mymessage);
                    db.TicketHistories.Add(th5);
                }

                if (oldticket?.TicketStatusId != ticket.TicketStatusId)
                {
                    TicketHistory th6 = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "TicketStatusId",
                        OldValue = db.TicketStatuses.Find(oldticket?.TicketStatusId).Name,
                        NewValue = db.TicketStatuses.Find(ticket.TicketStatusId).Name,
                        Changed = changed,
                        UserId = userId,

                        User = user
                    };
                    notification.Body = "The " + th6.Property + " has been changed from " + th6.OldValue + " to " + th6.NewValue + " by " + th6.User.DisplayName + ".";

                    mymessage.Html = notification.Body;
                    transportWeb.DeliverAsync(mymessage);
                    db.TicketHistories.Add(th6);
                }
                if ((oldticket?.AssignedToUserId == null) || (oldticket?.AssignedToUserId != ticket.AssignedToUserId))


                {
                    TicketHistory th7 = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "AssignedToUserId",
                        OldValue = "not assigned",
                        NewValue = db.Users.Find(ticket.AssignedToUserId).DisplayName,
                        Changed = changed,
                        UserId = userId,

                        User = user,

                    };

                    var assignNotification = new IdentityMessage
                    {
                        Destination = db.Users.Find(ticket.AssignedToUserId).UserName,
                        Subject = "You have been assigned to a ticket.",
                        Body = "You have been assigned to the following ticket: Id" + ticket.Id + ":  " + ticket.Title + "."

                    };
                    SendGridMessage mymessage2 = new SendGridMessage();
                    mymessage2.AddTo(assignNotification.Destination);
                    mymessage2.From = new System.Net.Mail.MailAddress(from);
                    mymessage2.Subject = assignNotification.Subject;
                    mymessage2.Html = assignNotification.Body;
                    transportWeb.DeliverAsync(mymessage2);

                    notification.Body = "The " + th7.Property + " has been changed from " + th7.OldValue + " to " + th7.NewValue + " by " + th7.User.DisplayName + ".";

                    transportWeb.DeliverAsync(mymessage);

                    db.TicketHistories.Add(th7);

                }



                db.Entry(ticket).State = EntityState.Modified;                              //orignal statement
                db.Entry(ticket).Property("Title").IsModified = true;                       //added this and and the next line.
                db.Entry(ticket).Property("Created").IsModified = false;

                ticket.Updated = DateTimeOffset.Now;


                ticket.AssignedToUser = db.Users.Find(ticket.AssignedToUserId);
                db.SaveChanges();



            }


            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "Username", ticket.AssignedToUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);


            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket tickets = db.Tickets.Find(id);
            if (tickets == null)
            {
                return HttpNotFound();
            }
            return View(tickets);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket tickets = db.Tickets.Find(id);
            db.Tickets.Remove(tickets);
            db.SaveChanges();
            return RedirectToAction("Index");
        }




        public ActionResult MyTickets()
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();  //helps create list of projects user is assigned to 

            var ticket = db.Tickets.Where(u => u.OwnerUserId == currentUserId);


            return View(ticket.ToList());
        }



        public ActionResult AssignedTickets()
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();  //helps create list of projects user is assigned to 


            var ticket = db.Tickets.Where(u => u.AssignedToUserId == currentUserId);

            return View(ticket.ToList());
        }

        public ActionResult MyProjectTickets([Bind(Include = "ProjectName, Id, tickets")] TicketViewModel model)
        {
            var context = this.db;

            var currentUserId = User.Identity.GetUserId();  //helps create list of projects user is assigned to 
            var currentUser = db.Users.Find(currentUserId);
            var projectTickets = currentUser.Project.SelectMany(p => p.Ticket);
            return View(projectTickets.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}

