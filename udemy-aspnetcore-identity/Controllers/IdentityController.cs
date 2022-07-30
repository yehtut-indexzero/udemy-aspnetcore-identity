using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using udemy_aspnetcore_identity.Models;
using udemy_aspnetcore_identity.Service;

namespace udemy_aspnetcore_identity.Controllers
{
    public class IdentityController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult SignIn()
        {
            return View(new SignInViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction("MFACheck");
                }
                else
                {
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("Login", "Cannot login.");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            return View(model);

        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, returnUrl);
            var callBackUrl = Url.Action("ExternalLoginCallback");
            properties.RedirectUri = callBackUrl;
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            var emailClaim = info.Principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
            var user = new IdentityUser { Email = emailClaim.Value, UserName = emailClaim.Value };
            await _userManager.CreateAsync(user);
            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, false);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult MFACheck()
        {
            return View(new MNFACheckViewModel());
        }

         [HttpPost]
        public async Task<IActionResult> MFACheck(MNFACheckViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, false, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home", null); 
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Signout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Signin");
        }
        public async Task<IActionResult> SignUp()
        {
            var viewmodel = new SignUpViewModel() { Role = "Member" };
            return View(viewmodel);
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!(await _roleManager.RoleExistsAsync(model.Role)))
                {
                    var role = new IdentityRole() { Name = model.Role };
                    var roleResult = await _roleManager.CreateAsync(role);

                    if (!roleResult.Succeeded)
                    {

                        var errors = roleResult.Errors.Select(x => x.Description);
                        ModelState.AddModelError("Role", string.Join(",", "Role could not be created."));

                        return View(model);
                    }

                }

                if ((await _userManager.FindByEmailAsync(model.Email)) == null)
                {

                    var user = new IdentityUser
                    {
                        UserName = model.Email,
                        Email = model.Email
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    user = await _userManager.FindByEmailAsync(model.Email);

                    if (result.Succeeded)
                    {
                        var claim = new Claim("Department", model.Department);

                        await _userManager.AddClaimAsync(user, claim);

                        await _userManager.AddToRoleAsync(user, model.Role);

                        //var confirmationLink = Url.ActionLink("ConfirmEmail", "Identity", new { userId = user.Id, @token = token });

                        //await _emailSender.SendEmailAsync("yehtut.developer@gmail.com", user.Email, "Confirm your email address", confirmationLink);

                        return RedirectToAction("SignIn");
                    }
                    else
                    {
                        ModelState.AddModelError("SignUp", string.Join("", result.Errors.Select(x => x.Description)));
                        return View(model);
                    }
                }
            }

            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByNameAsync(userId);

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return RedirectToAction("SignIn");
            }
            return new NotFoundResult();
        }

        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> MFASetUp()
        {
            const string provider = "aspnetidentity";


            var user = await _userManager.GetUserAsync(User);


            await _userManager.ResetAuthenticatorKeyAsync(user);

            var token = await _userManager.GetAuthenticatorKeyAsync(user);



            //var qrcodeurl = string.Format("otpauth://totop/{0}:{1}?secret={2}&issuer={3}&digits=6", provider, user.Email, token, provider);
            var qrCodeUrl = $"otpauth://totp/{provider}:{user.Email}?secret={token}&issuer={provider}&digits=6";

            var modle = new MFAViewModel()
            {
                QRCodeUrl = qrCodeUrl,
                Token = token
            };

            return View(modle);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MFASetUp(MFAViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var succeed = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, model.Code);

                if (succeed)
                {
                    await _userManager.SetTwoFactorEnabledAsync(user, true);
                }
                else
                {
                    ModelState.AddModelError("Verifty", "Your MFA Code could not be validated.");
                }

            }
            return View(model);
        }
    }
}
