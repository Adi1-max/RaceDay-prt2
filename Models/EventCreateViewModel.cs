using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay222MVC.Models
{
    public class EventCreateViewModel
    {
        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(100, ErrorMessage = "Event name cannot exceed 100 characters.")]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;


    [Required(ErrorMessage = "Event description is required.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Event location is required.")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;
    }


}
