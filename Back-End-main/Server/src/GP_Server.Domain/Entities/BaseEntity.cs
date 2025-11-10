using System;

namespace GP_Server.Domain.Entities;

public class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }

    public BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
