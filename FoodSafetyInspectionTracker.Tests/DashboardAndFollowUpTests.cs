using FluentAssertions;
using FoodSafetyInspectionTracker.Data;
using FoodSafetyInspectionTracker.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                new Premises { Name = "A", Address = "1", Town = "Dublin", RiskRating = "High" },
                new Premises { Name = "B", Address = "2", Town = "Cork", RiskRating = "Low" }
            );
            context.SaveChanges();

            context.Premises.Count().Should().Be(2);
        }

        [Fact]
        public void CanFilterPremisesByTown()
        {
            using var context = GetDbContext();

            context.Premises.AddRange(
                new Premises { Name = "A", Address = "1", Town = "Dublin", RiskRating = "High" },
                new Premises { Name = "B", Address = "2", Town = "Cork", RiskRating = "Low" }
            );
            context.SaveChanges();

            var dublinCount = context.Premises.Count(p => p.Town == "Dublin");

            dublinCount.Should().Be(1);
        }

        [Fact]
        public void OpenFollowUpCanBeDetected()
        {
            var followUp = new FollowUp
            {
                DueDate = DateTime.Now.AddDays(2),
                Status = "Open"
            };

            followUp.Status.Should().Be("Open");
        }

        [Fact]
        public void OverdueFollowUpCanBeDetected()
        {
            var followUp = new FollowUp
            {
                DueDate = DateTime.Now.AddDays(-2),
                Status = "Open"
            };

            var isOverdue = followUp.Status == "Open" && followUp.DueDate < DateTime.Now;

            isOverdue.Should().BeTrue();
        }
    }
}