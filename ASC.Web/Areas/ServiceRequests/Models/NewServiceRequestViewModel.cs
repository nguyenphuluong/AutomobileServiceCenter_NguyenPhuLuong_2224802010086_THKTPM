using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Areas.ServiceRequests.Models
{
    public class NewServiceRequestViewModel
    {
        [Required(ErrorMessage = "The Vehicle Name field is required.")]
        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Vehicle Type field is required.")]
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Requested Services field is required.")]
        [Display(Name = "Requested Services")]
        public string RequestedServices { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Requested Date field is required.")]
        [Display(Name = "Requested Date")]
        public string RequestedDate { get; set; } = string.Empty;
    }
}