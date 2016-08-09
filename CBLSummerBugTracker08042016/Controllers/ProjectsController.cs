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
using CBLSummerBugTracker08042016.Models.CodeFirst.Helpers;
using Microsoft.AspNet.Identity;

namespace CBLSummerBugTracker08042016.Controllers
{
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Projects
        public ActionResult Index()
        {
            UserRolesHelper helper = new UserRolesHelper();
            //var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var currentUser = db.Users.Find(System.Web.HttpContext.Current.User.Identity.GetUserId());
            if (helper.IsUserInRole(User.Identity.GetUserId(), "Admin"))
            {
                ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            }
            else
            {
                ViewBag.ProjectId = new SelectList(currentUser.Project, "Id", "Name");
            }
            return View(db.Projects.ToList());
        }

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }
        [Authorize(Roles = "Admin")]
        // GET: Projects/Create
        public ActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Created")] Project project)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                project.Created = new DateTimeOffset(DateTime.Now);
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Created")] Project project)
        {
            if (ModelState.IsValid)
            {

                db.Entry(project).State = EntityState.Modified;                                  ///orignal statement
                db.Entry(project).Property("Name").IsModified = true;                               //added this and and the next line.
                db.Entry(project).Property("Created").IsModified = false;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            return View(project);
        }

        // GET: Projects/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project projects = db.Projects.Find(id);
            if (projects == null)
            {
                return HttpNotFound();
            }
            return View(projects);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
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
   
    //Get Action
    [Authorize(Roles = "Admin, Project Manager")]
    public ActionResult EditAssignedProjectUsers(int Id)
    {
        var project = db.Projects.Find(Id);
        UserProjectsHelper helper = new UserProjectsHelper(db);
        var model = new ProjectUserListViewModel();
        model.ProjectName = project.Name;
        model.Id = project.Id;
        model.selected = helper.ListProjectUsers(Id).ToArray();

        model.users = new MultiSelectList(db.Users, "Id", "DisplayName", model.selected);


        return View(model);
    }
    //Post Action
    [Authorize(Roles = "Admin, Project Manager")]
    [HttpPost]
    public ActionResult EditAssignedProjectUsers([Bind(Include = "ProjectName, selected, Id, users")] ProjectUserListViewModel model)
    {
        //this "calls" the function in UserRolesHelper
        UserProjectsHelper helper = new UserProjectsHelper(db);
        var UsersOnProjects = db.Projects.Find(model.Id);                                                       //get project
/*        var existusers = helper.ListProjectUsers(model.Id).ToArray(); */                                    //create list of project users
//line above inserted into ListToRemove below in attempt to use less code.  still have to see if it works. 08082016        
       
        List<string> ListSelected = new List<string>();                                                             //selected list of roles
        List<string> ListToRemove = new List<string>(helper.ListProjectUsers(model.Id).ToArray());                                                         //existing list of roles

        foreach (var item in UsersOnProjects.User)
        {
            ListToRemove.Add(item.Id);
        }


        foreach (var item in model.selected)                                                    //selected list
            {
            ListSelected.Add(item);
        }

        foreach (var item in ListSelected)
        {
            ListToRemove.Remove(item);
        }


        foreach (var item in ListSelected)                                          //action of adding to list
            {
            if (!helper.IsUserOnProject(item, model.Id))
            {
                helper.AddUserToProject(item, model.Id);
            }
        }

        foreach (var item in ListToRemove)
        {
            if (helper.IsUserOnProject(item, model.Id))
            {
                helper.RemoveUserFromProject(item, model.Id);
            }
        }

        //model.ProjectName = model.ProjectName;                                            do i need this to maintain the name??
        model.selected = db.Users.Select(n => n.Id).ToArray();
        model.users = new MultiSelectList(db.Users, "Id", "DisplayName", model.selected);
        db.SaveChanges();
        return View(model);
    }
}
}
