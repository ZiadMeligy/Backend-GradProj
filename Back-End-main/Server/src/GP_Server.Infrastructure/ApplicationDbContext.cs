using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using GP_Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace GP_Server.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    #region Users
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Role> ApplicationRoles { get; set; }
    #endregion

    #region Studies
    public DbSet<Study> Studies { get; set; }
    #endregion

}
