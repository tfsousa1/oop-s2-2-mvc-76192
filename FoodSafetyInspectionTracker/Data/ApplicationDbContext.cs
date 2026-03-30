using FoodSafetyInspectionTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspectionTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Premises> Premises { get; set; }
        public DbSet<Inspection> Inspections { get; set; }
        public DbSet<FollowUp> FollowUps { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // One premises can have many inspections.
            builder.Entity<Inspection>()
                .HasOne(i => i.Premises)
                .WithMany(p => p.Inspections)
                .HasForeignKey(i => i.PremisesId)
                .OnDelete(DeleteBehavior.Cascade);

            // One inspection can have many follow-ups.
            builder.Entity<FollowUp>()
                .HasOne(f => f.Inspection)
                .WithMany(i => i.FollowUps)
                .HasForeignKey(f => f.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}