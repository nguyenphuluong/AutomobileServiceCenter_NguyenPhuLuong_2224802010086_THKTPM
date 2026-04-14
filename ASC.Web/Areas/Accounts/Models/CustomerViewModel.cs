namespace ASC.Web.Areas.Accounts.Models
{
    public class CustomerItemViewModel
    {
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CustomerViewModel
    {
        public List<CustomerItemViewModel> Customers { get; set; } = new();

        public CustomerRegistrationViewModel Registration { get; set; } = new CustomerRegistrationViewModel();
    }
}