using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBLSummerBugTracker08042016.Models.CodeFirst.Helpers
{
    public class UserProjectsHelper
    {
        private ApplicationDbContext db;

        public UserProjectsHelper(ApplicationDbContext db)
        {
            this.db = db;//allow operations on database
        }

        public bool IsUserOnProject(string userId, int projectId)
        {
            Project isOnProject = db.Projects.FirstOrDefault(p => p.Id == projectId);//find project
            var flag = isOnProject.User.Any(u => u.Id == userId);//query if user on project
            return flag;//return true or false
        }

        public IList<int> ListUserProjects(string userId)
        {
            UserProjectsHelper helper = new UserProjectsHelper(db);//assign helper to db
            var up = new ProjectUserListViewModel();//use view model for data to display
            IList<int> userProjects = new List<int>();//new list to hold projects that user is on
            var proj = db.Projects;//access projects db
            foreach (var item in proj)
                if (IsUserOnProject(userId, item.Id))//use helper to check if on project
                {
                    up.Id = item.Id;
                    userProjects.Add(up.Id);//add project id to list of projects user is on
                }
            return userProjects;        //return list of projects
        }

        public IList<string> ListProjectUsers(int projectId)
        {
            
            var project = db.Projects.Find(projectId);          //grab project with project id
            IList<string> projectUsers = new List<string>();            //make a list to put users into     
            foreach (var item in project.User)            //for each user in project add to newly created list
            {
                projectUsers.Add(item.Id);
            }

            return projectUsers;            //return list of users

        }

        public void AddUserToProject(string userId, int projectId)
        {
            ApplicationUser user = db.Users.Find(userId);           //find user
            Project project = db.Projects.First(p => p.Id == projectId);            //select project
            IEnumerable<ApplicationUser> projectUsers = project.User.ToList();          //create a list of users on project
            UserProjectsHelper projectUserListHelper = new UserProjectsHelper(db);        //helper to access database

            if (!projectUserListHelper.IsUserOnProject(user.Id, project.Id))            //if user is not on project
            {
                project.User.Add(user);                     //add to project, else do nothing
                db.SaveChanges();
            }

        }

        public void RemoveUserFromProject(string userId, int projectId)
        {
            ApplicationUser user = db.Users.Find(userId);               //find user
            Project project = db.Projects.First(p => p.Id == projectId);            //select project
            IEnumerable<ApplicationUser> projectUsers = project.User.ToList();          //create a list of users on project
            UserProjectsHelper projectUserListHelper = new UserProjectsHelper(db);        //helper to access database

            if (projectUserListHelper.IsUserOnProject(user.Id, project.Id))         //if user is on project
            {
                project.User.Remove(user);          //remove from project, else do nothing
                db.SaveChanges();
            }

        }
    }
}