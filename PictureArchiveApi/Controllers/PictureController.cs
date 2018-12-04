using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using PictureArchiveApi;

namespace Logic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PictureController : ControllerBase
    {
        private readonly IHostingEnvironment _environment;
        private DAL.Picture DalPicture;    
        private readonly UserManager<IdentityUser> _userManager;

        public PictureController(
            IHostingEnvironment IHostingEnvironment,
            UserManager<IdentityUser> userManager
            )
        {
            _environment = IHostingEnvironment;
            _userManager = userManager;
            DalPicture = new Picture(Startup.ConnectionString, _environment);
        }


        // api/picture/upload
        [Authorize]
        [Route("upload")]
        [HttpPost]
        public async Task<object> Upload()
        {
            string newFileName = string.Empty;

            if (HttpContext.Request.Form.Files != null)
            {
                IFormFileCollection files = HttpContext.Request.Form.Files;

                string token = Request.Headers.GetCommaSeparatedValues("Authorization").First().Remove(0, 7);

                string email = new JwtSecurityTokenHandler().ReadJwtToken(token).Subject;

                IdentityUser user = new IdentityUser
                {
                    Email = email,
                    UserName = email
                };

                user = await _userManager.GetUserAsync(HttpContext.User);

                await DalPicture.StorePicture(files, user.Id);
            }

            return Ok();
        }
        
        // api/picture
        [Authorize]
        [HttpGet("{id?}")]
        public List<string> Index(int id)
        {
            // Check for invalid data
            if (id <= 0 || id > 100)
            {
                id = 10;
            }

            List<string> referencesList = new List<string>();

            // Convert picture element to URL to image
            foreach (MPicture picture in DalPicture.GetPictures(id))
            {
                referencesList.Add($"/{picture.UserId}/{picture.Id}");
            }
            return referencesList;
        }
    }
}