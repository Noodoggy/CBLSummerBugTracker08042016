using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public class ListUserViewModel : ApplicationUser
    {
       public IQueryable<ApplicationUser> user { get; set; } 
        
    }
}