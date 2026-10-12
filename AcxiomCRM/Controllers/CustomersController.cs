using System.Security.Claims;
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Controllers;

public static class Scope {
  public static IQueryable<Customer> For(this IQueryable<Customer> q, ClaimsPrincipal u) =>
    u.IsInRole(Roles.Sales) ? q.Where(c => c.CreatedBy == u.FindFirstValue(ClaimTypes.NameIdentifier)) : q;
}

public class CustomersController : Controller {
  private readonly AppDbContext _db; private readonly IAuditService _audit;
  public CustomersController(AppDbContext db, IAuditService a) { _db = db; _audit = a; }

  public async Task<IActionResult> Index(string? q, int page = 1) {
    var query = _db.Customers.For(User).AsQueryable();
    if (!string.IsNullOrWhiteSpace(q))
      query = query.Where(c => c.CustomerName.Contains(q) || c.Email.Contains(q) || c.Phone.Contains(q) || (c.CompanyName ?? "").Contains(q));
    return View(await query.OrderByDescending(c => c.CreatedDate).Skip((page-1)*20).Take(20).ToListAsync());
  }
  public IActionResult Create() => View(new Customer());

  [HttpPost]
  public async Task<IActionResult> Create(Customer m) {
    if (await _db.Customers.AnyAsync(c => c.Email == m.Email)) ModelState.AddModelError(nameof(m.Email), "A customer with this email already exists.");
    if (await _db.Customers.AnyAsync(c => c.Phone == m.Phone)) ModelState.AddModelError(nameof(m.Phone), "A customer with this phone already exists.");
    if (!ModelState.IsValid) return View(m);
    m.CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
    m.CustomerCode = "CUS-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
    m.CreatedDate = DateTime.UtcNow;
    _db.Add(m); await _db.SaveChangesAsync();
    await _audit.LogAsync("Create", nameof(Customer), m.CustomerId.ToString(), null, $"{m.CustomerCode}|{m.CustomerName}");
    return RedirectToAction(nameof(Index));
  }
  [HttpPost, Authorize(Roles = "Admin,Manager")]
  public async Task<IActionResult> Delete(int id) {
    var c = await _db.Customers.FindAsync(id); if (c == null) return NotFound();
    var old = c.Status; c.Status = "Inactive"; await _db.SaveChangesAsync();
    await _audit.LogAsync("Delete", nameof(Customer), id.ToString(), old, c.Status);
    return RedirectToAction(nameof(Index));
  }
}
