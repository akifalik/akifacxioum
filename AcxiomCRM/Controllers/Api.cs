using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Controllers;

public record CustomerDto(int Id, string Code, string Name, string Email, string Phone, string? Company, string Status);
public class CustomerInput {
  [Required, StringLength(100)] public string Name { get; set; } = "";
  [Required, EmailAddress] public string Email { get; set; } = "";
  [Required, RegularExpression(Rules.PhoneRegex, ErrorMessage = "Enter a valid phone number.")] public string Phone { get; set; } = "";
  public string? Company { get; set; }
}

// NOTE: uses the Identity cookie. Anti-forgery is skipped because API clients can't send MVC tokens;
// for browser-facing use, switch /api to JWT bearer auth to avoid CSRF exposure.
[ApiController, Route("api/customers"), Authorize, IgnoreAntiforgeryToken]
public class CustomersApiController : ControllerBase {
  private readonly AppDbContext _db; private readonly IAuditService _audit;
  public CustomersApiController(AppDbContext db, IAuditService a) { _db = db; _audit = a; }
  static CustomerDto Map(Customer c) => new(c.CustomerId, c.CustomerCode, c.CustomerName, c.Email, c.Phone, c.CompanyName, c.Status);

  [HttpGet] public async Task<ActionResult<IEnumerable<CustomerDto>>> Get() =>
    Ok((await _db.Customers.For(User).Take(200).ToListAsync()).Select(Map));

  [HttpGet("{id}")] public async Task<ActionResult<CustomerDto>> Get(int id) {
    var c = await _db.Customers.For(User).FirstOrDefaultAsync(x => x.CustomerId == id);
    return c == null ? NotFound() : Map(c);
  }
  [HttpPost] public async Task<ActionResult<CustomerDto>> Post(CustomerInput i) {
    if (await _db.Customers.AnyAsync(c => c.Email == i.Email || c.Phone == i.Phone)) return Conflict(new { error = "Customer already exists." });
    var c = new Customer { CustomerName = i.Name, Email = i.Email, Phone = i.Phone, CompanyName = i.Company,
      CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier), CustomerCode = "CUS-" + Guid.NewGuid().ToString("N")[..8].ToUpper() };
    _db.Add(c); await _db.SaveChangesAsync();
    await _audit.LogAsync("Create", nameof(Customer), c.CustomerId.ToString(), null, c.CustomerCode);
    return CreatedAtAction(nameof(Get), new { id = c.CustomerId }, Map(c));
  }
  [HttpPut("{id}")] public async Task<IActionResult> Put(int id, CustomerInput i) {
    var c = await _db.Customers.For(User).FirstOrDefaultAsync(x => x.CustomerId == id); if (c == null) return NotFound();
    if (await _db.Customers.AnyAsync(x => x.CustomerId != id && (x.Email == i.Email || x.Phone == i.Phone))) return Conflict(new { error = "Email or phone in use." });
    var old = $"{c.CustomerName}|{c.Email}|{c.Phone}";
    c.CustomerName = i.Name; c.Email = i.Email; c.Phone = i.Phone; c.CompanyName = i.Company;
    await _db.SaveChangesAsync(); await _audit.LogAsync("Update", nameof(Customer), id.ToString(), old, $"{c.CustomerName}|{c.Email}|{c.Phone}");
    return NoContent();
  }
  [HttpDelete("{id}"), Authorize(Roles = "Admin,Manager")] public async Task<IActionResult> Delete(int id) {
    var c = await _db.Customers.FindAsync(id); if (c == null) return NotFound();
    c.Status = "Inactive"; await _db.SaveChangesAsync(); await _audit.LogAsync("Delete", nameof(Customer), id.ToString()); return NoContent();
  }
}
