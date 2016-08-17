using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using CBLSummerBugTracker08042016.Models.CodeFirst;
using System.IO;
using CBLSummerBugTracker08042016.Models;
using SendGrid;
using System.Configuration;

namespace BugTracker.Controllers
{
    public class TicketAttachmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: TicketAttachments
        public ActionResult Index()
        {
            var ticketAttachment = db.TicketAttachments.Include(t => t.Ticket).Include(t => t.User);
            return View(ticketAttachment.ToList());
        }

        // GET: TicketAttachments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachments = db.TicketAttachments.Find(id);
            if (ticketAttachments == null)
            {
                return HttpNotFound();
            }
            return View(ticketAttachments);
        }

        //// GET: TicketAttachments/Create
        public ActionResult Create()
        {
            
            return View();
        }

        // POST: TicketAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TicketId,FilePath,Description,Created,UserId,FileUrl")] TicketAttachment ticketAttachments, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {

                var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                var currentUser = db.Users.Find(currentUserId);
                ViewBag.UserId = currentUser;
                ticketAttachments.UserId = currentUserId;
                ticketAttachments.Created = DateTime.Now.ToLocalTime();
                ticketAttachments.User = currentUser;
                ticketAttachments.FileUrl = image.ToString();
                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/img/attachments/"), fileName));
                    ticketAttachments.FilePath = "~/img/attachments/" + fileName;
                }
                ticketAttachments.Created = new DateTimeOffset(DateTime.Now);
                db.TicketAttachments.Add(ticketAttachments);
 

                var notification = new IdentityMessage();
                var changed = DateTime.Now;
                var transportWeb = new Web(ConfigurationManager.AppSettings["SendGridAPIKey"]);
                TicketHistory thatt = new TicketHistory
                {
                    TicketId = ticketAttachments.TicketId,
                    Property = "Attachment",
                    OldValue = "",
                    NewValue = ticketAttachments.FilePath,
                    Changed = changed,
                    UserId = currentUserId,

                    User = currentUser,                            //this is for use in notification message
                };

                notification.Body = "A new attachment, " + thatt.NewValue + " has been added by " + thatt.User.DisplayName + ".";
                SendGridMessage mymessage = new SendGridMessage();
                mymessage.Html = notification.Body;
                transportWeb.DeliverAsync(mymessage);
                db.TicketHistories.Add(thatt);
                db.SaveChanges();

                return RedirectToAction("Details", "Tickets", new { id = ticketAttachments.TicketId });
            }


            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachments.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "DisplayName", ticketAttachments.UserId);


            return View(ticketAttachments);
        }

        // GET: TicketAttachments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachments = db.TicketAttachments.Find(id);
            if (ticketAttachments == null)
            {
                return HttpNotFound();
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachments.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "DisplayName", ticketAttachments.UserId);
            return View(ticketAttachments);
        }

        // POST: TicketAttachments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TicketId,FilePath,Description,Created,UserId,FileUrl")] TicketAttachment ticketAttachment, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ticketAttachment).State = EntityState.Modified;
                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/img/attachments/"), fileName));
                    ticketAttachment.FileUrl = "~/img/attachments/" + fileName;
                }
                db.SaveChanges();
                var oldticketAttachment = db.TicketAttachments.AsNoTracking().FirstOrDefault(t => t.Id == ticketAttachment.Id);
                var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                var currentUser = db.Users.Find(currentUserId);
                var notification = new IdentityMessage();
                var changed = DateTime.Now;
                var transportWeb = new Web(ConfigurationManager.AppSettings["SendGridAPIKey"]);
                TicketHistory thatt = new TicketHistory
                {
                    TicketId = ticketAttachment.TicketId,
                    Property = "Attachment",
                    OldValue = oldticketAttachment.FileUrl,
                    NewValue = ticketAttachment.FileUrl,
                    Changed = changed,
                    UserId = currentUserId,

                    User = currentUser,                            //this is for use in notification message
                };

                notification.Body = "The " + thatt.Property + " has been changed from " + thatt.OldValue + " to " + thatt.NewValue + " by " + thatt.User.DisplayName + ".";
                SendGridMessage mymessage = new SendGridMessage();
                mymessage.Html = notification.Body;
                transportWeb.DeliverAsync(mymessage);
                db.TicketHistories.Add(thatt);


                return RedirectToAction("Index");
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "DisplayName", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachments = db.TicketAttachments.Find(id);
            if (ticketAttachments == null)
            {
                return HttpNotFound();
            }
            return View(ticketAttachments);
        }

        // POST: TicketAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TicketAttachment ticketAttachments = db.TicketAttachments.Find(id);
            db.TicketAttachments.Remove(ticketAttachments);
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
