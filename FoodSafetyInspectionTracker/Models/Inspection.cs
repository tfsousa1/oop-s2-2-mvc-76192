using System.ComponentModel.DataAnnotations;

namespace FoodSafetyInspectionTracker.Models
{
    public class Inspection
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Premises")]
        public int PremisesId { get; set; }

        // Navigation property used to access the related premises details.
        public Premises? Premises { get; set; }

        [Required]
        [Display(Name = "Inspection Date")]
        [DataType(DataType.Date)]
        public DateTime InspectionDate { get; set; }

        [Range(0, 100)]
        public int Score { get; set; }

        [Required]
        [StringLength(20)]
        public string Outcome { get; set; } = "";

        [StringLength(500)]
        public string Notes { get; set; } = "";

        // One inspection can have many follow-up actions.
        public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    }
}