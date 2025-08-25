using System.Runtime;

namespace OrangeHrmApi.Models.DTOs
{
    public class AddEmployeeRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public JobInfo Job { get; set; } = new();
    }

}
