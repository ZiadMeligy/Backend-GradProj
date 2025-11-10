using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace GP_Server.Domain.Entities;

public class Image
    {
        public int Id { get; set; }
        [NotMapped]
        public IFormFile File { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FilePath { get; set; } = string.Empty;

    }
