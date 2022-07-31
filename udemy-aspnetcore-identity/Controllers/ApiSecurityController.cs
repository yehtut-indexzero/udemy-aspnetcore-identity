using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using udemy_aspnetcore_identity.Models;
using Microsoft.IdentityModel.Tokens;

namespace udemy_aspnetcore_identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiSecurityController : ControllerBase
    {
        public ApiSecurityController(IConfiguration configuration, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        [AllowAnonymous]
        [Route(template:"Auth")]
        [HttpPost]
        public async Task<IActionResult> TokenAuth(SignInViewModel model)
        {
            var issuers = _configuration["Tokens:Issuer"];
            var audience = _configuration["Tokens:Audience"];
            var key = _configuration["Tokens:Key"];

            if (ModelState.IsValid)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);

                if (signInResult.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Username);

                    if (user != null)
                    {
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Email,user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti,user.Id)
                        };

                        var keyBytes = Encoding.UTF8.GetBytes(key);

                        var theKey = new SymmetricSecurityKey(keyBytes);

                        var creds = new SigningCredentials(theKey, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(issuers, audience, claims, expires: DateTime.Now.AddMinutes(30), signingCredentials: creds);

                        return Ok(new { token=new JwtSecurityTokenHandler().WriteToken(token)});
                    }
                }
            }

            return BadRequest();
        }
    }
}
