using System;

using System.Linq;

using System.Web.Mvc;

using System.Data.Entity;

using System.Net;

using CBLSummerBugTracker08042016.Models;
using Microsoft.AspNet.Identity;
using CBLSummerBugTracker08042016.Models.CodeFirst;

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
                    var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    var currentUser = db.Users.Find(currentUserId);
                    ViewBag.UserId = currentUser;
                    ticketComments.UserId = currentUserId;
                    ticketComments.Created = DateTime.Now.ToLocalTime();
                    ticketComments.User = currentUser;
                    db.TicketComments.Add(ticketComments);
                    db.SaveChanges();
                    return RedirectToAction("Details", "Tickets", new { id = ticketComments.TicketId });
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
            public ActionResult Edit([Bind(Include = "Id,Comment,Created,TicketId,UserId")] TicketComment ticketComments)
            {
                if (ModelState.IsValid)
                {

                    db.Entry(ticketComments).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComments.TicketId);

                return View(ticketComments);
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