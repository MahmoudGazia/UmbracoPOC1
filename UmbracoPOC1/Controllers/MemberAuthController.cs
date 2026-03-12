using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;
using UmbracoPOC1.Models;

namespace UmbracoPOC1.Controllers
{
    public class MemberAuthController : SurfaceController
    {
        private readonly IMemberManager _memberManager;
        private readonly IMemberService _memberService;
        private readonly IMemberSignInManager _memberSignInManager;
        private readonly ILogger<MemberAuthController> _logger;

        public MemberAuthController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IMemberManager memberManager,
            IMemberService memberService,
            IMemberSignInManager memberSignInManager,
            ILogger<MemberAuthController> logger)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberManager = memberManager;
            _memberService = memberService;
            _memberSignInManager = memberSignInManager;
            _logger = logger;

            _logger.LogInformation("MemberAuthController instantiated");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["LoginError"] = "Please fill in all required fields correctly.";
                return CurrentUmbracoPage();
            }

            var result = await _memberSignInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                isPersistent: model.RememberMe, 
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                TempData["Success"] = "Login successful! Welcome back.";
                
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                
                return RedirectToCurrentUmbracoPage();
            }

            if (result.IsLockedOut)
            {
                TempData["LoginError"] = "Your account has been locked out due to multiple failed login attempts. Please try again later.";
            }
            else if (result.IsNotAllowed)
            {
                TempData["LoginError"] = "Your account is not approved yet. Please contact support.";
            }
            else
            {
                TempData["LoginError"] = "Invalid email or password. Please try again.";
            }

            return CurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleRegister(RegisterViewModel model)
        {
            _logger.LogInformation("HandleRegister called - Email: {Email}", model?.Email ?? "null");

            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    TempData["RegisterError"] = $"Validation failed: {string.Join(", ", errors)}";
                    return CurrentUmbracoPage();
                }

                // Check if member already exists
                var existingMember = _memberService.GetByEmail(model.Email);
                if (existingMember != null)
                {
                    TempData["RegisterError"] = "An account with this email address already exists.";
                    return CurrentUmbracoPage();
                }

                // Create new member
                var memberIdentity = MemberIdentityUser.CreateNew(
                    model.Email,
                    model.Email,
                    "registeredMember", // Member type alias
                    true, // isApproved
                    model.Email // name
                );

                var createResult = await _memberManager.CreateAsync(
                    memberIdentity,
                    model.Password
                );

                if (createResult.Succeeded)
                {
                    // Get the created member to set additional properties
                    var member = _memberService.GetByEmail(model.Email);
                    if (member != null)
                    {
                        member.SetValue("firstName", model.FirstName);
                        member.SetValue("lastName", model.LastName);

                        if (!string.IsNullOrEmpty(model.PhoneNumber))
                        {
                            member.SetValue("phoneNumber", model.PhoneNumber);
                        }

                        _memberService.Save(member);
                    }

                    // Automatically sign in the new member
                    await _memberSignInManager.SignInAsync(memberIdentity, isPersistent: false);

                    TempData["Success"] = $"Welcome, {model.FirstName}! Your account has been created successfully.";
                    return RedirectToCurrentUmbracoPage();
                }

                // If we got here, something went wrong
                var createErrors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                TempData["RegisterError"] = $"Registration failed: {createErrors}";
                return CurrentUmbracoPage();
            }
            catch (Exception ex)
            {
                TempData["RegisterError"] = $"An error occurred: {ex.Message}";
                return CurrentUmbracoPage();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleLogout()
        {
            await _memberSignInManager.SignOutAsync();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToCurrentUmbracoPage();
        }
    }
}
