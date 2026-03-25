using AutomobileServiceCenter_NguyenPhuLuong.Interfaces;

namespace AutomobileServiceCenter_NguyenPhuLuong.Services
{
    public class EmailService : IEmailService
    {
        public string Send(string to, string subject)
        {
            return $"Email sent to {to} with subject {subject}";
        }
    }
}