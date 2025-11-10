using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GP_Server.Domain.Entities;

public class AuditableEntity: BaseEntity
{
    [ForeignKey(nameof(ApplicationUser))]
    public string CreatorId { get; set; } = string.Empty;
    public virtual ApplicationUser Creator { get; set; } = null!;
    public AuditableEntity() : base()
    {
    }

}
