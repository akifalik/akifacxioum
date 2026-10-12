using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var b = WebApplication.CreateBuilder(args);
b.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(b.Configuration.GetConnectionString("Default")));
b.Services.AddIdentity<ApplicationUser, IdentityRole>(o => {
  o.Password.RequiredLength = 8; o.Password.RequireDigit = true; o.Password.RequireUppercase = true;
  o.Password.RequireLowercase = true; o.Password.RequireNonAlphanumeric = true;
  o.Lockout.MaxFailedAccessAttempts = 5; o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
  o.Lockout.AllowedForNewUsers = true; o.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

b.Services.ConfigureApplicationCookie(o => {
  o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/Denied";
  o.Cookie.HttpOnly = true; o.Cookie.SecurePolicy = CookieSecurePolicy.Always; o.SlidingExpiration = true;
  o.ExpireTimeSpan = TimeSpan.FromMinutes(30);
  o.Events.OnRedirectToLogin = c => { if (c.Request.Path.StartsWithSegments("/api")) c.Response.StatusCode = 401; else c.Response.Redirect(c.RedirectUri); return Task.CompletedTask; };
  o.Events.OnRedirectToAccessDenied = c => { if (c.Request.Path.StartsWithSegments("/api")) c.Response.StatusCode = 403; else c.Response.Redirect(c.RedirectUri); return Task.CompletedTask; };
});
b.Services.AddHttpContextAccessor();
b.Services.AddScoped<IAuditService, AuditService>();
b.Services.AddControllersWithViews(o => {
  o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
  o.Filters.Add(new AuthorizeFilter()); // every page requires login unless [AllowAnonymous]
});

var app = b.Build();
using (var s = app.Services.CreateScope()) {
  await s.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
  await Seeder.RunAsync(s.ServiceProvider, app.Configuration);
}
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting();
app.UseAuthentication(); app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Dashboard}/{action=Index}/{id?}");
app.Run();
