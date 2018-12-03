﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models.User;

namespace Logic.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private User DalUser;

        public UserController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager
        )
        {
            _configuration = configuration;

        DalUser = new User(userManager, signInManager);
        }
        [HttpPost]
        public async Task<object> Login([FromBody] Login model)
        {
            try
            {
                IdentityUser appUser = await DalUser.Login(model);

                var JwtToken = await GenerateJwtToken(model.Email, appUser);

                return "{ " +
                       $" \"token\": \"{JwtToken}\" " +
                       //$", \"role\":  \"{userRole[0]}\" " +
                       "}"; // Return token as JSON
            }
            catch (Exception e)
            {
                return StatusCode(405);

            }
        }

        [HttpPost]
        public async Task<object> Register([FromBody] Login model)
        {
            var user = await DalUser.Register(model);
            
            return await GenerateJwtToken(model.Email, user);
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