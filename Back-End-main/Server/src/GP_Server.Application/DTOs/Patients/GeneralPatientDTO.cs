using System;
using GP_Server.Application.DTOs.Users;

namespace GP_Server.Application.DTOs.Patients;

public class GeneralPatientDTO : PatientDTO
{
    public Guid Id { get; set; }
    public GeneralUserDTO Creator { get; set; } = new GeneralUserDTO();
    public DateTime CreatedAt { get; set; }
}
