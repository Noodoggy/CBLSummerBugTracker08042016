using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public class RoleManagerViewModel
    {
        public string RoleName { get; set; }
        public MultiSelectList RoleList { get; set; }
        public string[] SelectedList { get; set; }

    }
}