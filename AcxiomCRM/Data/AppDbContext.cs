using AcxiomCRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser> {
  public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) {}
  public DbSet<Customer> Customers => Set<Customer>();
  public DbSet<Lead> Leads => Set<Lead>();
  public DbSet<Opportunity> Opportunities => Set<Opportunity>();
  public DbSet<FollowUp> FollowUps => Set<FollowUp>();
  public DbSet<Activity> Activities => Set<Activity>();
  public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
  protected override void OnModelCreating(ModelBuilder m) {
    base.OnModelCreating(m);
    m.Entity<Customer>().HasIndex(c => c.Email).IsUnique();
    m.Entity<Customer>().HasIndex(c => c.Phone).IsUnique();
    m.Entity<Opportunity>().Property(o => o.Amount).HasPrecision(18,2);
    m.Entity<Lead>().Property(l => l.ExpectedValue).HasPrecision(18,2);
  }
}
public static class Seeder {
  public static async Task RunAsync(IServiceProvider sp, IConfiguration cfg) {
    var rm = sp.GetRequiredService<RoleManager<IdentityRole>>();
    var um = sp.GetRequiredService<UserManager<ApplicationUser>>();
    foreach (var r in new[]{Roles.Admin, Roles.Manager, Roles.Sales})
      if (!await rm.RoleExistsAsync(r)) await rm.CreateAsync(new IdentityRole(r));
    var email = cfg["Seed:AdminEmail"]!;
    if (await um.FindByEmailAsync(email) == null) {
      var u = new ApplicationUser { UserName = email, Email = email, FullName = "Administrator", EmailConfirmed = true };
      if ((await um.CreateAsync(u, cfg["Seed:AdminPassword"]!)).Succeeded) await um.AddToRoleAsync(u, Roles.Admin);
    }
  }
}
