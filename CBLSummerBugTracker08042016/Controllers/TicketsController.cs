﻿using System;
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
using System.Reflection;
using System.Collections.Generic;

namespace CBLSummerBugTracker08042016.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        protected ApplicationDbContext ApplicationDbContext { get; set; }
        protected UserManager<ApplicationUser> UserManager { get; set; }




        [Authorize]
        public ActionResult Index()
        {
            UserRolesHelper helper = new UserRolesHelper();
            var currentUserId = User.Identity.GetUserId();                   //find current user by id
            var currentUser = db.Users.Find(User.Identity.GetUserId());

            if (helper.IsUserInRole(User.Identity.GetUserId(), "Admin"))
            {
                ViewBag.UserId = currentUserId;
                return View((db.Tickets.Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType)).ToList());
            }

            if (helper.IsUserInRole(User.Identity.GetUserId(), "Project Manager"))
            {
                ViewBag.UserId = currentUserId;
                return View((currentUser.Project.SelectMany(p => p.Ticket)).ToList());
                //return View(currentUser.Project.SelectMany(p => p.Ticket).AsQueryable().Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType).ToList());               //tickets of projects assigned to user

            }
            if (helper.IsUserInRole(User.Identity.GetUserId(), "Developer"))
            {
                ViewBag.UserId = currentUserId;
                return View((db.Tickets.Where(u => u.AssignedToUserId == currentUserId).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType)).ToList());        //tickets assigned to user;
            }
            if (helper.IsUserInRole(User.Identity.GetUserId(), "Submitter"))
            {
                ViewBag.UserId = currentUserId;
                return View((db.Tickets.Where(u => u.OwnerUserId == currentUserId).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType)).ToList());             //tickets owned by user
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // GET: Tickets/Details/5
        [Authorize/*(Roles = "Submitter, Admin")*/]
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
            UserProjectsHelper helper2 = new UserProjectsHelper();
            var currentUserId = User.Identity.GetUserId();                   //find current user by id
            var currentUser = db.Users.Find(currentUserId);                                                //find user id 
            var ticketassigned = db.Tickets.Where(u => u.AssignedToUserId == currentUserId);             //list of tickets assigned to user
            ViewBag.UserId = currentUserId;
            if ((ticket.OwnerUserId == currentUserId)                                                   //check if currentuser is ticketowner
                || (ticket.AssignedToUserId == currentUserId)                                           //check if currentuser is assigned ticket
                || (helper.IsUserInRole(currentUserId, "Admin"))
                || helper2.IsUserOnProject(currentUserId, ticket.ProjectId)) //check if user is on assigned project
              
            {
                if (ticket.TicketAttachment != null && ticket.TicketComment != null)
                {
                    var combinedList = new List<ICommentAttachmentInterface>();

                    foreach (var item in ticket.TicketAttachment)
                    {
                        combinedList.Add(item);
                    }
                    foreach (var item in ticket.TicketComment)
                    {
                        combinedList.Add(item);
                    }



                    ViewBag.CommAtt = combinedList.OrderByDescending(d => d.Date);
                }

                
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
            if ((helper.IsUserInRole(User.Identity.GetUserId(), "Submitter")) || (helper.IsUserInRole(User.Identity.GetUserId(), "Developer")))             //possible code for changing status to always pending if not project manager or admin
            {
                ViewBag.TicketStatusId = new SelectList(db.TicketStatuses.Where(s => s.Name == "Pending").AsEnumerable(), "Id", "Name")/* "Pending"*/;
            }
            else
            {
                ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            }
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }



        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Submitter, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Created,Updated,Title,Description,OwnerUserId,TicketStatusId")] Ticket tickets)  //extracted code:  ,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId
        {
            if (ModelState.IsValid)
            {
                tickets.ProjectId = null;
                tickets.TicketStatus = db.TicketStatuses.Find(tickets.TicketStatusId);
                db.Tickets.Add(tickets);                //add data to tickets table
                db.Entry(tickets).Property("ProjectId").IsModified = false;
                tickets.OwnerUserId = User.Identity.GetUserId();         //set owner to current user id
                tickets.OwnerUser = db.Users.Find(tickets.OwnerUserId);                     //set owner to current user (for display purposes)
                tickets.Created = DateTimeOffset.Now;
                db.SaveChanges();


            }
            UserRolesHelper helper = new UserRolesHelper();
            ViewBag.ProjectId = new SelectList(tickets.OwnerUser.Project, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            if ((helper.IsUserInRole(User.Identity.GetUserId(), "Submitter")) || (helper.IsUserInRole(User.Identity.GetUserId(), "Developer")))             //possible code for changing status to always pending if not project manager or admin
            {
                ViewBag.TicketStatusId = new SelectList(db.TicketStatuses.Where(s => s.Name == "Pending").AsEnumerable(), "Id", "Name");
            }
            else
            {
                ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            }
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");

            return RedirectToAction("TicketManagement");


        }

        // GET: Tickets/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            Ticket ticket = db.Tickets.Find(id);                            //find ticket by id

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (ticket == null)
            {
                return HttpNotFound();
            }



            UserRolesHelper helper = new UserRolesHelper();
            var currentUserId = User.Identity.GetUserId();  //get currentuser Id
            var currentUser = db.Users.Find(currentUserId);
            UserProjectsHelper helper2 = new UserProjectsHelper();
            var users = db.Users.AsEnumerable();
            if (ticket.OwnerUserId == currentUserId                   //check for owneruser
                || ticket.AssignedToUserId == currentUserId               //check for assigned user
                || helper2.IsUserOnProject(currentUserId, ticket.ProjectId)                    //check for user assigned to project, thus ticket
                || helper.IsUserInRole(currentUserId, "Admin"))    //check for admin
            {
                if ((ticket.OwnerUserId != currentUserId) && !helper2.IsUserOnProject(currentUserId, ticket.ProjectId) && !helper.IsUserInRole(currentUserId, "Admin") && !(ticket.AssignedToUserId == currentUserId))
                {
                    ViewBag.Message = "You are not authorized to edit this ticket. Ticket might not be ready for editing.  Please log in with the correct credentials.";
                    return RedirectToAction("Login", "Account");
                }

                if (helper.IsUserInRole(currentUserId, "Admin") || helper.IsUserInRole(currentUserId, "Project Manager"))           //check for admin or project manager
                {

                    ViewBag.AssignedToUserId = new SelectList(users, "Id", "DisplayName", ticket.AssignedToUserId);         //allow for assigning of ticket
                }
                else
                    {
                    ViewBag.AssignedToUserId = ticket.AssignedToUserId;
                }


                if (helper.IsUserInRole(currentUserId, "Admin"))              //check for admin
                {
                    ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);            //allow for project list to assign to
                }
                else if (helper.IsUserInRole(currentUserId, "Project Manager"))              //check for project manager
                {
                    ViewBag.ProjectId = new SelectList(currentUser.Project, "Id", "Name", ticket.ProjectId);             //list of assigned projects for PM to assign tickets to
                }
                else
                {

                    ViewBag.ProjectId = new SelectList(db.Projects.Where(t => t.Id == ticket.ProjectId).AsEnumerable(), "Id", "Name");          //otherwise just show project name
                }
                ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);      //list of priority choicecs
                                                                                                                            //ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);        //list of status choices
                if ((helper.IsUserInRole(currentUserId, "Submitter")) || (helper.IsUserInRole(currentUserId, "Developer")))          //possible code for changing status to static if not project manager or admin
                {
                    ViewBag.TicketStatusId = new SelectList(db.TicketStatuses.Where(t => t.Id == ticket.TicketStatusId).AsEnumerable(), "Id", "Name");
                }
                if ((helper.IsUserInRole(currentUserId, "Admin")) || (helper.IsUserInRole(currentUserId, "Project Manager")))
                {
                    ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
                }
                ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);       //list of type choices
                return View(ticket);
            }


            else
            {
                ViewBag.Message = "You are not authorized to view this ticket.  Please log in with the correct credentials.";
                return RedirectToAction("Login", "Account");
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
                var oldticket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);  //pull into memory the current ticket without changing table 
                var apiKey = ConfigurationManager.AppSettings["SendGridAPIKey"];
                var from = ConfigurationManager.AppSettings["ContactEmail"];
                var notification = new IdentityMessage();

                if (oldticket.AssignedToUserId != null)
                {
                    notification.Destination = db.Users.Find(oldticket.AssignedToUserId).UserName;          //if already assigned, send notification
                    notification.Subject = "A change has been made to your Assigned Ticket: " + oldticket.Title;
                    TicketNotification tn = new TicketNotification
                    {
                        TicketId = ticket.Id,
                        UserId = oldticket.AssignedToUserId
                    };
                    TicketHistory thn = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "Notification",
                        OldValue = "",
                        NewValue = "Sent to " + db.Users.Find(oldticket.AssignedToUserId).UserName,
                        Changed = changed,
                        UserId = userId,

                        User = user,
                    };
                    db.TicketHistories.Add(thn);
                    db.TicketNotifications.Add(tn);
                    var transportWeb3 = new Web(ConfigurationManager.AppSettings["SendGridAPIKey"]);
                    SendGridMessage mymessage3 = new SendGridMessage();
                    mymessage3.AddTo(notification.Destination);
                    mymessage3.From = new System.Net.Mail.MailAddress(from);
                    mymessage3.Subject = notification.Subject;
                    mymessage3.Html = notification.Body;
                    transportWeb3.DeliverAsync(mymessage3);
                }

                if ((oldticket.AssignedToUserId == null && ticket.AssignedToUserId != null) || (oldticket.AssignedToUserId != null && ticket.AssignedToUserId != null))
                {
                    notification.Destination = db.Users.Find(ticket.AssignedToUserId).UserName;            //if edited assigned user, send notification
                    notification.Subject = "A change has been made to your Assigned Ticket: " + oldticket.Title;
                    TicketNotification tn = new TicketNotification
                    {
                        TicketId = ticket.Id,
                        UserId = ticket.AssignedToUserId
                    };

                    TicketHistory thn = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "Notification",
                        OldValue = "",
                        NewValue = "Sent to " + db.Users.Find(ticket.AssignedToUserId).UserName,
                        Changed = changed,
                        UserId = userId,

                        User = user,
                    };
                    db.TicketHistories.Add(thn);
                    db.TicketNotifications.Add(tn);
                    var transportWeb4 = new Web(ConfigurationManager.AppSettings["SendGridAPIKey"]);
                    SendGridMessage mymessage4 = new SendGridMessage();
                    mymessage4.AddTo(notification.Destination);
                    mymessage4.From = new System.Net.Mail.MailAddress(from);
                    mymessage4.Subject = notification.Subject;
                    mymessage4.Html = notification.Body;
                    transportWeb4.DeliverAsync(mymessage4);
                };

                var transportWeb = new Web(ConfigurationManager.AppSettings["SendGridAPIKey"]);
                SendGridMessage mymessage = new SendGridMessage();
                if ((oldticket.AssignedToUserId == null && ticket.AssignedToUserId != null) || (oldticket.AssignedToUserId != null && ticket.AssignedToUserId != null))
                {
                    mymessage.AddTo(notification.Destination);
                    mymessage.From = new System.Net.Mail.MailAddress(from);
                    mymessage.Subject = notification.Subject;
                    mymessage.Html = notification.Body;
                    transportWeb.DeliverAsync(mymessage);
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
                    var oldValue = "Not assigned";
                    if (oldticket?.ProjectId != null)
                    {
                        oldValue = db.Projects.Find(oldticket.ProjectId).Name; 
                    }
                    TicketHistory th3 = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "Updated",
                        OldValue = oldValue,
                        NewValue = db.Projects.Find(ticket.ProjectId).Name,
                        Changed = changed,
                        UserId = userId,

                        User = user
                    };
                    notification.Body = "The " + th3.Property + "property has been changed from " + th3.OldValue + " to " + th3.NewValue + " by " + th3.User.DisplayName + ".";

                    mymessage.Html = notification.Body;
                    transportWeb.DeliverAsync(mymessage);
                    db.TicketHistories.Add(th3);
                }

                if (oldticket?.TicketTypeId != ticket.TicketTypeId)
                {
                    var oldValue = "Not assigned";
                    if (oldticket?.TicketTypeId != null)
                    {
                        oldValue = db.Projects.Find(oldticket.TicketTypeId).Name;
                    }
                    TicketHistory th4 = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "TicketTypeId",
                        OldValue = oldValue,
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
                    var oldValue = "Not assigned";
                    if (oldticket?.TicketPriorityId != null)
                    {
                        oldValue = db.Projects.Find(oldticket.TicketPriorityId).Name;
                    }
                    TicketHistory th5 = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "TicketPriorityId",
                        OldValue = oldValue,
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
                    var oldValue = "Not assigned";
                    if (oldticket?.TicketStatusId != null)
                    {
                        oldValue = db.Projects.Find(oldticket.TicketStatusId).Name;
                    }
                    TicketHistory th6 = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Property = "TicketStatusId",
                        OldValue = oldValue,
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
                if ((oldticket?.AssignedToUserId == null && ticket.AssignedToUserId != null) || (oldticket?.AssignedToUserId != ticket.AssignedToUserId))


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
                        Body = "You have been assigned to the following ticket: " + ticket.Title + ": " + ticket.Description + "."

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


                    //code to determine if old assigned user has other tickets assigned to project and if not, then remove from project

                        UserProjectsHelper helper3 = new UserProjectsHelper();
                    if (oldticket.AssignedToUserId != null)
                    {
                        var userTicket = db.Users.Find(oldticket.AssignedToUserId).Project.SelectMany(p => p.Ticket).ToList();
                        var projectTickets = userTicket.FindAll(t => t.ProjectId == oldticket.ProjectId);

                        if (projectTickets.Count() == 1)                //code to possibly remove user from project if no other tickets on project
                        {
                            helper3.RemoveUserFromProject(oldticket.AssignedToUserId, ticket.ProjectId);
                        }
                    }
                    if (!helper3.IsUserOnProject(ticket.AssignedToUserId, ticket.ProjectId))
                    { helper3.AddUserToProject(ticket.AssignedToUserId, ticket.ProjectId); } //automatically add user to project
                    
                    

                }


                db.Entry(ticket).State = EntityState.Modified;                              //orignal statement
                db.Entry(ticket).Property("Title").IsModified = true;                       //added this and and the next line.
                db.Entry(ticket).Property("Created").IsModified = false;

                ticket.Updated = DateTimeOffset.Now;

                var helper4 = new UserProjectsHelper();
                ticket.AssignedToUser = db.Users.Find(ticket.AssignedToUserId);
                if (!helper4.IsUserOnProject(ticket.AssignedToUserId, ticket.ProjectId))
                { helper4.AddUserToProject(ticket.AssignedToUserId, ticket.ProjectId); }
                db.SaveChanges();



            }


            UserRolesHelper helper = new UserRolesHelper();
            var currentUserId = User.Identity.GetUserId();  //get currentuser Id
            var currentUser = db.Users.Find(currentUserId);
            UserProjectsHelper helper2 = new UserProjectsHelper();

            if ((ticket.OwnerUserId == currentUserId)                   //check for owneruser
                || (ticket.AssignedToUserId == currentUserId)               //check for assigned user
                || (helper2.IsUserOnProject(currentUserId, ticket.ProjectId) && (helper.IsUserInRole(User.Identity.GetUserId(), "Project Manager")))           //check for user assigned to project, thus ticket
                || (helper.IsUserInRole(User.Identity.GetUserId(), "Admin")))         //check for admin
            {

                if (helper.IsUserInRole(User.Identity.GetUserId(), "Admin") || helper.IsUserInRole(User.Identity.GetUserId(), "Project Manager"))           //check for admin or project manager
                {

                    ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "Username", ticket.AssignedToUserId);         //allow for assigning of ticket
                }
                if (ticket.AssignedToUserId != null)
                {
                    ViewBag.AssignedToUserId = new SelectList(ticket.AssignedToUserId);             //otherwise just show ticket as assigned to user - no edit
                }
                else if (ticket.OwnerUserId != currentUserId)
                {
                    ViewBag.Message = "You are not authorized to view this ticket. Ticket might not be ready for editing.  Please log in with the correct credentials.";
                    return RedirectToAction("Login", "Account");
                }

                if (helper.IsUserInRole(User.Identity.GetUserId(), "Admin"))              //check for admin
                {
                    ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);            //allow for project list to assign to
                }
                else if (helper.IsUserInRole(User.Identity.GetUserId(), "Project Manager"))              //check for project manager
                {
                    ViewBag.ProjectId = new SelectList(currentUser.Project, "Id", "Name", ticket.ProjectId);             //list of assigned projects for PM to assign tickets to
                }
                else
                {

                    ViewBag.ProjectId = ViewBag.ProjectId = new SelectList(db.Projects.Where(t => t.Id == ticket.ProjectId).AsEnumerable(), "Id", "Name");          //otherwise just show project name
                }
                ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);      //list of priority choicecs
                                                                                                                            //ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);        //list of status choices
                if ((helper.IsUserInRole(User.Identity.GetUserId(), "Submitter")) || (helper.IsUserInRole(User.Identity.GetUserId(), "Developer")))          //possible code for changing status to static if not project manager or admin
                {
                    ViewBag.TicketStatusId = new SelectList(db.TicketStatuses.Where(t => t.Id == ticket.TicketStatusId).AsEnumerable(), "Id", "Name");
                }
                else if ((helper.IsUserInRole(User.Identity.GetUserId(), "Admin")) || (helper.IsUserInRole(User.Identity.GetUserId(), "Project Manager")))
                {
                    ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
                }
                ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);       //list of type choices
                ViewBag.UserId = currentUserId;
                return View(ticket);
            }


            else
            {
                ViewBag.Message = "You are not authorized to view this ticket.  Please log in with the correct credentials.";
                return RedirectToAction("Login", "Account");
            }
        

            
        }

        //commented out to prevent accidental deletion of tickets
        //// GET: Tickets/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Ticket tickets = db.Tickets.Find(id);
        //    if (tickets == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(tickets);
        //}

        //// POST: Tickets/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Ticket tickets = db.Tickets.Find(id);
        //    db.Tickets.Remove(tickets);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}




        public ActionResult MyTickets()
        {
            //helps create list of projects user is assigned to 
            var currentUserId = User.Identity.GetUserId();
            
            ViewBag.UserId = currentUserId;
            return View(db.Tickets.Where(u => u.OwnerUserId == currentUserId).ToList());
        }



        public ActionResult AssignedTickets()
        {
            var currentUserId = User.Identity.GetUserId();
            var ticket = db.Tickets.Where(u => u.AssignedToUserId == currentUserId);
            ViewBag.UserId = currentUserId;
            return View(ticket.ToList());
        }

        public ActionResult MyProjectTickets([Bind(Include = "ProjectName, Id, tickets")] TicketViewModel model)
        {
            UserRolesHelper helper = new UserRolesHelper();
            var currentUserId = User.Identity.GetUserId();
            var ticket = new List<Ticket>();
            if (helper.IsUserInRole(currentUserId, "Admin"))
            {
                ticket = db.Tickets.ToList();
            }
            else
            {
                ticket = db.Users.Find(currentUserId).Project.SelectMany(p => p.Ticket).ToList();
            }
            ViewBag.UserId = currentUserId;
            return View(ticket);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [Authorize]
        public ActionResult TicketManagement()
        {
            UserRolesHelper helper = new UserRolesHelper();
            var currentUserId = User.Identity.GetUserId();                   //find current user by id
            var currentUser = db.Users.Find(User.Identity.GetUserId());

            if (helper.IsUserInRole(User.Identity.GetUserId(), "Admin"))
            {
                ViewBag.UserId = currentUserId;
                var tmvm = new TicketManagementViewModel();
                tmvm.TicketList = (db.Tickets.Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType)).ToList();
                tmvm.CreateTicket = new Models.CodeFirst.Ticket();
                return View(tmvm);
            }
            else
            if (helper.IsUserInRole(User.Identity.GetUserId(), "Project Manager"))
            {
                ViewBag.UserId = currentUserId;
                var tmvm = new TicketManagementViewModel();
                tmvm.TicketList = (currentUser.Project.SelectMany(p => p.Ticket)).ToList();
                tmvm.CreateTicket = new Models.CodeFirst.Ticket();
                return View(tmvm);
                //return View(currentUser.Project.SelectMany(p => p.Ticket).AsQueryable().Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType).ToList());               //tickets of projects assigned to user

            }
            else
            if (helper.IsUserInRole(User.Identity.GetUserId(), "Developer"))
            {
                ViewBag.UserId = currentUserId;
                var tmvm = new TicketManagementViewModel();
                tmvm.TicketList = (db.Tickets.Where(u => u.AssignedToUserId == currentUserId).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType)).ToList();
                tmvm.CreateTicket = new Models.CodeFirst.Ticket();
                return View(tmvm);       //tickets assigned to user;
            }
            else
            if (helper.IsUserInRole(User.Identity.GetUserId(), "Submitter"))
            {
                ViewBag.UserId = currentUserId;
                var tmvm = new TicketManagementViewModel();
                tmvm.TicketList = ((db.Tickets.Where(u => u.OwnerUserId == currentUserId).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType)).ToList());
                tmvm.CreateTicket = new Models.CodeFirst.Ticket();
                return View(tmvm);           //tickets owned by user
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

    }
}



