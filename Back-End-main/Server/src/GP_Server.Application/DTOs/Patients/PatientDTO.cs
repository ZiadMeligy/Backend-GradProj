using System;
using System.ComponentModel.DataAnnotations;

namespace GP_Server.Application.DTOs.Patients;

public class PatientDTO
{
    public string Name { get; set; } = string.Empty;
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Phone]
    public string Phone { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    [RegularExpression("^(A\\+|A-|B\\+|B-|AB\\+|AB-|O\\+|O-)$", ErrorMessage = "Invalid Blood Type")]
    public string BloodType { get; set; } = string.Empty;

    [RegularExpression("^(Male|Female)$", ErrorMessage = "There are only two")]
    public string Gender { get; set; } = string.Empty;
}
