using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public class ProjectIndexViewModel
    {
        public IList<Project> ProjectList { get; set; }
        public Project CreateProject { get; set; }
    }
}