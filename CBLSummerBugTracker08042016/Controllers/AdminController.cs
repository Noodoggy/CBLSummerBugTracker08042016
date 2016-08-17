using CBLSummerBugTracker08042016.Models;
using CBLSummerBugTracker08042016.Models.CodeFirst;
using CBLSummerBugTracker08042016.Models.CodeFirst.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CBLSummerBugTracker08042016.Controllers
{

    [Authorize(Roles = "Admin, Project Manager")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        // GET: Admin
        public ActionResult Index()
        {
            var users = db.Users.ToList();                                          //generate list of users

            return View(users);
        }

        public ActionResult EditUsers(string id)
        {
            var user = db.Users.Find(id);                                                   //find user by id
            UserRolesHelper helper = new UserRolesHelper();                                     //instantiate helper
            var model = new AdminUserViewModel();                                               //instantiate viewmodel
            model.Name = user.DisplayName;                                                      //use display name for user
            model.Id = user.Id;                                                                   //add user data to viewmodel
            model.selected = helper.ListUserRoles(id).ToArray();                                    //add user roles to viewmodel
            model.roles = new MultiSelectList(db.Roles, "Name", "Name", model.selected);             //list of roles with user roles selected
            return View(model);
        }

        [HttpPost]
        public ActionResult EditUsers([Bind(Include = "selected, Id")] AdminUserViewModel model)
        {
            var roleManager = new UserRolesHelper();                                        //UserRolesHelper
            var user = model.Id;                                                            //user id
            var SelectList = model.selected;                                                //selected items in list in model
            var existrole = roleManager.ListUserRoles(user);                                //list of roles user is already in
         //the following generates two lists for comparison
            List<string> ListSelected = new List<string>();                                 //selected list of roles
            List<string> ListExisting = new List<string>();                                 //existing list of roles

            foreach (var item in SelectList)                                                //add selected list 
            {
                ListSelected.Add(item);
            }

            foreach (var item in ListSelected)                                              //adding to role if not in role
            {
                if (!existrole.Contains(item))
                {
                    if (!roleManager.IsUserInRole(user, item))
                    {
                        roleManager.AddUserToRole(user, item);
                    };
                };
            };
            foreach (var item in existrole)                                                 //remove from role if in role
            {
                if (!ListSelected.Contains(item))
                {
                    if (roleManager.IsUserInRole(user, item))
                    {
                        roleManager.RemoveUserFromRole(user, item);
                    };
                };

            };

            model.Name = db.Users.Find(user).DisplayName;                                   //show DisplayName instead of userId
            model.roles = new MultiSelectList(db.Roles, "Name", "Name", roleManager.ListUserRoles(user).ToArray());  //refreshed selectlist
            return View(model);
        }


        public ActionResult ListUsers()
        {
            //UserRolesModel model = new UserRolesModel();
            //UserRolesHelper helper = new UserRolesHelper();
            //foreach (var user in db.Users)
            //{

            //    var roles = helper.ListUserRoles(user.Id);
            //    foreach (var item in roles)
            //    {
            //        var roleName = db.Roles.Select(r => r.Name);
            //    }
            //    model.user = db.Users.Find(user.Id);
            //    model.roles = roleName;
            //}
            

              return View(db.Users.ToList());                

        }


        public ActionResult ListUsersByRole()
        {
            UsersByRoleViewModel model = new UsersByRoleViewModel();
            UserRolesHelper helper = new UserRolesHelper();
            model.Admin = helper.UsersInRole("Admin");
            model.PM = helper.UsersInRole("Project Manager");
            model.Dev = helper.UsersInRole("Developer");
            model.Sub = helper.UsersInRole("Submitter");
           model.Roles = new List<string>();
            foreach (var role in db.Roles)
            {
                model.Roles.Add(role.Name);
            }
            
                
            return View(model);
        }

    }
}