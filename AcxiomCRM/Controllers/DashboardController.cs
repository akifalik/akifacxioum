using System.Security.Claims;
using System.Text.Json;
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Controllers;

public class DashboardController : Controller {
  private readonly AppDbContext _db; public DashboardController(AppDbContext db) => _db = db;
  public async Task<IActionResult> Index() {
    var uid = User.FindFirstValue(ClaimTypes.NameIdentifier); var sales = User.IsInRole(Roles.Sales);
    var leads = _db.Leads.AsQueryable(); var opps = _db.Opportunities.AsQueryable();
    if (sales) { leads = leads.Where(l => l.AssignedTo == uid); opps = opps.Where(o => o.AssignedTo == uid); }
    var closed = new[] { "Lost", "Converted", "Unqualified" };
    ViewBag.Customers = await _db.Customers.For(User).CountAsync();
    ViewBag.Leads = await leads.CountAsync();
    ViewBag.OpenLeads = await leads.CountAsync(l => !closed.Contains(l.Status));
    ViewBag.Opps = await opps.CountAsync();
    ViewBag.OpenOpps = await opps.CountAsync(o => o.Stage != "Won" && o.Stage != "Lost");
    ViewBag.Won = await opps.CountAsync(o => o.Stage == "Won");
    ViewBag.Lost = await opps.CountAsync(o => o.Stage == "Lost");
    ViewBag.Pipeline = await opps.Where(o => o.Stage != "Won" && o.Stage != "Lost").SumAsync(o => (decimal?)o.Amount) ?? 0;
    var ls = await leads.GroupBy(l => l.Status).Select(g => new { k = g.Key, v = g.Count() }).ToListAsync();
    var os = await opps.GroupBy(o => o.Stage).Select(g => new { k = g.Key, v = g.Count() }).ToListAsync();
    var from = DateTime.Today.AddMonths(-5); from = new DateTime(from.Year, from.Month, 1);
    var ms = (await opps.Where(o => o.Stage == "Won" && o.ExpectedCloseDate >= from).ToListAsync())
      .GroupBy(o => o.ExpectedCloseDate.ToString("yyyy-MM")).OrderBy(g => g.Key).Select(g => new { k = g.Key, v = g.Sum(o => o.Amount) }).ToList();
    ViewBag.LeadChart = JsonSerializer.Serialize(new { labels = Rules.LeadStatuses, data = Rules.LeadStatuses.Select(s => ls.FirstOrDefault(x => x.k == s)?.v ?? 0) });
    ViewBag.OppChart = JsonSerializer.Serialize(new { labels = Rules.Stages, data = Rules.Stages.Select(s => os.FirstOrDefault(x => x.k == s)?.v ?? 0) });
    ViewBag.SalesChart = JsonSerializer.Serialize(new { labels = ms.Select(x => x.k), data = ms.Select(x => x.v) });
    return View();
  }
}
