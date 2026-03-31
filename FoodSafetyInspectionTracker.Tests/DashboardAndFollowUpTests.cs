using FluentAssertions;
using FoodSafetyInspectionTracker.Data;
using FoodSafetyInspectionTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspectionTracker.Tests
{
    public class DashboardAndFollowUpTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void CanCountTotalPremises()
        {
            using var context = GetDbContext();

            context.Premises.AddRange(
                new Premises { Name = "Cafe Central", Address = "1 Main Street", Town = "Dublin", RiskRating = "High" },
                new Premises { Name = "Fresh Market", Address = "2 River Road", Town = "Cork", RiskRating = "Low" }
            );

            context.SaveChanges();

            var totalPremises = context.Premises.Count();

            totalPremises.Should().Be(2);
        }

        [Fact]
        public void CanFilterPremisesByTown()
        {
            using var context = GetDbContext();

            context.Premises.AddRange(
                new Premises { Name = "Cafe Central", Address = "1 Main Street", Town = "Dublin", RiskRating = "High" },
                new Premises { Name = "Green Bites", Address = "2 Bridge Street", Town = "Dublin", RiskRating = "Medium" },
                new Premises { Name = "Harbour Foods", Address = "3 Dock Road", Town = "Cork", RiskRating = "Low" }
            );

            context.SaveChanges();

            var dublinPremises = context.Premises.Where(p => p.Town == "Dublin").ToList();

            dublinPremises.Should().HaveCount(2);
        }

        [Fact]
        public void CanCountInspectionsThisMonth()
        {
            using var context = GetDbContext();

            var premises = new Premises
            {
                Name = "Urban Kitchen",
                Address = "7 Bridge Street",
                Town = "Galway",
                RiskRating = "High"
            };

            context.Premises.Add(premises);
            context.SaveChanges();

            var firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            context.Inspections.AddRange(
                new Inspection
                {
                    PremisesId = premises.Id,
                    InspectionDate = firstDayOfMonth.AddDays(1),
                    Score = 82,
                    Outcome = "Pass",
                    Notes = "Good overall hygiene."
                },
                new Inspection
                {
                    PremisesId = premises.Id,
                    InspectionDate = firstDayOfMonth.AddDays(3),
                    Score = 58,
                    Outcome = "Fail",
                    Notes = "Temperature issue found."
                },
                new Inspection
                {
                    PremisesId = premises.Id,
                    InspectionDate = firstDayOfMonth.AddMonths(-1).AddDays(10),
                    Score = 75,
                    Outcome = "Pass",
                    Notes = "Older inspection outside this month."
                }
            );

            context.SaveChanges();

            var inspectionsThisMonth = context.Inspections.Count(i => i.InspectionDate >= firstDayOfMonth);

            inspectionsThisMonth.Should().Be(2);
        }

        [Fact]
        public void CanCountFailedInspectionsThisMonth()
        {
            using var context = GetDbContext();

            var premises = new Premises
            {
                Name = "Temple Grill",
                Address = "10 College Avenue",
                Town = "Dublin",
                RiskRating = "High"
            };

            context.Premises.Add(premises);
            context.SaveChanges();

            var firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            context.Inspections.AddRange(
                new Inspection
                {
                    PremisesId = premises.Id,
                    InspectionDate = firstDayOfMonth.AddDays(2),
                    Score = 55,
                    Outcome = "Fail",
                    Notes = "Cross-contamination risk."
                },
                new Inspection
                {
                    PremisesId = premises.Id,
                    InspectionDate = firstDayOfMonth.AddDays(4),
                    Score = 88,
                    Outcome = "Pass",
                    Notes = "Strong compliance."
                },
                new Inspection
                {
                    PremisesId = premises.Id,
                    InspectionDate = firstDayOfMonth.AddDays(6),
                    Score = 61,
                    Outcome = "Fail",
                    Notes = "Cleaning issues."
                }
            );

            context.SaveChanges();

            var failedThisMonth = context.Inspections.Count(i =>
                i.InspectionDate >= firstDayOfMonth &&
                i.Outcome == "Fail");

            failedThisMonth.Should().Be(2);
        }

        [Fact]
        public void ClosedFollowUpRequiresClosedDate()
        {
            var followUp = new FollowUp
            {
                InspectionId = 1,
                DueDate = DateTime.Today.AddDays(-2),
                Status = "Closed",
                ClosedDate = null
            };

            var isValid = !(followUp.Status == "Closed" && followUp.ClosedDate == null);

            isValid.Should().BeFalse();
        }

        [Fact]
        public void OpenFollowUpCanBeDetected()
        {
            var followUp = new FollowUp
            {
                InspectionId = 1,
                DueDate = DateTime.Today.AddDays(2),
                Status = "Open"
            };

            followUp.Status.Should().Be("Open");
        }

        [Fact]
        public void OverdueFollowUpCanBeDetected()
        {
            var followUp = new FollowUp
            {
                InspectionId = 1,
                DueDate = DateTime.Today.AddDays(-2),
                Status = "Open"
            };

            var isOverdue = followUp.Status == "Open" && followUp.DueDate < DateTime.Today;

            isOverdue.Should().BeTrue();
        }
    }
}