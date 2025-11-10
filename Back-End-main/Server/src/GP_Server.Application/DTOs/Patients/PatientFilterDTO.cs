using System;

namespace GP_Server.Application.DTOs.Patients;

public class PatientFilterDTO
{
    public string? Name { get; set; }
    public string? BloodType { get; set; }
    public string? Gender { get; set; }

}
