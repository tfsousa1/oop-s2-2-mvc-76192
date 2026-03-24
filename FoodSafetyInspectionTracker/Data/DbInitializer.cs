using FoodSafetyInspectionTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspectionTracker.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager)
        {
            await context.Database.MigrateAsync();

            string[] roles = { "Admin", "Inspector", "Viewer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            await CreateUserIfMissing(userManager, "admin@foodsafety.local", "Admin123!", "Admin");
            await CreateUserIfMissing(userManager, "inspector@foodsafety.local", "Inspector123!", "Inspector");
            await CreateUserIfMissing(userManager, "viewer@foodsafety.local", "Viewer123!", "Viewer");

            if (await context.Premises.AnyAsync())
            {
                return;
            }

            var premisesList = new List<Premises>
            {
                new Premises { Name = "Cafe Central", Address = "Main Street", Town = "Dublin", RiskRating = "High" },
                new Premises { Name = "Green Bites", Address = "River Road", Town = "Dublin", RiskRating = "Medium" },
                new Premises { Name = "Fresh Market", Address = "Market Square", Town = "Cork", RiskRating = "Low" },
                new Premises { Name = "Urban Kitchen", Address = "Bridge Street", Town = "Galway", RiskRating = "High" },
                new Premises { Name = "Harbour Foods", Address = "Dock Road", Town = "Cork", RiskRating = "Medium" },
                new Premises { Name = "Sunrise Cafe", Address = "Church Lane", Town = "Dublin", RiskRating = "Low" }
            };

            context.Premises.AddRange(premisesList);
            await context.SaveChangesAsync();

            var inspections = new List<Inspection>
            {
                new Inspection { PremisesId = premisesList[0].Id, InspectionDate = DateTime.Now.AddDays(-10), Score = 60, Outcome = "Fail", Notes = "Hygiene issues" },
                new Inspection { PremisesId = premisesList[1].Id, InspectionDate = DateTime.Now.AddDays(-5), Score = 85, Outcome = "Pass", Notes = "Minor issues" },
                new Inspection { PremisesId = premisesList[2].Id, InspectionDate = DateTime.Now.AddDays(-20), Score = 72, Outcome = "Pass", Notes = "Improvement required" },
                new Inspection { PremisesId = premisesList[3].Id, InspectionDate = DateTime.Now.AddDays(-3), Score = 55, Outcome = "Fail", Notes = "Temperature control issue" }
            };

            context.Inspections.AddRange(inspections);
            await context.SaveChangesAsync();

            var followUps = new List<FollowUp>
            {
                new FollowUp { InspectionId = inspections[0].Id, DueDate = DateTime.Now.AddDays(7), Status = "Open" },
                new FollowUp { InspectionId = inspections[3].Id, DueDate = DateTime.Now.AddDays(-2), Status = "Open" },
                new FollowUp { InspectionId = inspections[2].Id, DueDate = DateTime.Now.AddDays(-10), Status = "Closed", ClosedDate = DateTime.Now.AddDays(-5) }
            };

            context.FollowUps.AddRange(followUps);
            await context.SaveChangesAsync();
        }

        private static async Task CreateUserIfMissing(
            UserManager<IdentityUser> userManager,
            string email,
            string password,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create seeded user {email}: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}