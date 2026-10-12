using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace AcxiomCRM.Controllers;

public class LoginVm { [Required, EmailAddress] public string Email { get; set; } = ""; [Required, DataType(DataType.Password)] public string Password { get; set; } = ""; }
public class RegisterVm : LoginVm { [Required, StringLength(100)] public string FullName { get; set; } = ""; [Compare(nameof(Password))] public string ConfirmPassword { get; set; } = ""; }

[AllowAnonymous]
public class AccountController : Controller {
  private readonly SignInManager<ApplicationUser> _sm; private readonly UserManager<ApplicationUser> _um; private readonly IAuditService _audit;
  public AccountController(SignInManager<ApplicationUser> sm, UserManager<ApplicationUser> um, IAuditService a) { _sm = sm; _um = um; _audit = a; }

  [HttpGet] public IActionResult Login() => View();
  [HttpPost] public async Task<IActionResult> Login(LoginVm vm) {
    if (!ModelState.IsValid) return View(vm);
    var u = await _um.FindByEmailAsync(vm.Email);
    if (u == null || !u.IsActive) { await _audit.LogAsync("FailedLogin", "Account", null, null, vm.Email); ModelState.AddModelError("", "Invalid login attempt."); return View(vm); }
    var r = await _sm.PasswordSignInAsync(u, vm.Password, false, lockoutOnFailure: true);
    if (r.Succeeded) { await _audit.LogAsync("Login", "Account", u.Id, userId: u.Id); return RedirectToAction("Index", "Dashboard"); }
    await _audit.LogAsync(r.IsLockedOut ? "Lockout" : "FailedLogin", "Account", u.Id, userId: u.Id);
    ModelState.AddModelError("", r.IsLockedOut ? "Account locked. Try again later." : "Invalid login attempt.");
    return View(vm);
  }
  [HttpGet] public IActionResult Register() => View();
  [HttpPost] public async Task<IActionResult> Register(RegisterVm vm) {
    if (!ModelState.IsValid) return View(vm);
    var u = new ApplicationUser { UserName = vm.Email, Email = vm.Email, FullName = vm.FullName };
    var r = await _um.CreateAsync(u, vm.Password);
    if (!r.Succeeded) { foreach (var e in r.Errors) ModelState.AddModelError("", e.Description); return View(vm); }
    await _um.AddToRoleAsync(u, Roles.Sales); // self-registration is always least privilege
    await _audit.LogAsync("Register", "Account", u.Id, userId: u.Id);
    await _sm.SignInAsync(u, false);
    return RedirectToAction("Index", "Dashboard");
  }
  [HttpPost, Authorize] public async Task<IActionResult> Logout() { await _audit.LogAsync("Logout", "Account"); await _sm.SignOutAsync(); return RedirectToAction(nameof(Login)); }
  public IActionResult Denied() => View();
}
