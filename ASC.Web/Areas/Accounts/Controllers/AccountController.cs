using ASC.Web.Areas.Accounts.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASC.Web.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _mailSender;

       
        private const string EngineerRole = "Engineer";
        private const string CustomerRole = "User";

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender mailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mailSender = mailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Service Engineers

        [HttpGet]
        public async Task<IActionResult> ServiceEngineers()
        {
            HttpContext.Session.SetString("activeItem", "ServiceEngineers");

            var vm = new ServiceEngineerViewModel
            {
                ServiceEngineers = await LoadServiceEngineersAsync(),
                Registration = new ServiceEngineerRegistrationViewModel
                {
                    IsEdit = false,
                    Active = true
                }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceEngineers(ServiceEngineerViewModel serviceEngineers)
        {
            HttpContext.Session.SetString("activeItem", "ServiceEngineers");

            if (!ModelState.IsValid)
            {
                serviceEngineers.ServiceEngineers = await LoadServiceEngineersAsync();
                return View(serviceEngineers);
            }

            var reg = serviceEngineers.Registration;

            if (reg.IsEdit && !string.IsNullOrEmpty(reg.Id))
            {
                var user = await _userManager.FindByIdAsync(reg.Id);
                if (user == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy nhân viên.");
                    serviceEngineers.ServiceEngineers = await LoadServiceEngineersAsync();
                    return View(serviceEngineers);
                }

                user.UserName = reg.Username;
                user.Email = reg.Email;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    serviceEngineers.ServiceEngineers = await LoadServiceEngineersAsync();
                    return View(serviceEngineers);
                }

                if (!string.IsNullOrWhiteSpace(reg.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, reg.Password);

                    if (!passwordResult.Succeeded)
                    {
                        foreach (var error in passwordResult.Errors)
                            ModelState.AddModelError("", error.Description);

                        serviceEngineers.ServiceEngineers = await LoadServiceEngineersAsync();
                        return View(serviceEngineers);
                    }
                }

                await UpsertClaimAsync(user, "Fullname", reg.Fullname ?? "");
                await UpsertClaimAsync(user, "Active", reg.Active.ToString());

                await _mailSender.SendEmailAsync(
                    reg.Email,
                    "Account Update",
                    "Tài khoản của bạn đã được cập nhật."
                );
            }
            else
            {
                if (string.IsNullOrWhiteSpace(reg.Password))
                {
                    ModelState.AddModelError("Registration.Password", "Password is required.");
                    serviceEngineers.ServiceEngineers = await LoadServiceEngineersAsync();
                    return View(serviceEngineers);
                }

                var existedUser = await _userManager.FindByEmailAsync(reg.Email);
                if (existedUser != null)
                {
                    ModelState.AddModelError("", "Email này đã tồn tại.");
                    serviceEngineers.ServiceEngineers = await LoadServiceEngineersAsync();
                    return View(serviceEngineers);
                }

                var user = new IdentityUser
                {
                    UserName = reg.Username,
                    Email = reg.Email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user, reg.Password);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    serviceEngineers.ServiceEngineers = await LoadServiceEngineersAsync();
                    return View(serviceEngineers);
                }

                await _userManager.AddToRoleAsync(user, EngineerRole);
                await _userManager.AddClaimAsync(user, new Claim("Fullname", reg.Fullname ?? ""));
                await _userManager.AddClaimAsync(user, new Claim("Active", reg.Active.ToString()));

                await _mailSender.SendEmailAsync(
                    reg.Email,
                    "Account Register",
                    "Tài khoản Service Engineer của bạn đã được tạo."
                );
            }

            return RedirectToAction(nameof(ServiceEngineers));
        }

        private async Task<List<ServiceEngineerItemViewModel>> LoadServiceEngineersAsync()
        {
            var users = await _userManager.GetUsersInRoleAsync(EngineerRole);
            var result = new List<ServiceEngineerItemViewModel>();

            foreach (var user in users)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var fullname = claims.FirstOrDefault(c => c.Type == "Fullname")?.Value ?? "";
                var activeText = claims.FirstOrDefault(c => c.Type == "Active")?.Value ?? "true";

                bool.TryParse(activeText, out bool activeValue);

                result.Add(new ServiceEngineerItemViewModel
                {
                    Id = user.Id,
                    Username = user.UserName ?? "",
                    Email = user.Email ?? "",
                    Fullname = fullname,
                    Active = activeValue
                });
            }

            return result;
        }

        #endregion

        #region Customers

        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            HttpContext.Session.SetString("activeItem", "Customers");

            var vm = new CustomerViewModel
            {
                Customers = await LoadCustomersAsync(),
                Registration = new CustomerRegistrationViewModel
                {
                    IsEdit = false,
                    IsActive = false
                }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Customers(CustomerViewModel customer)
        {
            HttpContext.Session.SetString("activeItem", "Customers");

            if (!ModelState.IsValid)
            {
                customer.Customers = await LoadCustomersAsync();
                return View(customer);
            }

            
            if (!customer.Registration.IsEdit)
            {
                ModelState.AddModelError("", "Trang Customers không hỗ trợ tạo mới khách hàng.");
                customer.Customers = await LoadCustomersAsync();
                return View(customer);
            }

            if (string.IsNullOrWhiteSpace(customer.Registration.Email))
            {
                ModelState.AddModelError("", "Email khách hàng là bắt buộc.");
                customer.Customers = await LoadCustomersAsync();
                return View(customer);
            }

            var user = await _userManager.FindByEmailAsync(customer.Registration.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Không tìm thấy khách hàng.");
                customer.Customers = await LoadCustomersAsync();
                return View(customer);
            }

            await UpsertClaimAsync(user, "IsActive", customer.Registration.IsActive.ToString());

            if (customer.Registration.IsActive)
            {
                await _mailSender.SendEmailAsync(
                    customer.Registration.Email,
                    "Account Modified",
                    $"Your account has been activated. Email: {customer.Registration.Email}"
                );
            }
            else
            {
                await _mailSender.SendEmailAsync(
                    customer.Registration.Email,
                    "Account Deactivated",
                    "Your account has been deactivated."
                );
            }

            return RedirectToAction(nameof(Customers));
        }

        private async Task<List<CustomerItemViewModel>> LoadCustomersAsync()
        {
            var users = await _userManager.GetUsersInRoleAsync(CustomerRole);
            var result = new List<CustomerItemViewModel>();

            foreach (var user in users)
            {
                var claims = await _userManager.GetClaimsAsync(user);

                
                var isActiveText =
                    claims.FirstOrDefault(c => c.Type == "IsActive")?.Value
                    ?? claims.FirstOrDefault(c => c.Type == "Active")?.Value
                    ?? "false";

                bool.TryParse(isActiveText, out bool isActiveValue);

                result.Add(new CustomerItemViewModel
                {
                    Email = user.Email ?? "",
                    IsActive = isActiveValue
                });
            }

            return result;
        }

        #endregion
        #region Profile

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            HttpContext.Session.SetString("activeItem", "Profile");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(new ProfileModel
            {
                UserName = user.UserName ?? ""
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileModel profile)
        {
            HttpContext.Session.SetString("activeItem", "Profile");

            if (!ModelState.IsValid)
            {
                return View(profile);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var existedUser = await _userManager.FindByNameAsync(profile.UserName);
            if (existedUser != null && existedUser.Id != user.Id)
            {
                ModelState.AddModelError("", "UserName đã tồn tại.");
                return View(profile);
            }

            user.UserName = profile.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(profile);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Cập nhật Profile thành công.";

            return RedirectToAction(nameof(Profile));
        }

        #endregion
        private async Task UpsertClaimAsync(IdentityUser user, string claimType, string claimValue)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            var oldClaim = claims.FirstOrDefault(c => c.Type == claimType);

            if (oldClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, oldClaim);
            }

            await _userManager.AddClaimAsync(user, new Claim(claimType, claimValue));
        }
    }
}