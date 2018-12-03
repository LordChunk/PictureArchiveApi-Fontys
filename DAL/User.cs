using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Models.User;

namespace DAL
{
    public class User
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public User(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityUser> Login(Login model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
            IdentityUser appUser;

            if (result.Succeeded)
            {
               appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.Email);
            }
            else
            {
                throw new ApplicationException("INVALID_LOGIN_ATTEMPT");
            }

            return appUser;
        }

        public async Task<IdentityUser> Register(Login model)
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
            }

            return user;
        }
    }
}
