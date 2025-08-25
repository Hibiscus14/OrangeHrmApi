namespace OrangeHrmApi.Models.DTOs
{
    public class CreateClaimRequest
    {
        public string? EmployeeName { get; set; }
        public string? EmployeeId { get; set; }
        public string Event { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }
}
