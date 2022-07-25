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

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Username);

                   //var userClaims =await _userManager.GetClaimsAsync(user);

                   // if (!userClaims.Any(x=>x.Type=="Department"))
                   // {
                   //     ModelState.AddModelError("Claim","User is not from Tech Department");
                   //     return View(model);
                   // }

                    if (await _userManager.IsInRoleAsync(user, "Member"))
                    {
                        return RedirectToAction("Member", "Home");
                    }

                    //return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Login", "Cannot login.");
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
    }
}
