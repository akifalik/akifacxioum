using System.Security.Claims;
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Controllers;

public class OpportunitiesController : Controller {
  private readonly AppDbContext _db; private readonly IAuditService _audit;
  public OpportunitiesController(AppDbContext db, IAuditService a) { _db = db; _audit = a; }
  private string Uid => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

  public async Task<IActionResult> Index(string? q, string? stage) {
    var query = _db.Opportunities.AsQueryable();
    if (User.IsInRole(Roles.Sales)) query = query.Where(o => o.AssignedTo == Uid);
    if (!string.IsNullOrWhiteSpace(q)) query = query.Where(o => o.OpportunityName.Contains(q));
    if (!string.IsNullOrWhiteSpace(stage)) query = query.Where(o => o.Stage == stage);
    return View(await query.OrderByDescending(o => o.CreatedDate).Take(100).ToListAsync());
  }
  public IActionResult Create() => View(new Opportunity { ExpectedCloseDate = DateTime.Today.AddDays(30) });

  [HttpPost]
  public async Task<IActionResult> Create(Opportunity m) {
    if (!await _db.Customers.AnyAsync(c => c.CustomerId == m.CustomerId)) ModelState.AddModelError(nameof(m.CustomerId), "Customer not found.");
    if (!ModelState.IsValid) return View(m);
    m.AssignedTo ??= Uid; m.Status = Rules.IsActiveStage(m.Stage) ? "Open" : m.Stage;
    _db.Add(m); await _db.SaveChangesAsync();
    await _audit.LogAsync("Create", nameof(Opportunity), m.OpportunityId.ToString(), null, $"{m.OpportunityName}|{m.Amount}|{m.Stage}");
    return RedirectToAction(nameof(Index));
  }
}
