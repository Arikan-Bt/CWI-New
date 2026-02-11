namespace CWI.Application.DTOs.Customers;

public class CustomerDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxOfficeName { get; set; }
    public string? TaxNumber { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsVendor { get; set; }
}
