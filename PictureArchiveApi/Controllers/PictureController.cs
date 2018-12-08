using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using PictureArchiveApi;

namespace Logic.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PictureController : ControllerBase
    {
        private DAL.Picture DalPicture;
        private readonly UserManager<IdentityUser> _userManager;

        public PictureController(
            UserManager<IdentityUser> userManager
        )
        {
            _userManager = userManager;
            DalPicture = new Picture(Startup.ConnectionString, Startup.AzureStorageConnectionString);
        }

        // api/picture
        [Authorize]
        [HttpGet]
        public List<string> Index(int amount = 0, int offset = 0)
        {
            List<string> referencesList = new List<string>();

            // Convert picture element to URL to image
            foreach (MPicture picture in DalPicture.GetPictures(amount, offset))
            {
                referencesList.Add($"/{picture.UserId}/{picture.Id}");
            }

            return referencesList;
        }

        [HttpPost]
        [Route("upload")]
        [Authorize]
        public async Task UploadToBlobAsync()
        {
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

                DalPicture.StorePictureInBlobStorage(files, user.Id);
            }
        }
    }
}