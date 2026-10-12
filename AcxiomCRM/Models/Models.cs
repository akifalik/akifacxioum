using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace AcxiomCRM.Models;

public static class Roles { public const string Admin="Admin", Manager="Manager", Sales="SalesExecutive"; }
public static class Rules {
  public const string PhoneRegex = @"^[6-9]\d{9}$";
  public static readonly string[] LeadStatuses = { "New","Contacted","Qualified","Unqualified","Converted","Lost" };
  public static readonly string[] Stages = { "Qualification","Proposal","Negotiation","Won","Lost" };
  public static bool IsActiveStage(string s) => s is not ("Won" or "Lost");
}
public class ApplicationUser : IdentityUser { [StringLength(100)] public string FullName { get; set; } = ""; public bool IsActive { get; set; } = true; }

public class Customer {
  public int CustomerId { get; set; }
  [StringLength(20)] public string CustomerCode { get; set; } = "";
  [Required, StringLength(100)] public string CustomerName { get; set; } = "";
  [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
  [Required, RegularExpression(Rules.PhoneRegex, ErrorMessage="Enter a valid phone number.")] public string Phone { get; set; } = "";
  [StringLength(100)] public string? CompanyName { get; set; }
  [StringLength(250)] public string? Address { get; set; }
  [StringLength(60)] public string? City { get; set; }
  [StringLength(60)] public string? State { get; set; }
  public string Status { get; set; } = "Active";
  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  public string? CreatedBy { get; set; }
}
public class Lead : IValidatableObject {
  public int LeadId { get; set; }
  [StringLength(20)] public string LeadCode { get; set; } = "";
  [Required, StringLength(100)] public string LeadName { get; set; } = "";
  [Required, EmailAddress] public string Email { get; set; } = "";
  [Required, RegularExpression(Rules.PhoneRegex, ErrorMessage="Enter a valid phone number.")] public string Phone { get; set; } = "";
  [StringLength(100)] public string? CompanyName { get; set; }
  [StringLength(60)] public string? Source { get; set; }
  [Required] public string Status { get; set; } = "New";
  [Range(0, 100000000)] public decimal ExpectedValue { get; set; }
  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  public string? AssignedTo { get; set; }
  public IEnumerable<ValidationResult> Validate(ValidationContext c) {
    if (!Rules.LeadStatuses.Contains(Status)) yield return new("Invalid lead status.", new[]{nameof(Status)});
  }
}
public class Opportunity : IValidatableObject {
  public int OpportunityId { get; set; }
  [Required, StringLength(150)] public string OpportunityName { get; set; } = "";
  public int CustomerId { get; set; }
  public int? LeadId { get; set; }
  public decimal Amount { get; set; }
  [Required] public string Stage { get; set; } = "Qualification";
  public int Probability { get; set; }
  public DateTime ExpectedCloseDate { get; set; }
  public string Status { get; set; } = "Open";
  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  public string? AssignedTo { get; set; }
  public IEnumerable<ValidationResult> Validate(ValidationContext c) {
    if (!Rules.Stages.Contains(Stage)) yield return new("Invalid stage.", new[]{nameof(Stage)});
    if (Probability < 0 || Probability > 100) yield return new("Probability must be between 0 and 100.", new[]{nameof(Probability)});
    if (Amount < 0) yield return new("Opportunity Amount cannot be negative.", new[]{nameof(Amount)});
    if (Rules.IsActiveStage(Stage)) {
      if (Amount <= 0) yield return new("Opportunity Amount must be greater than 0.", new[]{nameof(Amount)});
      if (ExpectedCloseDate.Date < DateTime.Today) yield return new("Expected Close Date cannot be in the past.", new[]{nameof(ExpectedCloseDate)});
    }
  }
}
public class FollowUp : IValidatableObject {
  public int FollowUpId { get; set; }
  public int? CustomerId { get; set; }
  public int? LeadId { get; set; }
  public DateTime FollowUpDate { get; set; }
  [StringLength(30)] public string FollowUpType { get; set; } = "Call";
  [StringLength(500)] public string? Remarks { get; set; }
  public string Status { get; set; } = "Planned";
  public string? AssignedTo { get; set; }
  public IEnumerable<ValidationResult> Validate(ValidationContext c) {
    if (FollowUpId == 0 && Status == "Planned" && FollowUpDate.Date < DateTime.Today)
      yield return new("Follow-up date cannot be earlier than today.", new[]{nameof(FollowUpDate)});
  }
}
public class Activity {
  public int ActivityId { get; set; }
  [Required] public string ActivityType { get; set; } = "Call";
  [Required, StringLength(150)] public string Subject { get; set; } = "";
  [StringLength(1000)] public string? Description { get; set; }
  public DateTime ActivityDate { get; set; }
  public int? CustomerId { get; set; }
  public int? LeadId { get; set; }
  public string? AssignedTo { get; set; }
  public string Status { get; set; } = "Planned";
}
public class AuditLog {
  public int AuditLogId { get; set; }
  public string? UserId { get; set; }
  public string Action { get; set; } = "";
  public string EntityName { get; set; } = "";
  public string? RecordId { get; set; }
  public string? OldValue { get; set; }
  public string? NewValue { get; set; }
  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  public string? IpAddress { get; set; }
}
