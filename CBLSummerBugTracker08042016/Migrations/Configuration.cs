namespace CBLSummerBugTracker08042016.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<CBLSummerBugTracker08042016.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(CBLSummerBugTracker08042016.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            var roleManager = new RoleManager<IdentityRole>(
            new RoleStore<IdentityRole>(context));

            //seed db with roles
            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }
            if (!context.Roles.Any(r => r.Name == "Project Manager"))
            {
                roleManager.Create(new IdentityRole { Name = "Project Manager" });
            }
            if (!context.Roles.Any(r => r.Name == "Developer"))
            {
                roleManager.Create(new IdentityRole { Name = "Developer" });
            }
            if (!context.Roles.Any(r => r.Name == "Submitter"))
            {
                roleManager.Create(new IdentityRole { Name = "Submitter" });
            }

            var userManager = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser>(context));

            //seed db with designer as admin and client as project manager
            if (!context.Users.Any(u => u.Email == "cbentonlandry@gmail.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "cbentonlandry@gmail.com",
                    Email = "cbentonlandry@gmail.com",
                    FirstName = "Benton",
                    LastName = "Landry",
                    DisplayName = "ChBeLa"
                }, "Kissme^3");
            }

            var userId = userManager.FindByEmail("cbentonlandry@gmail.com").Id;
            userManager.AddToRole(userId, "Admin");

            if (!context.Users.Any(u => u.Email == "moderator@coderfoundry.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "moderator@coderfoundry.com",
                    Email = "moderator@coderfoundry.com",
                    FirstName = "Antonio",
                    LastName = "Raynor",
                    DisplayName = "ANIVRA"
                }, "CoderFoundry_2016");

                userId = userManager.FindByEmail("moderator@coderfoundry.com").Id;
                userManager.AddToRole(userId, "Project Manager");
            }

            context.TicketPriorities.AddOrUpdate(
                  p => p.Name,
                  new Models.CodeFirst.TicketPriority { Name = "Blocker" },
                  new Models.CodeFirst.TicketPriority { Name = "Critical" },
                  new Models.CodeFirst.TicketPriority { Name = "Major" },
                  new Models.CodeFirst.TicketPriority { Name = "Minor" },
                  new Models.CodeFirst.TicketPriority { Name = "Trivial" }
                );

            context.TicketTypes.AddOrUpdate(
                  p => p.Name,
                  new Models.CodeFirst.TicketType { Name = "User Interface Defects" },
                  new Models.CodeFirst.TicketType { Name = "Boundary Related Defects" },
                  new Models.CodeFirst.TicketType { Name = "Error Handling Defects" },
                  new Models.CodeFirst.TicketType { Name = "Improper Service Levels(Control flow defects)" },
                  new Models.CodeFirst.TicketType { Name = "Interpreting Data Defects" },
                  new Models.CodeFirst.TicketType { Name = "Race Conditions(Compatibility and Intersystem defects)" },
                  new Models.CodeFirst.TicketType { Name = "Load Conditions(Memory Leakages under load)" },
                  new Models.CodeFirst.TicketType { Name = "Hardware Failures" }
                );

            context.TicketStatuses.AddOrUpdate(
                  p => p.Name,
                  new Models.CodeFirst.TicketStatus { Name = "Pending" },
                  new Models.CodeFirst.TicketStatus { Name = "Open" },
                  new Models.CodeFirst.TicketStatus { Name = "Resolved" }

                );

        }  
    }
}
