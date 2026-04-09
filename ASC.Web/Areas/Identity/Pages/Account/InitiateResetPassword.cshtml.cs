using ASC.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASC.Web.Areas.Identity.Pages.Account
{
    public class InitiateResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public InitiateResetPasswordModel(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userEmail = HttpContext.User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new
                {
                    userId = user.Id,
                    code = code
                },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                userEmail,
                "Reset Password",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

            return RedirectToPage("/Account/ResetPasswordEmailConfirmation");
        }
    }
}