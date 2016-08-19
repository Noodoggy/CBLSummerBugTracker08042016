using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBLSummerBugTracker08042016.Models.CodeFirst
{
    public interface ICommentAttachmentInterface
    {
         DateTimeOffset Date { get; }
    }

    public partial class TicketAttachment : ICommentAttachmentInterface
    {
        public DateTimeOffset Date
        {
            get { return Created; }
        }
    }

    public partial class TicketComment : ICommentAttachmentInterface
    {
        public DateTimeOffset Date
        {
            get { return Created; }
        }
    }
}