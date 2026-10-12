using AcxiomCRM.Data;
using AcxiomCRM.Models;
using System.Security.Claims;
namespace AcxiomCRM.Services;

public interface IAuditService { Task LogAsync(string action, string entity, string? recordId = null, string? oldV = null, string? newV = null, string? userId = null); }
public class AuditService : IAuditService {
  private readonly AppDbContext _db; private readonly IHttpContextAccessor _ctx;
  public AuditService(AppDbContext db, IHttpContextAccessor ctx) { _db = db; _ctx = ctx; }
  public async Task LogAsync(string action, string entity, string? recordId = null, string? oldV = null, string? newV = null, string? userId = null) {
    var h = _ctx.HttpContext;
    _db.AuditLogs.Add(new AuditLog {
      UserId = userId ?? h?.User.FindFirstValue(ClaimTypes.NameIdentifier),
      Action = action, EntityName = entity, RecordId = recordId, OldValue = oldV, NewValue = newV,
      IpAddress = h?.Connection.RemoteIpAddress?.ToString() });
    await _db.SaveChangesAsync(); // never pass passwords/tokens in oldV/newV
  }
}
