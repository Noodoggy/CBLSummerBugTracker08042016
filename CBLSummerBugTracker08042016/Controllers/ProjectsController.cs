using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
        
        private UserRolesHelper ur = new UserRolesHelper();



        [Authorize(Roles = "Admin, Project Manager, Developer")]
        public ActionResult Index()
        {
            UserRolesHelper helper = new UserRolesHelper();
            var currentUser = db.Users.Find(System.Web.HttpContext.Current.User.Identity.GetUserId());      //get current user id
            var projectList = new ProjectIndexViewModel();
           
            if (helper.IsUserInRole(User.Identity.GetUserId(), "Admin"))                                   
            {
                ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");                      //if admin, allow all projects 

                projectList.ProjectList = db.Projects.ToList();
            }
            else
            {
                ViewBag.ProjectId = new SelectList(currentUser.Project, "Id", "Name");              //else, only projects assigned to
                projectList.ProjectList = currentUser.Project.ToList();
            }
            return View(projectList);
        }


        [Authorize(Roles = "Admin, Project Manager, Developer")]
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



        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Created")] Project project)
        {
            
            if (ModelState.IsValid)
            {
                var createProject = new Project();
                createProject.Created = new DateTimeOffset(DateTime.Now);
                createProject.Name = project.Name;                  
                db.Projects.Add(createProject);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            return View();
        }




        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Created")] Project project)
        {
            if (ModelState.IsValid)
            {

                db.Entry(project).State = EntityState.Modified;                                  
                db.Entry(project).Property("Name").IsModified = true;                               
                db.Entry(project).Property("Created").IsModified = false;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            return View(project);
        }

        //commented out since no one is allowed to delete projects but kept in code for future allowance

        //// GET: Projects/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Project projects = db.Projects.Find(id);
        //    if (projects == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(projects);
        //}

        //// POST: Projects/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Project project = db.Projects.Find(id);
        //    db.Projects.Remove(project);
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
   

    //Get Action
    [Authorize(Roles = "Admin, Project Manager")]
    public ActionResult EditAssignedProjectUsers(int Id)
    {
        var project = db.Projects.Find(Id);                                             //find project by id
        UserProjectsHelper helper = new UserProjectsHelper();                         //helper that can be used locally
        var model = new ProjectUserListViewModel();                                     //use model to render in view
        model.ProjectName = project.Name;                                               //insert project data
        model.Id = project.Id;                                                           //insert project id
        model.selected = helper.ListProjectUsers(Id).ToArray();                         //users on project to array of selected for multiselect list
            model.Project = project;
        model.users = new MultiSelectList(db.Users,"Id", "DisplayName", model.selected);        //mulitselect list with names and selected as users on project


        return View(model);
    }


    //POST Action
    [Authorize(Roles = "Admin, Project Manager")]
    [HttpPost]
    public ActionResult EditAssignedProjectUsers([Bind(Include = "Id, Project, ProjectName, selected")] ProjectUserListViewModel changedUsers)
    {
        UserRolesHelper roles = new UserRolesHelper();
        UserProjectsHelper helper = new UserProjectsHelper();                                                    
        List<string> ListSelected = new List<string>();                                       //selected list of users to assign
        List<string> ListToRemove = new List<string>(helper.ListProjectUsers(changedUsers.Id).ToArray());          //creates list of users assigned in Get Action 

        foreach (var item in changedUsers.selected)                                                    //create selected list
            {
                ListSelected.Add(item);
            }

        foreach (var item in ListSelected)                                   //if item on selected list occurs in ListToRemove, then remove
            {
                ListToRemove.Remove(item);
            }


        foreach (var item in ListSelected)                                          //action of adding to list
            {
                if (!helper.IsUserOnProject(item, changedUsers.Id))                            //check to make sure not already assigned to project
                {
                    helper.AddUserToProject(item, changedUsers.Id);                             //add
                    if (!roles.IsUserInRole(item, "Submitter"))
                        {
                            roles.AddUserToRole(item, "Submitter");
                        }
                }
            }

        foreach (var item in ListToRemove)                                          //action of removing from list
        {
            if (helper.IsUserOnProject(item, changedUsers.Id))                             //check to make sure is assigned to project
            {
                helper.RemoveUserFromProject(item, changedUsers.Id);                       //remove
            }
        }
            var model = new ProjectUserListViewModel();
            model.Id = changedUsers.Id;
            model.Project = db.Projects.Find(changedUsers.Id);
            model.ProjectName = model.Project.Name;
            model.selected = db.Users.Select(n => n.Id).ToArray();                      //update model.selected to reflect projectusers table
        model.users = new MultiSelectList(db.Users, "Id", "DisplayName", changedUsers.selected);       //multiselect with selected for projectusers
        db.SaveChanges();                                                                   
        return View(model);
    }
}
}
