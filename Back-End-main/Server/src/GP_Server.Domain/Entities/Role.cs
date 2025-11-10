
using Microsoft.AspNetCore.Identity;
namespace GP_Server.Domain.Entities;

public class Role : IdentityRole
{
    public Role()
    {
    }
    public Role(string name) : base(name)
    {
    }

}
