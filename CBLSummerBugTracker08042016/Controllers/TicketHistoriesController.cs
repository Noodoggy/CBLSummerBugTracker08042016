using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CBLSummerBugTracker08042016.Models;
using CBLSummerBugTracker08042016.Models.CodeFirst;

namespace CBLSummerBugTracker08042016.Controllers
{
    public class TicketHistoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: TicketHistories
        public ActionResult Index()
        {
            var ticketHistories = db.TicketHistories.Include(t => t.Ticket).Include(t => t.User);
            return View(ticketHistories.ToList());
        }

        // GET: TicketHistories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketHistory ticketHistory = db.TicketHistories.Find(id);
            if (ticketHistory == null)
            {
                return HttpNotFound();
            }
            return View(ticketHistory);
        }
        //commented for preserving history, can uncomment to make changes
        // GET: TicketHistories/Create
        //public ActionResult Create()
        //{
        //    ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
        //    ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName");
        //    return View();
        //}

        // POST: TicketHistories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TicketId,Property,OldValue,NewValue,Changed,UserId")] TicketHistory ticketHistory)
        {
            if (ModelState.IsValid)
            {
                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketHistory.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketHistory.UserId);
            return View(ticketHistory);
        }
//commented for preserving history, can uncomment to make changes
        //// GET: TicketHistories/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TicketHistory ticketHistory = db.TicketHistories.Find(id);
        //    if (ticketHistory == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketHistory.TicketId);
        //    ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketHistory.UserId);
        //    return View(ticketHistory);
        //}

        //// POST: TicketHistories/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,TicketId,Property,OldValue,NewValue,Changed,UserId")] TicketHistory ticketHistory)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(ticketHistory).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketHistory.TicketId);
        //    ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketHistory.UserId);
        //    return View(ticketHistory);
        //}

        //// GET: TicketHistories/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TicketHistory ticketHistory = db.TicketHistories.Find(id);
        //    if (ticketHistory == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ticketHistory);
        //}

        //// POST: TicketHistories/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    TicketHistory ticketHistory = db.TicketHistories.Find(id);
        //    db.TicketHistories.Remove(ticketHistory);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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
