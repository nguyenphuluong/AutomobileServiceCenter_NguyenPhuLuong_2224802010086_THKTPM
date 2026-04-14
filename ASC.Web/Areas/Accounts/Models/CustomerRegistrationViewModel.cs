using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Areas.Accounts.Models
{
    public class CustomerRegistrationViewModel
    {
        public string Email { get; set; } = string.Empty;

        public bool IsEdit { get; set; } = false;

        public bool IsActive { get; set; } = false;
    }
}