using FoodSafetyInspectionTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace FoodSafetyInspectionTracker.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            string[] roles = { "Admin", "Inspector", "Viewer" };

            foreach (var role in roles)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    roleManager.CreateAsync(new IdentityRole(role)).Wait();
                }
            }

            if (context.Premises.Any())
            {
                return;
            }

            var premisesList = new List<Premises>
            {
                new Premises { Name = "Cafe Central", Address = "Main Street", Town = "Dublin", RiskRating = "High" },
                new Premises { Name = "Green Bites", Address = "River Road", Town = "Dublin", RiskRating = "Medium" },
                new Premises { Name = "Fresh Market", Address = "Market Square", Town = "Cork", RiskRating = "Low" }
            };

            context.Premises.AddRange(premisesList);
            context.SaveChanges();

            var inspections = new List<Inspection>
            {
                new Inspection
                {
                    PremisesId = premisesList[0].Id,
                    InspectionDate = DateTime.Now.AddDays(-10),
                    Score = 60,
                    Outcome = "Fail",
                    Notes = "Hygiene issues"
                },
                new Inspection
                {
                    PremisesId = premisesList[1].Id,
                    InspectionDate = DateTime.Now.AddDays(-5),
                    Score = 85,
                    Outcome = "Pass",
                    Notes = "Minor issues"
                }
            };

            context.Inspections.AddRange(inspections);
            context.SaveChanges();

            var followUps = new List<FollowUp>
            {
                new FollowUp
                {
                    InspectionId = inspections[0].Id,
                    DueDate = DateTime.Now.AddDays(7),
                    Status = "Open"
                }
            };

            context.FollowUps.AddRange(followUps);
            context.SaveChanges();
        }
    }
}