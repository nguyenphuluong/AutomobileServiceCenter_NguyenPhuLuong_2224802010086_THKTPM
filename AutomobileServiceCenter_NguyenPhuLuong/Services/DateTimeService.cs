using AutomobileServiceCenter_NguyenPhuLuong.Interfaces;

namespace AutomobileServiceCenter_NguyenPhuLuong.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }
    }
}