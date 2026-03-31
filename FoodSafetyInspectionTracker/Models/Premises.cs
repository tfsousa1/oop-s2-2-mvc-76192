using System.ComponentModel.DataAnnotations;

namespace FoodSafetyInspectionTracker.Models
{
    public class Premises
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [Required]
        [StringLength(150)]
        public string Address { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Town { get; set; } = "";

        [Required]
        [Display(Name = "Risk Rating")]
        [StringLength(20)]
        public string RiskRating { get; set; } = "";

        // One premises can have many inspections.
        public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    }
}