using System;

namespace CreditReporting.Application.DTOs
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public string? AuthUserName { get; set; }
        public string? Email { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string AadharNo { get; set; } 
        public string PanNo { get; set; }
        public DateTime Dob { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public decimal MonthlyIncome { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
