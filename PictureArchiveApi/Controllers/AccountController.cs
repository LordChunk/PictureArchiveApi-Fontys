using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models.User;

namespace Logic.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(
            IConfiguration configuration,
            UserManager<IdentityUser> userManager
        )
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<object> Login([FromBody] ILogin model)
        {
            var user = new User();

            var result = await user.Login(model);

            if (result.Succeeded)
            {
                var JwtToken = await GenerateJwtToken(model.Email, appUser);

                //var userRole = await _userManager.GetRolesAsync(appUser);

                return  "{" +
                            $" \"token\": \"{JwtToken}\" " +
                            //$", \"role\":  \"{userRole[0]}\" " +
                        "}"; // Return token as JSON
            }

            throw new ApplicationException("INVALID_LOGIN_ATTEMPT");
        }

        private async Task<object> GenerateJwtToken(string email, IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}