using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Areas.Accounts.Models
{
    public class ProfileModel
    {
        [Required]
        [Display(Name = "UserName")]
        public string UserName { get; set; } = string.Empty;
    }
}