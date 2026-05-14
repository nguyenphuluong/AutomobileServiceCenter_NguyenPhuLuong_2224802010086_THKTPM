using ASC.Model.BaseTypes;
using System.ComponentModel.DataAnnotations;

namespace ASC.Model.Models
{
    public class ServiceRequestMessage : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string ServiceRequestId { get; set; }

        public string UserName { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}