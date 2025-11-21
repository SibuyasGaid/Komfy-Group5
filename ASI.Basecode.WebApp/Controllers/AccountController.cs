using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AccountController : ControllerBase<AccountController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public AccountController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            IEmailService emailService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
            this._emailService = emailService;
        }

        /// <summary>
        /// Login Method
        /// </summary>
        /// <returns>Created response view</returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            TempData["returnUrl"] = System.Net.WebUtility.UrlDecode(HttpContext.Request.Query["ReturnUrl"]);
            this._sessionManager.Clear();
            this._session.SetString("SessionId", System.Guid.NewGuid().ToString());
            return this.View();
        }

        /// <summary>
        /// Authenticate user and signs the user in when successful.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns> Created response view </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            this._session.SetString("HasSession", "Exist");

            User user = null;

            var loginResult = _userService.AuthenticateUser(model.UserId, model.Password, ref user);
            if (loginResult == LoginResult.Success)
            {
                // Authentication successful
                await this._signInManager.SignInAsync(user);
                this._session.SetString("UserName", user.Name);

                // Redirect to returnUrl if valid, otherwise to Home
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Authentication failed - add model-level error and return view with model so UI shows inline messages
                ModelState.AddModelError(string.Empty, "Incorrect username or password");
                // keep TempData for backward-compatible toast systems or JS handlers
                TempData["ErrorMessage"] = "Incorrect username or password";
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register(UserViewModel model)
        {
            // Validate server-side constraints (e.g., MinLength attributes)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _userService.AddUser(model);
                return RedirectToAction("Login", "Account");
            }
            catch (InvalidDataException ex)
            {
                // Map known invalid-data errors to field-level validation so the UI shows inline errors
                var msg = ex.Message ?? "";

                if (msg.IndexOf("email", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ModelState.AddModelError(nameof(model.Email), msg);
                }
                else if (msg.Equals(Resources.Messages.Errors.UserExists, System.StringComparison.OrdinalIgnoreCase) ||
                         msg.IndexOf("userid", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ModelState.AddModelError(nameof(model.UserId), msg);
                }
                else
                {
                    // General model error
                    ModelState.AddModelError(string.Empty, msg);
                }

                // Return the same model so field-level messages render in the view
                return View(model);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
            }

            return View(model);
        }

        // AJAX endpoint: check if email is available (used by client-side registration UX)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult IsEmailAvailable(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { available = false });
            }

            try
            {
                // Service returns null when not found
                var existing = _userService.GetUserByEmail(email);
                return Json(new { available = existing == null });
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error checking email availability");
                return Json(new { available = false });
            }
        }

        // AJAX endpoint: check if username is available (used by client-side registration UX)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult IsUsernameAvailable(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Json(new { available = false });
            }

            try
            {
                var existing = _userService.GetUserDetails(username);
                return Json(new { available = existing == null });
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error checking username availability");
                return Json(new { available = false });
            }
        }

        /// <summary>
        /// Sign Out current account and return login view.
        /// </summary>
        /// <returns>Created response view</returns>
        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            this._sessionManager.Clear();
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Handles user logout by clearing authentication and session data.
        /// Redirects to the landing page after successful logout.
        /// </summary>
        /// <returns>Redirect to landing page</returns>
        public async Task<IActionResult> Logout()
        {
            await this._signInManager.SignOutAsync();
            this._sessionManager.Clear();
            // Redirect to landing page without setting TempData to prevent message persistence
            return RedirectToAction("Landing", "Home");
        }

        /// <summary>
        /// Access Denied - Show when user doesn't have permission
        /// </summary>
        /// <returns>Access denied view</returns>
        [HttpGet]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Fix default user passwords after migration on new device
        /// Navigate to /Account/FixPasswords to re-encrypt admin and user passwords
        /// This is needed when running migrations on a new device due to encryption key differences
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult FixPasswords()
        {
            try
            {
                // Get the default seeded users
                var adminUser = _userService.GetUserDetails("admin");
                var regularUser = _userService.GetUserDetails("user");

                if (adminUser != null)
                {
                    // Set plain text password, UpdateUser will encrypt it with the current environment's key
                    adminUser.Password = "admin";
                    _userService.UpdateUser(adminUser);
                }

                if (regularUser != null)
                {
                    // Set plain text password, UpdateUser will encrypt it with the current environment's key
                    regularUser.Password = "user";
                    _userService.UpdateUser(regularUser);
                }

                return Content("✅ Passwords fixed successfully!\n\nYou can now login with:\n- Username: admin, Password: admin\n- Username: user, Password: user\n\nGo to /Account/Login to sign in.");
            }
            catch (Exception ex)
            {
                return Content($"❌ Error fixing passwords: {ex.Message}\n\nPlease check that the database has been migrated and contains the default admin and user accounts.");
            }
        }

        // CRITICAL FEATURE #1: Forgot Password - Display form
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // CRITICAL FEATURE #1: Forgot Password - Process form
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                var token = _userService.GeneratePasswordResetToken(email);

                // Generate password reset URL
                var resetUrl = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);

                // Try to send email, but fallback to showing token if email fails
                try
                {
                    await _emailService.SendPasswordResetEmailAsync(email, token, resetUrl);
                    TempData["SuccessMessage"] = "Password reset link has been sent to your email.";
                }
                catch (Exception emailEx)
                {
                    // Email failed, but show token for development/testing
                    _logger.LogError(emailEx, "Failed to send email. Falling back to showing token.");
                    TempData["SuccessMessage"] = "Email service unavailable. Use the link below to reset your password.";
                    TempData["ResetToken"] = token;
                    TempData["ErrorMessage"] = $"Email Error: {emailEx.Message}";
                }

                return View("ForgotPasswordConfirmation");
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                _logger.LogError(ex, "Error generating password reset token.");
                return View();
            }
        }

        // CRITICAL FEATURE #1: Reset Password - Display form
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction("Login");
            }

            // Validate token
            if (!_userService.ValidatePasswordResetToken(token))
            {
                TempData["ErrorMessage"] = "Invalid or expired password reset token.";
                return RedirectToAction("Login");
            }

            ViewBag.Token = token;
            return View();
        }

        // CRITICAL FEATURE #1: Reset Password - Process form
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                TempData["ErrorMessage"] = "Password is required.";
                ViewBag.Token = token;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match.";
                ViewBag.Token = token;
                return View();
            }

            try
            {
                _userService.ResetPassword(token, newPassword);
                TempData["SuccessMessage"] = "Password reset successfully. Please login with your new password.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                _logger.LogError(ex, "Error resetting password.");
                ViewBag.Token = token;
                return View();
            }
        }

        // CRITICAL FEATURE #4: Edit Profile - Display form
        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "You must be logged in to edit your profile.";
                return RedirectToAction("Login");
            }

            var user = _userService.GetUserDetails(userId);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // CRITICAL FEATURE #4: Edit Profile - Process form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(string name, string email)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "You must be logged in to edit your profile.";
                return RedirectToAction("Login");
            }

            try
            {
                _userService.UpdateUserProfile(userId, name, email);
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction("EditProfile");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                _logger.LogError(ex, "Error updating profile.");

                var user = _userService.GetUserDetails(userId);
                return View(user);
            }
        }

        // CRITICAL FEATURE #4: Change Password - Display form
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // CRITICAL FEATURE #4: Change Password - Process form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "You must be logged in to change your password.";
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                TempData["ErrorMessage"] = "New password is required.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation do not match.";
                return View();
            }

            try
            {
                _userService.ChangePassword(userId, currentPassword, newPassword);
                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction("EditProfile");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                _logger.LogError(ex, "Error changing password.");
                return View();
            }
        }
    }
}
