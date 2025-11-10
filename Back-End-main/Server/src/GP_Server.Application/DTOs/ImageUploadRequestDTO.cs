using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GP_Server.Application.DTOs;

public class ImageUploadRequestDTO
{
    [Required]
    public IFormFile File { get; set; } = null!;
}
