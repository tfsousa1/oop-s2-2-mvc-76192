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

            // Create the application roles required by the assessment brief.
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create default users for each role so the lecturer can test access levels quickly.
            await CreateUserIfMissing(userManager, "admin@foodsafety.local", "CouncilAdmin25!", "Admin");
            await CreateUserIfMissing(userManager, "inspector@foodsafety.local", "InspectSafe25!", "Inspector");
            await CreateUserIfMissing(userManager, "viewer@foodsafety.local", "ViewOnly25!", "Viewer");

            // Stop here if the database already contains premises data.
            if (await context.Premises.AnyAsync())
            {
                return;
            }

            var premisesList = new List<Premises>
            {
                new Premises { Name = "Cafe Central", Address = "12 Main Street", Town = "Dublin", RiskRating = "High" },
                new Premises { Name = "Green Bites", Address = "44 River Road", Town = "Dublin", RiskRating = "Medium" },
                new Premises { Name = "Sunrise Cafe", Address = "9 Church Lane", Town = "Dublin", RiskRating = "Low" },
                new Premises { Name = "Temple Grill", Address = "101 College Avenue", Town = "Dublin", RiskRating = "High" },

                new Premises { Name = "Fresh Market", Address = "3 Market Square", Town = "Cork", RiskRating = "Low" },
                new Premises { Name = "Harbour Foods", Address = "27 Dock Road", Town = "Cork", RiskRating = "Medium" },
                new Premises { Name = "Lee Deli", Address = "18 Patrick Street", Town = "Cork", RiskRating = "High" },
                new Premises { Name = "Cork Corner Cafe", Address = "6 Washington Lane", Town = "Cork", RiskRating = "Medium" },

                new Premises { Name = "Urban Kitchen", Address = "7 Bridge Street", Town = "Galway", RiskRating = "High" },
                new Premises { Name = "Atlantic Eats", Address = "25 Quay Road", Town = "Galway", RiskRating = "Medium" },
                new Premises { Name = "Bay Bakery", Address = "14 Sea View", Town = "Galway", RiskRating = "Low" },
                new Premises { Name = "West End Bistro", Address = "31 Shop Street", Town = "Galway", RiskRating = "High" }
            };

            context.Premises.AddRange(premisesList);
            await context.SaveChangesAsync();

            var now = DateTime.Now;

            var inspections = new List<Inspection>
            {
                new Inspection { PremisesId = premisesList[0].Id, InspectionDate = now.AddDays(-2), Score = 58, Outcome = "Fail", Notes = "Poor surface sanitising in food prep area." },
                new Inspection { PremisesId = premisesList[1].Id, InspectionDate = now.AddDays(-4), Score = 81, Outcome = "Pass", Notes = "Generally compliant with minor storage advice." },
                new Inspection { PremisesId = premisesList[2].Id, InspectionDate = now.AddDays(-6), Score = 90, Outcome = "Pass", Notes = "Good hygiene practices observed." },
                new Inspection { PremisesId = premisesList[3].Id, InspectionDate = now.AddDays(-8), Score = 49, Outcome = "Fail", Notes = "Cross-contamination risk identified." },

                new Inspection { PremisesId = premisesList[4].Id, InspectionDate = now.AddDays(-10), Score = 76, Outcome = "Pass", Notes = "Records available and acceptable." },
                new Inspection { PremisesId = premisesList[5].Id, InspectionDate = now.AddDays(-12), Score = 63, Outcome = "Fail", Notes = "Chilled storage temperature exceeded limits." },
                new Inspection { PremisesId = premisesList[6].Id, InspectionDate = now.AddDays(-14), Score = 71, Outcome = "Pass", Notes = "Cleaning schedule needs improvement." },
                new Inspection { PremisesId = premisesList[7].Id, InspectionDate = now.AddDays(-16), Score = 84, Outcome = "Pass", Notes = "Well-managed kitchen operations." },

                new Inspection { PremisesId = premisesList[8].Id, InspectionDate = now.AddDays(-18), Score = 52, Outcome = "Fail", Notes = "Hand washing compliance was weak." },
                new Inspection { PremisesId = premisesList[9].Id, InspectionDate = now.AddDays(-20), Score = 79, Outcome = "Pass", Notes = "Satisfactory inspection overall." },
                new Inspection { PremisesId = premisesList[10].Id, InspectionDate = now.AddDays(-22), Score = 88, Outcome = "Pass", Notes = "No major issues found." },
                new Inspection { PremisesId = premisesList[11].Id, InspectionDate = now.AddDays(-24), Score = 60, Outcome = "Fail", Notes = "Pest prevention controls need attention." },

                new Inspection { PremisesId = premisesList[0].Id, InspectionDate = now.AddDays(-26), Score = 73, Outcome = "Pass", Notes = "Improvement from previous inspection." },
                new Inspection { PremisesId = premisesList[1].Id, InspectionDate = now.AddDays(-28), Score = 68, Outcome = "Fail", Notes = "Food labelling incomplete." },
                new Inspection { PremisesId = premisesList[2].Id, InspectionDate = now.AddDays(-30), Score = 92, Outcome = "Pass", Notes = "Excellent housekeeping and records." },
                new Inspection { PremisesId = premisesList[3].Id, InspectionDate = now.AddDays(-32), Score = 55, Outcome = "Fail", Notes = "Raw and cooked foods stored too closely." },

                new Inspection { PremisesId = premisesList[4].Id, InspectionDate = now.AddDays(-34), Score = 80, Outcome = "Pass", Notes = "Routine inspection passed." },
                new Inspection { PremisesId = premisesList[5].Id, InspectionDate = now.AddDays(-36), Score = 61, Outcome = "Fail", Notes = "Cleaning materials not stored correctly." },
                new Inspection { PremisesId = premisesList[6].Id, InspectionDate = now.AddDays(-38), Score = 77, Outcome = "Pass", Notes = "Minor paperwork issues only." },
                new Inspection { PremisesId = premisesList[7].Id, InspectionDate = now.AddDays(-40), Score = 86, Outcome = "Pass", Notes = "Staff training records complete." },

                new Inspection { PremisesId = premisesList[8].Id, InspectionDate = now.AddDays(-42), Score = 57, Outcome = "Fail", Notes = "Cleaning checks not signed off." },
                new Inspection { PremisesId = premisesList[9].Id, InspectionDate = now.AddDays(-44), Score = 82, Outcome = "Pass", Notes = "Premises in satisfactory condition." },
                new Inspection { PremisesId = premisesList[10].Id, InspectionDate = now.AddDays(-46), Score = 89, Outcome = "Pass", Notes = "Strong compliance shown." },
                new Inspection { PremisesId = premisesList[11].Id, InspectionDate = now.AddDays(-48), Score = 53, Outcome = "Fail", Notes = "Waste disposal area poorly maintained." },

                new Inspection { PremisesId = premisesList[6].Id, InspectionDate = now.AddDays(-50), Score = 74, Outcome = "Pass", Notes = "Acceptable standards with advice given." }
            };

            context.Inspections.AddRange(inspections);
            await context.SaveChangesAsync();

            var followUps = new List<FollowUp>
            {
                new FollowUp { InspectionId = inspections[0].Id, DueDate = now.AddDays(5), Status = "Open" },
                new FollowUp { InspectionId = inspections[3].Id, DueDate = now.AddDays(-1), Status = "Open" },
                new FollowUp { InspectionId = inspections[5].Id, DueDate = now.AddDays(7), Status = "Open" },
                new FollowUp { InspectionId = inspections[8].Id, DueDate = now.AddDays(-3), Status = "Open" },
                new FollowUp { InspectionId = inspections[11].Id, DueDate = now.AddDays(4), Status = "Open" },

                new FollowUp { InspectionId = inspections[13].Id, DueDate = now.AddDays(-10), Status = "Closed", ClosedDate = now.AddDays(-7) },
                new FollowUp { InspectionId = inspections[15].Id, DueDate = now.AddDays(-8), Status = "Closed", ClosedDate = now.AddDays(-6) },
                new FollowUp { InspectionId = inspections[17].Id, DueDate = now.AddDays(6), Status = "Open" },
                new FollowUp { InspectionId = inspections[20].Id, DueDate = now.AddDays(-4), Status = "Open" },
                new FollowUp { InspectionId = inspections[23].Id, DueDate = now.AddDays(-12), Status = "Closed", ClosedDate = now.AddDays(-9) }
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

            // Assign the correct role if the user exists but is not yet linked to it.
            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}