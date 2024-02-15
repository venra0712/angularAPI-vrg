using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Account
    {
        public Guid AccountId { get; set; }
        public string EmployeeName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime DateCreated { get; set; }
    }
}
