using CBLSummerBugTracker08042016.Models;
using CBLSummerBugTracker08042016.Models.CodeFirst;
using CBLSummerBugTracker08042016.Models.CodeFirst.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        public ActionResult EditUser(string id)
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
        public ActionResult EditUser([Bind(Include = "selected, Id")] AdminUserViewModel model)
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


            model.roles = new MultiSelectList(db.Roles, "Name", "Name", roleManager.ListUserRoles(user).ToArray());  //refreshed selectlist
            return View(model);
        }


        public ActionResult ListUsers()
        {
            var users = db.Users.ToList();
            return View(users);
        }


    }
}