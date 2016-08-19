using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CBLSummerBugTracker08042016.Models.CodeFirst.Helpers;

namespace CBLSummerBugTracker08042016.Models.CodeFirst.Helpers
{
    public class UserTicketsHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        public bool IsUserOwnedTicket(string userId, int ticketId)
        {
            Ticket isAssignedToTicket = db.Tickets.FirstOrDefault(p => p.Id == ticketId);                   //find ticket
            var flag = isAssignedToTicket.OwnerUserId.Equals(userId);                           //query if user on ticket       
            return flag;//return true or false
        }



        public bool IsUserAssignedTicket(string userId, int ticketId)
        {
            Ticket isAssignedToTicket = db.Tickets.AsNoTracking().FirstOrDefault(p => p.Id == ticketId);                   //find ticket
            var flag = isAssignedToTicket.AssignedToUserId.Equals(userId);                      //query if user on ticket
            return flag;//return true or false
        }


        public IList<int> ListUserOwnedTickets(string userId)
        {
                                                             
            IList<int> userOwnedTickets = new List<int>();                  //new list to hold tickets assigned to user 
            var tic = db.Tickets;                                                                       //access tickets db
            foreach (var item in tic)
                if (IsUserOwnedTicket(userId, item.Id))                                     //use helper to check if on ticket
                {
                    
                    userOwnedTickets.Add(item.Id);                                //add ticket id to list of tickets owned by user
                }
            return userOwnedTickets;                                                                //return list of tickets


        }

    public IList<int> ListUserAssignedTickets(string userId)
    {
                                         
        var ut = new TicketsAssignedListViewModel();                                    //use view model for data to display
        IList<int> userAssignedTickets = new List<int>();                       //new list to hold tickets assigned to user 
                                                                                //access tickets db
        foreach (var item in db.Tickets)
            if (IsUserAssignedTicket(userId, item.Id))                                  //use helper to check if on ticket
            {
                ut.Id = item.Id;
                userAssignedTickets.Add(ut.Id);                             //add ticket id to list of tickets assigned to user
            }
        return userAssignedTickets;                                                                      //return list of tickets
    }



    public void AddUserToTicket(string userId, int ticketId)
    {
        ApplicationUser user = db.Users.Find(userId);                                                               //find user
        Ticket ticket = db.Tickets.Find(ticketId);                                                //select ticket

        if (!IsUserAssignedTicket(user.Id, ticket.Id))                                     //if user is not on ticket
        {
            ticket.AssignedToUserId = user.Id;                                                      //add to ticket, else do nothing
            db.SaveChanges();
        }

    }


        ///come back to figure out whether to remove or just change ticket to another user, which might be a better case.

        //public void RemoveUserFromTicket(string userId, int ticketId)
        //{
        //    ApplicationUser user = db.Users.Find(userId);               //find user
        //    Ticket ticket= db.Tickets.First(p => p.Id == ticketId);            //select ticket

        //    UserTicketsHelper userToRemove = new UserTicketsHelper(db);        //helper to access database

        //    if (userToRemove.IsUserAssignedTicket(user.Id, ticket.Id))         //if user is on ticket
        //    {
        //        ticket.AssignedToUserId.          //remove from ticket, else do nothing
        //        db.SaveChanges();
        //    }

        //}




    }


}
