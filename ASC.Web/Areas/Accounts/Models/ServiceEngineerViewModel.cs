namespace ASC.Web.Areas.Accounts.Models
{
    public class ServiceEngineerItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public bool Active { get; set; }
    }

    public class ServiceEngineerViewModel
    {
        public List<ServiceEngineerItemViewModel> ServiceEngineers { get; set; } = new();

        public ServiceEngineerRegistrationViewModel Registration { get; set; } = new();
    }
}