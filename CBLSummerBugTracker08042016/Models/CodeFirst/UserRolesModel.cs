using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public class UserRolesModel : IEnumerable
    {
        public ApplicationUser user { get; set; }
        public ICollection<string> roles { get; set; }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}