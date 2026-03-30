namespace FoodSafetyInspectionTracker.Models
{
    public class DashboardViewModel
    {
        public int TotalPremises { get; set; }
        public int TotalInspections { get; set; }
        public int InspectionsThisMonth { get; set; }
        public int FailedInspectionsThisMonth { get; set; }
        public int OpenFollowUps { get; set; }
        public int OverdueFollowUps { get; set; }

        public string? SelectedTown { get; set; }
        public string? SelectedRiskRating { get; set; }

        public List<string> Towns { get; set; } = new();
        public List<string> RiskRatings { get; set; } = new();

        public List<Inspection> RecentInspections { get; set; } = new();
    }
}