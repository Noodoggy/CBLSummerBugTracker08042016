using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public class ProjectUserListViewModel
    {
        public string ProjectName { get; set; }
        public int Id { get; set; }
        public MultiSelectList users { get; set; }
        public string[] selected { get; set; }
        public virtual Project Project { get; set; }
    }

}