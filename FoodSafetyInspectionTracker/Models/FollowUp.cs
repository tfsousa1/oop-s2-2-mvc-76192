using System.ComponentModel.DataAnnotations;

namespace FoodSafetyInspectionTracker.Models
{
    public class FollowUp
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Inspection")]
        public int InspectionId { get; set; }

        // Navigation property used to access the inspection linked to this follow-up.
        public Inspection? Inspection { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "";

        [Display(Name = "Closed Date")]
        [DataType(DataType.Date)]
        public DateTime? ClosedDate { get; set; }
    }
}