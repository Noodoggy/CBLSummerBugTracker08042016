using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public class AdminUserViewModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public MultiSelectList roles { get; set; }
        public string[] selected { get; set; }
    }
}