using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public class UsersByRoleViewModel
    {
        public ICollection<ApplicationUser> Admin { get; set; }
        public ICollection<ApplicationUser> PM { get; set; }
        public ICollection<ApplicationUser> Dev { get; set; }
        public ICollection<ApplicationUser> Sub { get; set; }
        public List<string> Roles { get; set; }
    }
       
}