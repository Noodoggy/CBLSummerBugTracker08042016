using System;

using System.Linq;

using System.Web.Mvc;

using System.Data.Entity;
using System.Net;
using CBLSummerBugTracker08042016.Models;
using CBLSummerBugTracker08042016.Models.CodeFirst;
using Microsoft.AspNet.Identity;
using System.Configuration;
using SendGrid;
using CBLSummerBugTracker08042016.Models.CodeFirst.Helpers;

namespace CBLSummerBugTracker08042016.Controllers
{


    public class TicketCommentsController : Controller
        {
            private ApplicationDbContext db = new ApplicationDbContext();

            // GET: TicketComments
            public ActionResult Index()
            {
                var ticketComment = db.TicketComments.Include(t => t.Ticket).Include(t => t.User);
                return View(ticketComment.ToList());
            }

            // GET: TicketComments/Details/5
            public ActionResult Details(int? id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                TicketComment ticketComments = db.TicketComments.Find(id);
                if (ticketComments == null)
                {
                    return HttpNotFound();
                }
                return View(ticketComments);
            }

            //// GET: TicketComments/Create
            //public ActionResult Create()
            //{
            //    var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            //    var currentUser = db.Users.Find(currentUserId);

            //    ViewBag.TicketId = new SelectList(db.Ticket, "Id", "Title");
            //    ViewBag.UserId = currentUser;
            //    return View();
            //}

            // POST: TicketComments/Create
            // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
            // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
            [HttpPost]
            [ValidateAntiForgeryToken]

            public ActionResult Create([Bind(Include = "Id,Comment,Created,TicketId,UserId")] TicketComment ticketComments)
            {

            if (ModelState.IsValid)
            {
                UserRolesHelper helper = new UserRolesHelper();
                UserProjectsHelper helper2 = new UserProjectsHelper();
                var transportWeb = new Web(ConfigurationManager.AppSettings["SendGridAPIKey"]);
                var notification = new IdentityMessage();
                var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                var currentUser = db.Users.Find(currentUserId);
                var ticket = db.Tickets.Find(ticketComments.TicketId);
                if ((ticket.OwnerUserId == currentUserId)                   //check for owneruser
               || (ticket.AssignedToUserId == currentUserId)               //check for assigned user
               || (helper2.IsUserOnProject(currentUserId, ticket.ProjectId))           //check for user assigned to project, thus ticket
               || (helper.IsUserInRole(User.Identity.GetUserId(), "Admin")))           //check for admin
                {
                    ViewBag.UserId = currentUser;
                    ticketComments.UserId = currentUserId;
                    ticketComments.Created = DateTime.Now.ToLocalTime();
                    ticketComments.User = currentUser;
                    db.TicketComments.Add(ticketComments);

                    var changed = DateTime.Now;
                    TicketHistory thcom = new TicketHistory
                    {
                        TicketId = ticketComments.TicketId,
                        Property = "Comments",
                        OldValue = "",
                        NewValue = currentUser.DisplayName + " has added a new comment.",
                        Changed = changed,
                        UserId = currentUserId,

                        User = currentUser,                            //this is for use in notification message
                    };

                    notification.Body = currentUser.DisplayName + " has added a new comment.";
                    SendGridMessage mymessage = new SendGridMessage();
                    mymessage.Html = notification.Body;
                    transportWeb.DeliverAsync(mymessage);
                    db.TicketHistories.Add(thcom);




                    db.SaveChanges();
                    return RedirectToAction("Details", "Tickets", new { id = ticketComments.TicketId });
                }

            }
                ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComments.TicketId);

                return View(ticketComments);
            }

            // GET: TicketComments/Edit/5
            public ActionResult Edit(int? id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                TicketComment ticketComments = db.TicketComments.Find(id);
                if (ticketComments == null)
                {
                    return HttpNotFound();
                }
                ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComments.TicketId);

                return View(ticketComments);
            }

            // POST: TicketComments/Edit/5
            // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
            // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Edit([Bind(Include = "Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
            {
                if (ModelState.IsValid)
                {

                var oldticketComment = db.TicketComments.AsNoTracking().FirstOrDefault(t => t.Id == ticketComment.Id);
                var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                var currentUser = db.Users.Find(currentUserId);
                var notification = new IdentityMessage();
                var changed = DateTime.Now;
                var transportWeb = new Web(ConfigurationManager.AppSettings["SendGridAPIKey"]);
                TicketHistory thcom = new TicketHistory
                {
                    TicketId = ticketComment.TicketId,
                    Property = "Comments",
                    OldValue = oldticketComment.Comment,
                    NewValue = ticketComment.Comment,
                    Changed = changed,
                    UserId = currentUserId,

                    User = currentUser,                            //this is for use in notification message
                };

                notification.Body = currentUser.DisplayName + " has edited a comment.";
                SendGridMessage mymessage = new SendGridMessage();
                mymessage.Html = notification.Body;
                transportWeb.DeliverAsync(mymessage);
                db.TicketHistories.Add(thcom);







                db.Entry(ticketComment).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }



                ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComment.TicketId);

                return View(ticketComment);
            }

            // GET: TicketComments/Delete/5
            public ActionResult Delete(int? id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                TicketComment ticketComments = db.TicketComments.Find(id);
                if (ticketComments == null)
                {
                    return HttpNotFound();
                }
                return View(ticketComments);
            }

            // POST: TicketComments/Delete/5
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public ActionResult DeleteConfirmed(int id)
            {
                TicketComment ticketComments = db.TicketComments.Find(id);
                db.TicketComments.Remove(ticketComments);
                db.SaveChanges();
                return RedirectToAction("Index");
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